using System.Collections.Generic;

namespace MokMock.Models
{
    internal class MethodModel
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterModel> Parameters { get; set; }
        public bool IsPrivate { get; internal set; }
        public bool IsGeneric { get; internal set; }
        public IEnumerable<string> GenericTypes { get; internal set; }
    }
}