using MokMock.Models;
using System.Collections.Generic;
using System.Text;

namespace MokMock.CodeGenerators
{
    internal class PropertyGenerator
    {
        internal void Generate(PropertyModel model, StringBuilder builder, HashSet<string> signaturesSet)
        {
            var signature = $"public {model.Type} {model.Name}";

            if (signaturesSet.Contains(signature))
            {
                return;
            }

            signaturesSet.Add(signature);

            builder.Append(signature);

            builder.Append("{");

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