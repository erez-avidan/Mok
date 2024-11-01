using System.Collections.Generic;

namespace MokMock.Models
{
    internal class ClassModel
    {
        public string MockName { get; set; }
        public string FullNamespace { get; set; }
        public List<MethodModel> Methods { get; internal set; }
        public List<PropertyModel> Properties { get; internal set; }
        public string TypeToString { get; internal set; }
    }
}