using System.Collections.Generic;

namespace MokMock.Models
{
    internal class MethodModel
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterModel> Parameters { get; set; }
    }
}