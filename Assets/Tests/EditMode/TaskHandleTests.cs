using System.Threading;
using NUnit.Framework;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class TaskHandleTests
    {
        private TaskHandle m_taskHandle;

        [SetUp]
        public void SetUp()
        {
            m_taskHandle = new TaskHandle();
        }

        [TearDown]
        public void TearDown()
        {
            m_taskHandle?.Stop();
        }

        [Test]
        public void GetNewToken_ReturnsNonCancelledToken()
        {
            CancellationToken token = m_taskHandle.GetNewToken();

            Assert.IsTrue(token.CanBeCanceled);
            Assert.IsFalse(token.IsCancellationRequested);
        }

        [Test]
        public void GetNewToken_CalledSecondTime_CancelsPreviousTokenAndReturnsFreshToken()
        {
            CancellationToken firstToken = m_taskHandle.GetNewToken();
            CancellationToken secondToken = m_taskHandle.GetNewToken();

            Assert.IsTrue(firstToken.IsCancellationRequested);
            Assert.IsTrue(secondToken.CanBeCanceled);
            Assert.IsFalse(secondToken.IsCancellationRequested);
        }

        [Test]
        public void Stop_CancelsActiveToken()
        {
            CancellationToken token = m_taskHandle.GetNewToken();

            m_taskHandle.Stop();

            Assert.IsTrue(token.IsCancellationRequested);
        }

        [Test]
        public void Stop_BeforeGetNewToken_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_taskHandle.Stop());
        }

        [Test]
        public void Stop_CalledMultipleTimes_DoesNotThrow()
        {
            CancellationToken token = m_taskHandle.GetNewToken();

            Assert.DoesNotThrow(() =>
            {
                m_taskHandle.Stop();
                m_taskHandle.Stop();
            });

            Assert.IsTrue(token.IsCancellationRequested);
        }
    }
}
