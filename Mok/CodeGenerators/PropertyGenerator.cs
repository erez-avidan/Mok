using MokMock.Models;
using System;
using System.Reflection;
using System.Text;

namespace MokMock.CodeGenerators
{
    internal class PropertyGenerator
    {
        internal void Generate(PropertyModel model, StringBuilder builder)
        {
            builder.Append($@"public {model.Type} {model.Name} {{");

            if (model.Get)
            {
                builder.Append($@"get {{ return get_{model.Name}_Mock(); }}");
            }

            if (model.Set)
            {
                builder.Append($@"set {{ set_{model.Name}_Mock(value); }}");
            }

            builder.Append($@"}}");


        }
    }
}