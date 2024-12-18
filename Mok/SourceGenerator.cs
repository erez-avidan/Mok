﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mok.CodeGenerators;
using Mok.Contracts;
using System.Collections.Generic;
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

            StaticCodeGenerator.Generate(context);
            List<MockFile> mocks = MocksCodeGenerator.GenerateMocks(context, receiver);
            CreateMocksFactory(mocks, context);
        }

       

        private void CreateMocksFactory(List<MockFile> models, GeneratorExecutionContext context)
        {
            var sb = new StringBuilder();
            foreach (var model in models)
            {
                sb.AppendLine($"{{ \"{model.ClassName}\", (handler) => new MokMock.Generated.{model.MockName}(handler) }},");
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

        public ISetupFunc<TResult> SetupGet<TResult>(Expression<Func<T, TResult>> expression)
        {{
            if (expression.Body is not MemberExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            var setup = new Setup<TResult>([]);
            handler.AddSetup($""get_{{body.Member.Name}}_Mock"", setup);
            return setup;
        }}

        public ISetupAction SetupSet<TResult>(Expression<Func<T, TResult>> expression, Expression<Func<TResult>> value)
        {{
            if (expression.Body is not MemberExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            IEnumerable<IMatcher> matchers = [MatcherFactory.GetMatcher(value.Body)];

            var setup = new VoidSetup(matchers);
            handler.AddSetup($""set_{{body.Member.Name}}_Mock"", setup);
            return setup;
        }}

        private void AddSetup(MethodCallExpression body, ISetup setup)
        {{
            handler.AddSetup(body.Method.Name, setup);
        }}

        private static IMatcher[] FetchParameters(MethodCallExpression body)
        {{
            return body.Arguments.Select(arg => MatcherFactory.GetMatcher(arg)).ToArray();
        }}

        public void VerifyGet<TResult>(Expression<Func<T, TResult>> expression, Times times)
        {{
            if (expression.Body is not MemberExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            int count = VerifyInternal(times, $""get_{{body.Member.Name}}_Mock"", []);

            if (times.ExpectedCalls != count)
            {{
                throw new IncorrectNumberOfCallsException($""Calls to property \""{{body.Member.Name}}\"" expected:{{times.ExpectedCalls}}, but found:{{count}}"");
            }}
        }}

        public void VerifySet<TResult>(Expression<Func<T, TResult>> expression, Expression<Func<TResult>> value, Times times)
        {{
            if (expression.Body is not MemberExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            IMatcher[] matchers = [MatcherFactory.GetMatcher(value.Body)];
            int count = VerifyInternal(times, $""set_{{body.Member.Name}}_Mock"", matchers);

            if (times.ExpectedCalls != count)
            {{
                throw new IncorrectNumberOfCallsException($""Calls to property \""{{body.Member.Name}}\"" expected:{{times.ExpectedCalls}}, but found:{{count}}"");
            }}
        }}

        public void Verify(Expression<Action<T>> expression, Times times)
        {{
            if (expression.Body is not MethodCallExpression body)
            {{
                throw new NotSupportedException(""expression not supported for Setup"");
            }}

            var matchers = FetchParameters(body);
            int count = VerifyInternal(times, body.Method.Name, matchers);

            if (times.ExpectedCalls != count)
            {{
                throw new IncorrectNumberOfCallsException($""Calls to method \""{{body.Method.Name}}\"" expected:{{times.ExpectedCalls}}, but found:{{count}}"");
            }}
        }}

        private int VerifyInternal(Times times, string name, IMatcher[] matchers)
        {{
            int count = 0;

            if (handler.calls.TryGetValue(name, out var calls))
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

            return count;
        }}
    }}
}}";
            context.AddSource("MockT.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }


        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }
}