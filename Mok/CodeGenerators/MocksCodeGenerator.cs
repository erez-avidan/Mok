using Generators;
using Microsoft.CodeAnalysis;
using Mok.Contracts;
using MokMock.CodeGenerators;
using MokMock.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mok.CodeGenerators
{
    internal class MocksCodeGenerator
    {
        internal static List<MockFile> GenerateMocks(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            var models = CreateModels(context, receiver);
            CreateCode(context, models);

            return models.Select(m => new MockFile { Name = m.Name, Namespace = m.Namespace }).ToList();
        }

        private static void CreateCode(GeneratorExecutionContext context, List<ClassModel> models)
        {
            var classGen = new ClassGenerator(new MethodGenerator(), new PropertyGenerator());
            foreach (var model in models)
            {
                context.AddSource(model.Name + "_Mock.g.cs", classGen.Generate(model));
            }
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
                            (m.IsAbstract || m.IsVirtual || m.IsOverride))
                        .Select(m =>
                        {
                            var isProperty = m.MethodKind == MethodKind.PropertySet || m.MethodKind == MethodKind.PropertyGet;
                            var name = m.Name;
                            if (isProperty)
                            {
                                name += "_Mock";
                            }
                            return new MethodModel
                            {
                                Name = name,
                                ReturnType = m.ReturnType.ToDisplayString(),
                                IsPrivate = isProperty,
                                Parameters = m.Parameters.Select(p => new ParameterModel
                                {
                                    Name = p.Name,
                                    Type = p.Type.ToDisplayString(),
                                }).ToList(),
                            };
                        }).ToList(),
                    Properties = symbol.GetMembers().OfType<IPropertySymbol>()
                        .Select(m => new PropertyModel
                        {
                            Name = m.Name,
                            Type = m.Type.ToDisplayString(),
                            Get = m.GetMethod != null,
                            Set = m.SetMethod != null,
                        }).ToList(),
                };

                classes.Add(classModel);
            }

            return classes;
        }
    }
}
