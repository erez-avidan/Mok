using ConsoleApp1;
using MokMock;
namespace Tests
{
    public class VerifyTests
    {
        [Fact]
        public void Test_Verify_Void_Called_Once()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            mocked.DoIt();

            mock.Verify(x => x.DoIt(), Times.Once);
        }

        [Fact]
        public void Test_Verify_Primitive_Parameters_Called_Once()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            mocked.GetSum(1,2);

            mock.Verify(x => x.GetSum(1, 2), Times.Once);
            mock.Verify(x => x.DoIt(), Times.Never);
        }

        [Fact]
        public void Test_Verify_Primitive_Parameters_Called_3Times()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            mocked.GetSum(1, 2);
            mocked.GetSum(1, 2);
            mocked.GetSum(1, 2);

            mock.Verify(x => x.GetSum(1, 2), Times.Exactly(3));
        }

        [Fact]
        public void Test_Verify_Primitive_Parameters_Called_2Times_Not_Matching()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;
            int a = 3; int b = 4;
            mocked.GetSum(a, b);
            mocked.GetSum(2, 3);

            mock.Verify(x => x.GetSum(1, 2), Times.Never);
        }

        [Fact]
        public void Test_Verify_ItIsAny()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            mocked.GetSum(1, 2);

            mock.Verify(x => x.GetSum(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Test_Verify_ItIs()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            mocked.GetSum(50, 2);

            mock.Verify(x => x.GetSum(It.Is<int>(x => x < 100), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Test_MultiThread_counting()
        {
            var mock = new Mok<IInterface>();

            var mocked = mock.Object;

            var iterations = Enumerable.Range(1, 1000);
            Parallel.ForEach(iterations, item =>
            {
                mocked.GetSum(50, 2);
            });
            
            mock.Verify(x => x.GetSum(It.Is<int>(x => x < 100), It.IsAny<int>()), Times.Exactly(1000));
        }
    }
}