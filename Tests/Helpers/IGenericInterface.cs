namespace ConsoleApp1
{
    internal interface IGenericInterface<T>
    {
        T Prop { get; set; }

        T Do(T x);
    }
}
