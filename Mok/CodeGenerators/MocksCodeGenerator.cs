using Generators;
using Microsoft.CodeAnalysis;
using Mok.Contracts;
using Mok.Models;
using MokMock.CodeGenerators;
using MokMock.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mok.CodeGenerators
{
    internal class MocksCodeGenerator
    {
        internal static List<MockFile> GenerateMocks(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            var classGen = new ClassGenerator(new MethodGenerator(), new PropertyGenerator());
            var models = CreateModels(context, receiver);

            return models.Select(m =>
            {
                string mockName = GetMockName(m);
                string classToString = GetClassToString(m);

                var generatedClassCode = classGen.Generate(m);

                context.AddSource($"{mockName}.g.cs", generatedClassCode);

                return new MockFile { ClassName = classToString, MockName = mockName };
            }).ToList();
        }

        private static string GetClassToString(ClassModel m)
        {
            var classToString = $"{m.Namespace}.{m.ClassName}";

            if (m.TypeArguments.Any())
            {
                classToString += $"`{m.TypeArguments.Length}[{string.Join(",", m.TypeArguments.Select(ta => ta.Namespace + '.' + ta.Name))}]";
            }

            return classToString;
        }

        private static string GetMockName(ClassModel m)
        {
            var mockName = m.ClassName;
            if (m.TypeArguments.Any())
            {
                mockName += "_" + string.Join("_", m.TypeArguments.Select(ta => ta.Name));
            }
            mockName += "_Mock";
            return mockName;
        }

        private static List<ClassModel> CreateModels(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            Dictionary<string, ClassModel> classes = [];

            foreach (var type in receiver.typesToMock)
            {
                var symbol = context.Compilation.GetSemanticModel(type.Value.SyntaxTree)
                    .GetSymbolInfo(type.Value).Symbol as INamedTypeSymbol;

                CreateClassModel(symbol, classes, context);
            }

            return [.. classes.Values];
        }

        private static void CreateClassModel(INamedTypeSymbol symbol, Dictionary<string, ClassModel> classes, GeneratorExecutionContext context)
        {
            var containingNamespace = symbol.ContainingNamespace.Name;
            var name = symbol.Name;
            string fullNamespace = GetClassKey(containingNamespace, name, symbol.TypeArguments);

            if (classes.ContainsKey(fullNamespace))
            {
                return;
            }

            if (symbol.IsSealed)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("MOK001",
                    "Mock of sealed class",
                    "Class {0} cannot be mocked because it is sealed",
                    "generator",
                    DiagnosticSeverity.Error,
                    true), symbol.Locations.FirstOrDefault(), symbol.Name));
            }

            foreach (var item in symbol.AllInterfaces)
            {
                CreateClassModel(item, classes, context);
            }

            var classModel = new ClassModel
            {
                Namespace = symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name,
                TypeArguments = symbol.TypeArguments.Select(ta => new TypeModel { Namespace = ta.ContainingNamespace.Name, Name = ta.Name }).ToArray(),
                Methods = GetMethods(symbol),
                Properties = GetProperties(symbol),
                Inherited = symbol.AllInterfaces.Select(inherited => classes[GetClassKey(inherited.ContainingNamespace.Name, inherited.Name, inherited.TypeArguments)])
            };
            classes[fullNamespace] = classModel;
        }

        private static string GetClassKey(string containingNamespace, string name, ImmutableArray<ITypeSymbol> typeArguments)
        { 
            var fullNamespace = $"{containingNamespace}.{name}";
            var typeArgs = typeArguments.Select(ta => new TypeModel { Namespace = ta.ContainingNamespace.Name, Name = ta.Name });
            if (typeArgs.Any())
            {
                foreach (var type in typeArgs)
                {
                    fullNamespace += "_" + type.Name;
                }
            }

            return fullNamespace;
        }

        private static List<PropertyModel> GetProperties(INamedTypeSymbol symbol)
        {
            var properties = new List<PropertyModel>();
            var members = symbol.GetMembers().OfType<IPropertySymbol>();

            foreach (var m in members)
            {
                properties.Add(new PropertyModel
                {
                    Name = m.Name,
                    Type = m.Type.ToDisplayString(),
                    Get = m.GetMethod != null,
                    Set = m.SetMethod != null,
                });
            }

            return properties;
        }

        private static List<MethodModel> GetMethods(INamedTypeSymbol symbol)
        {
            var models = new List<MethodModel>();
            var methods = symbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.IsAbstract || m.IsVirtual || m.IsOverride);

            foreach (var m in methods)
            {
                var isProperty = m.MethodKind == MethodKind.PropertySet || m.MethodKind == MethodKind.PropertyGet;
                var name = m.Name;
                if (isProperty)
                {
                    name += "_Mock";
                }
                models.Add(new MethodModel
                {
                    Name = name,
                    ReturnType = m.ReturnType.ToDisplayString(),
                    IsPrivate = isProperty,
                    IsGeneric = m.IsGenericMethod,
                    GenericTypes = m.TypeArguments.Select(t => t.Name),
                    Parameters = m.Parameters.Select(p => new ParameterModel
                    {
                        Name = p.Name,
                        Type = p.Type.ToDisplayString(),
                    }).ToList(),
                });
            }

            return models;
        }
    }
}
