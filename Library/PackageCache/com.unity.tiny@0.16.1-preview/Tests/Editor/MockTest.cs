using NUnit.Framework;
using Moq;

namespace Unity.Editor.Tests
{
    [TestFixture]
    internal class MoqTests
    {
        [Test]
        public void EnsureMoqIsCorrectlyLoadedAndUsable()
        {
            var mock = new Mock<IMockTestInterface>();
            mock.Setup(m => m.GetNumber(It.IsAny<int>())).Returns(10);
            mock.Setup(m => m.GetNumber(123)).Returns(20);
            
            Assert.That(mock.Object.GetNumber(123), Is.EqualTo(20));
            Assert.That(mock.Object.GetNumber(124), Is.EqualTo(10));

            mock.Verify(m => m.GetNumber(123), Times.Once());
            mock.Verify(m => m.GetNumber(124), Times.Once());
            mock.Verify(m => m.GetNumber(It.IsAny<int>()), Times.Exactly(2));
        }

        public interface IMockTestInterface
        {
            int GetNumber(int input);
        }
    }
}
