using Mok.Models;
using System.Collections.Generic;

namespace MokMock.Models
{
    internal class ClassModel
    {
        public List<MethodModel> Methods { get; internal set; }
        public List<PropertyModel> Properties { get; internal set; }
        public TypeModel[] TypeArguments { get; internal set; }
        public string ClassName { get; internal set; }
        public string Namespace { get; internal set; }
        public IEnumerable<ClassModel> Inherited { get; internal set; }
    }
}