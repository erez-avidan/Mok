using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MokMock.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MokMock.CodeGenerators
{
    internal class ClassGenerator(MethodGenerator methodGenerator, PropertyGenerator propertyGenerator)
    {
        public string Generate(ClassModel classModel)
        {
            var className = classModel.ClassName + '_';
            var fullNamespace = $"{classModel.Namespace}.{classModel.ClassName}";

            if (classModel.TypeArguments.Any())
            {
                className += string.Join("_", classModel.TypeArguments.Select(ta => ta.Name)) + '_';
                fullNamespace += $"<{string.Join(",", classModel.TypeArguments.Select(ta => ta.Namespace + '.' + ta.Name))}>";

            }

            className += "Mock";

            var builder = new StringBuilder();

            builder.AppendLine("");
            builder.Append(@$"
using MokMock;
namespace MokMock.Generated;
internal class {className} : {fullNamespace} {{
    private MockHandler handler;

        public {className}(MockHandler handler) {{
            this.handler = handler;        
}}");

            var signaturesSet = new HashSet<string>();

            GenerateMethodsAndProperties(methodGenerator, propertyGenerator, classModel, builder, signaturesSet);

            foreach (var inherited in classModel.Inherited)
            {
                GenerateMethodsAndProperties(methodGenerator,propertyGenerator, inherited, builder, signaturesSet);
            }

            builder.AppendLine("}");

            var fullCode = builder.ToString();
            fullCode = CSharpSyntaxTree.ParseText(fullCode).GetRoot().NormalizeWhitespace().ToFullString();

            return fullCode;
        }

        private static void GenerateMethodsAndProperties(MethodGenerator methodGenerator, 
            PropertyGenerator propertyGenerator,
            ClassModel classModel,
            StringBuilder builder,
            HashSet<string> signaturesSet)
        {
            foreach (var property in classModel.Properties)
            {
                propertyGenerator.Generate(property, builder, signaturesSet);
            }

            foreach (var method in classModel.Methods)
            {
                methodGenerator.Generate(method, builder, signaturesSet);
            }
        }
    }
}
