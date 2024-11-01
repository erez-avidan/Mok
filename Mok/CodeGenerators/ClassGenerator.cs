using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MokMock.Models;
using System.Text;

namespace MokMock.CodeGenerators
{
    internal class ClassGenerator(MethodGenerator methodGenerator, PropertyGenerator propertyGenerator)
    {
        public string Generate(ClassModel classModel)
        {
            var builder = new StringBuilder();

            builder.AppendLine("");
            builder.Append(@$"
using MokMock;
namespace MokMock.Generated;
internal class {classModel.MockName} : {classModel.FullNamespace} {{
    private MockHandler handler;

        public {classModel.MockName}(MockHandler handler) {{
            this.handler = handler;        
}}");

            foreach (var property in classModel.Properties)
            {
                propertyGenerator.Generate(property, builder);
            }

            foreach (var method in classModel.Methods)
            {
                methodGenerator.Generate(method, builder);
            }
            
            builder.AppendLine("}");

            var fullCode = builder.ToString();
            fullCode = CSharpSyntaxTree.ParseText(fullCode).GetRoot().NormalizeWhitespace().ToFullString();

            return fullCode;
        }
    }
}
