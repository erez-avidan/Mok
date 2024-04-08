using ConsoleApp1;
using MokMock;
namespace Tests
{
    public class SetupTests
    {
        [Fact]
        public void Test_Setup_Void_Throws()
        {
            var mock = new Mok<IInterface>();

            mock.Setup(x => x.DoIt()).Throws(new AggregateException());

            var mocked = mock.Object;

            try
            {
                mocked.DoIt();
                Assert.Fail();
            }
            catch (AggregateException)
            {

            }
        }

        [Fact]
        public void Test_Setup_ReturnsInt()
        {
            var mock = new Mok<IInterface>();
            int something = 2;
            mock.Setup(x => x.GetSum(1, something)).Returns(10);
            var mocked = mock.Object;

            Assert.Equal(10, mocked.GetSum(1, 2));
        }

        [Fact]
        public void Test_Setup_Any()
        {
            var mock = new Mok<IInterface>();
            mock.Setup(x => x.GetSum(It.IsAny<int>(), It.IsAny<int>())).Returns(100);
            var mocked = mock.Object;

            Assert.Equal(100, mocked.GetSum(1, 2));
        }
    }
}