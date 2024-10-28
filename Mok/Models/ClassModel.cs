using System.Collections.Generic;

namespace MokMock.Models
{
    internal class ClassModel
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<MethodModel> Methods { get; internal set; }
        public List<PropertyModel> Properties { get; internal set; }
    }
}