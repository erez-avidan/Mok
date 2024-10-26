using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MokMock.CodeGenerators;
using MokMock.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generators
{
    [Generator]
    internal class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var models = CreateModels(context, receiver);
            var sources = CreateCode(models);

            sources.ForEach(s =>
            {
                context.AddSource(s.Name, s.SourceCode);
            });

            GenerateStaticFiles(context);
            CreateMocksFactory(models, context);
        }

        private void GenerateStaticFiles(GeneratorExecutionContext context)
        {
           LibGen.Generate(context);

        }

        private void CreateMocksFactory(List<ClassModel> models, GeneratorExecutionContext context)
        {
            var sb = new StringBuilder();
            foreach (var model in models)
            {
                sb.AppendLine($"{{ \"{model.Namespace}.{model.Name}\", (handler) => new MokMock.Generated.{model.Name}_Mock(handler) }},");
            }

            var str = $@"
using System.Linq.Expressions;
namespace MokMock {{

    public interface IMock<T> {{
        ISetupAction Setup(Expression<Action<T>> expression);
        ISetupFunc<TResult> Setup<TResult>(Expression<Func<T, TResult>> expression);
        T Object {{ get;}}
    }}

    public class Mok<T> : IMock<T>
    {{
        private static Dictionary<string, Func<MockHandler, object>> initializers = new Dictionary<string, Func<MockHandler, object>>
        {{
            {sb}
        }};

        private MockHandler handler = new MockHandler();
        private T? value = default;

        public Mok() {{ }}

        public T Object 
            {{ 
                get
                {{
                    if (value == null)
                    {{
                        Type type = typeof(T);
                        value = (T)initializers[type.ToString()](handler);
                    }}
                    return value;
                }} 
            }}

        public ISetupAction Setup(Expression<Action<T>> expression)
        {{
            if (expression.Body is not MethodCallExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            IEnumerable<IMatcher> matchers = FetchParameters(body);
            var setup = new VoidSetup(matchers);
            AddSetup(body, setup);
            return setup;
        }}

        public ISetupFunc<TResult> Setup<TResult>(Expression<Func<T, TResult>> expression)
        {{
            if (expression.Body is not MethodCallExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            IEnumerable<IMatcher> matchers = FetchParameters(body);
            var setup = new Setup<TResult>(matchers);
            AddSetup(body, setup);
            return setup;
        }}

        private void AddSetup(MethodCallExpression body, ISetup setup)
        {{
            handler.AddSetup(body, setup);
        }}

        private static IMatcher[] FetchParameters(MethodCallExpression body)
        {{
            return body.Arguments.Select(arg => MatcherFactory.GetMatcher(arg)).ToArray();
        }}

        public void Verify(Expression<Action<T>> expression, Times times)
        {{
            if (expression.Body is not MethodCallExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            var matchers = FetchParameters(body);

            int count = 0;
            if (handler.calls.TryGetValue(body.Method.Name, out var calls))
            {{
                foreach (var callParams in calls)
                {{
                    if (callParams.Length == matchers.Length)
                    {{
                        int i = 0;
                        if (matchers.All(m => m.IsMatching(callParams[i++])))
                        {{
                            count++;
                        }}
                    }}
                }}
            }}

            if (times.ExpectedCalls != count)
            {{
                throw new IncorrectNumberOfCallsException($""Calls to method \""{{body.Method.Name}}\"" expected:{{times.ExpectedCalls}}, but found:{{count}}"");
            }}
        }}
    }}
}}";
            context.AddSource("MockT.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private List<CodeFile> CreateCode(List<ClassModel> models)
        {
            List<CodeFile> classes = [];
            var classGen = new ClassGenerator(new MethodGenerator());
            foreach (var model in models)
            {
                classes.Add(new CodeFile
                {
                    Name = model.Name +"_Mock.g.cs",
                    SourceCode = classGen.Generate(model)
                });
            }

            return classes;
        }

        private static List<ClassModel> CreateModels(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            List<ClassModel> classes = [];

            foreach (var type in receiver.typesToMock)
            {
                var symbol = context.Compilation.GetSemanticModel(type.Value.SyntaxTree)
                    .GetSymbolInfo(type.Value).Symbol as INamedTypeSymbol;
                if (symbol.IsSealed)
                {
                    // error
                    continue;
                }

                var classModel = new ClassModel
                {
                    Name = symbol.Name,
                    Namespace = symbol.ContainingNamespace.ToDisplayString(),
                    Methods = symbol.GetMembers().OfType<IMethodSymbol>().Where(m =>
                            (m.IsAbstract || m.IsVirtual || m.IsOverride)
                            && m.MethodKind != MethodKind.PropertySet)
                    .Select(m => new MethodModel
                    {
                        Name = m.Name,
                        ReturnType = m.ReturnType.ToDisplayString(),
                        Parameters = m.Parameters.Select(p => new ParameterModel
                        {
                            Name = p.Name,
                            Type = p.Type.ToDisplayString(),
                        }).ToList(),
                    }).ToList()
                };

                classes.Add(classModel);
            }

            return classes;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }
}