namespace MokMock.Models
{
    public class PropertyModel
    {
        public string Name { get; internal set; }
        public string Type { get; internal set; }
        public bool Get { get; internal set; }
        public bool Set { get; internal set; }
    }
}