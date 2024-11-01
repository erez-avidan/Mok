namespace ConsoleApp1
{
    internal interface IInterface
    {
        void DoIt();

        int GetSum(int a, int b);

        Task DoAsync(string x);

        Task<int> GetSumAsync(string x);

        int Prop { get; set; }

        T Do<T>(T x);

    }
}
