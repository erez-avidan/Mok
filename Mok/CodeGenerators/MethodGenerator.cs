using MokMock.Models;
using System.Linq;
using System.Text;

namespace MokMock.CodeGenerators
{
    internal class MethodGenerator
    {
        const string TASK_PREFIX = "System.Threading.Tasks.Task<";

        public void Generate(MethodModel model, StringBuilder builder)
        {
            builder.Append($@"public {model.ReturnType} {model.Name} (");

            if (model.Parameters?.Any() == true)
            {
                var last = model.Parameters.Last();

                model.Parameters.ForEach((para) =>
                {
                    builder.Append(para.Type);
                    builder.Append(" ");
                    builder.Append(para.Name);

                    if (last != para)
                    {
                        builder.Append(", ");
                    }
                });
            }
            builder.AppendLine(") {");

            if (model.ReturnType == "void")
            {
                builder.AppendLine(@$"handler.CallVoid(""{model.Name}"", new object[] {{");
                model.Parameters.ForEach((para) =>
                {
                    builder.Append(para.Name);
                        builder.Append(", ");
                });

                builder.AppendLine("});");
            } 
            else if (model.ReturnType == "System.Threading.Tasks.Task")
            {
                builder.AppendLine(@$"return handler.CallVoidAsync(""{model.Name}"", new object[] {{");
                model.Parameters.ForEach((para) =>
                {
                    builder.Append(para.Name);
                    builder.Append(", ");
                });

                builder.AppendLine("});");
            }
            else if (model.ReturnType.StartsWith(TASK_PREFIX))
            {
                var innerType = model.ReturnType.Substring(TASK_PREFIX.Length, (model.ReturnType.Length - TASK_PREFIX.Length) - 1);
                
                builder.AppendLine(@$"return handler.CallReturnValueAsync<{innerType}>(""{model.Name}"", new object[] {{");
                model.Parameters.ForEach((para) =>
                {
                    builder.Append(para.Name);
                    builder.Append(", ");
                });

                builder.AppendLine("});");
            }
            else 
            {
                builder.AppendLine(@$"return handler.CallReturnValue<{model.ReturnType}>(""{model.Name}"", new object[] {{");
                model.Parameters.ForEach((para) =>
                {
                    builder.Append(para.Name);
                    builder.Append(", ");
                });

                builder.AppendLine("});");
            }

            builder.AppendLine("}");

        }
    }
}
