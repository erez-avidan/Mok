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

        [Fact]
        public void Test_Setup_ItIs()
        {
            var mock = new Mok<IInterface>();
            mock.Setup(x => x.GetSum(It.Is<int>(x => x > 10 && x < 100), 2)).Returns(100);
            var mocked = mock.Object;

            Assert.Equal(100, mocked.GetSum(20, 2));
        }


        [Fact]
        public async Task Test_Setup_Async()
        {
            var mock = new Mok<IInterface>();
            mock.Setup(x => x.GetSumAsync(It.IsAny<string>())).ReturnsAsync(100);
            var mocked = mock.Object;

            Assert.Equal(100, await mocked.GetSumAsync("A"));
        }

        [Fact]
        public void Test_Setup_Get()
        {
            var mock = new Mok<IInterface>();
            mock.SetupGet(x => x.Prop).Returns(100);
            var mocked = mock.Object;

            Assert.Equal(100, mocked.Prop);
        }

        [Fact]
        public void Test_Setup_Set()
        {
            var mock = new Mok<IInterface>();
            var isBelow = false;
            mock.SetupSet(x => x.Prop, () => It.Is<int>(x => x > 5)).Callback(() => isBelow = false);
            mock.SetupSet(x => x.Prop, () => It.Is<int>(x => x <= 5)).Callback(() => isBelow = true);
            var mocked = mock.Object;

            mocked.Prop = 100;
            Assert.False(isBelow);

            mocked.Prop = 1;
            Assert.True(isBelow);
        }

        [Fact]
        public void Test_Setup_GenericMethod()
        {
            var mock = new Mok<IInterface>();
            mock.Setup(m => m.Do(1)).Returns(123);
            mock.Setup(m => m.Do<string>("a")).Returns("abc");

            var mocked = mock.Object;

            Assert.Equal(123, mocked.Do(1));
            Assert.Equal("abc", mocked.Do<string>("a"));
        }

        [Fact]
        public void Test_Setup_GenericClass()
        {
            var mock = new Mok<IGenericInterface<int>>();
            mock.Setup(m => m.Do(1)).Returns(123);

            var mock1 = new Mok<IGenericInterface<string>>();
            mock1.Setup(m => m.Do("a")).Returns("abc");

            var mocked = mock.Object;
            var mocked1 = mock1.Object;

            Assert.Equal(123, mocked.Do(1));
            Assert.Equal("abc", mocked1.Do("a"));
        }
    }
}