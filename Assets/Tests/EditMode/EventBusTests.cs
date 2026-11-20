using System;
using Com.Voobox.Framework.Events;
using NUnit.Framework;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class EventBusTests
    {
        private readonly struct PrimaryTestEvent : IEvent
        {
            public int Value { get; }
            public string Label { get; }

            public PrimaryTestEvent(int value, string label)
            {
                Value = value;
                Label = label;
            }
        }

        private readonly struct SecondaryTestEvent : IEvent
        {
            public int Code { get; }

            public SecondaryTestEvent(int code)
            {
                Code = code;
            }
        }

        [Test]
        public void Notify_InvokesSubscribedCallbackWithExactPayload()
        {
            PrimaryTestEvent received = default;
            var callCount = 0;

            void Handler(PrimaryTestEvent evt)
            {
                callCount++;
                received = evt;
            }

            EventBus.Subscribe<PrimaryTestEvent>(Handler);
            try
            {
                var expected = new PrimaryTestEvent(42, "hero-hit");
                EventBus.Notify(expected);

                Assert.AreEqual(1, callCount);
                Assert.AreEqual(42, received.Value);
                Assert.AreEqual("hero-hit", received.Label);
            }
            finally
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(Handler);
            }
        }

        [Test]
        public void Notify_InvokesMultipleSubscribersForSameEventType()
        {
            var firstCount = 0;
            var secondCount = 0;
            var firstValue = 0;
            var secondValue = 0;

            void FirstHandler(PrimaryTestEvent evt)
            {
                firstCount++;
                firstValue = evt.Value;
            }

            void SecondHandler(PrimaryTestEvent evt)
            {
                secondCount++;
                secondValue = evt.Value;
            }

            EventBus.Subscribe<PrimaryTestEvent>(FirstHandler);
            EventBus.Subscribe<PrimaryTestEvent>(SecondHandler);
            try
            {
                EventBus.Notify(new PrimaryTestEvent(99, "multi"));

                Assert.AreEqual(1, firstCount);
                Assert.AreEqual(1, secondCount);
                Assert.AreEqual(99, firstValue);
                Assert.AreEqual(99, secondValue);
            }
            finally
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(FirstHandler);
                EventBus.Unsubscribe<PrimaryTestEvent>(SecondHandler);
            }
        }

        [Test]
        public void Unsubscribe_RemovesOnlySpecifiedCallbackAndKeepsOthersActive()
        {
            var firstCount = 0;
            var secondCount = 0;

            void FirstHandler(PrimaryTestEvent evt) => firstCount++;
            void SecondHandler(PrimaryTestEvent evt) => secondCount++;

            EventBus.Subscribe<PrimaryTestEvent>(FirstHandler);
            EventBus.Subscribe<PrimaryTestEvent>(SecondHandler);
            try
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(FirstHandler);
                EventBus.Notify(new PrimaryTestEvent(7, "after-unsub"));

                Assert.AreEqual(0, firstCount);
                Assert.AreEqual(1, secondCount);
            }
            finally
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(FirstHandler);
                EventBus.Unsubscribe<PrimaryTestEvent>(SecondHandler);
            }
        }

        [Test]
        public void Unsubscribe_UnregisteredEventOrCallback_IsSafeNoOp()
        {
            void UnregisteredHandler(PrimaryTestEvent evt) { }
            void RegisteredHandler(PrimaryTestEvent evt) { }

            Assert.DoesNotThrow(() => EventBus.Unsubscribe<PrimaryTestEvent>(UnregisteredHandler));

            EventBus.Subscribe<PrimaryTestEvent>(RegisteredHandler);
            try
            {
                Assert.DoesNotThrow(() => EventBus.Unsubscribe<PrimaryTestEvent>(UnregisteredHandler));
            }
            finally
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(RegisteredHandler);
            }
        }

        [Test]
        public void Notify_WithNoSubscribers_IsSafeNoOp()
        {
            Assert.DoesNotThrow(() => EventBus.Notify(new SecondaryTestEvent(123)));
        }

        [Test]
        public void Notify_DifferentEventTypes_AreIsolated()
        {
            var primaryCount = 0;
            var secondaryCount = 0;

            void PrimaryHandler(PrimaryTestEvent evt) => primaryCount++;
            void SecondaryHandler(SecondaryTestEvent evt) => secondaryCount++;

            EventBus.Subscribe<PrimaryTestEvent>(PrimaryHandler);
            EventBus.Subscribe<SecondaryTestEvent>(SecondaryHandler);
            try
            {
                EventBus.Notify(new PrimaryTestEvent(10, "only-primary"));

                Assert.AreEqual(1, primaryCount);
                Assert.AreEqual(0, secondaryCount);

                EventBus.Notify(new SecondaryTestEvent(20));

                Assert.AreEqual(1, primaryCount);
                Assert.AreEqual(1, secondaryCount);
            }
            finally
            {
                EventBus.Unsubscribe<PrimaryTestEvent>(PrimaryHandler);
                EventBus.Unsubscribe<SecondaryTestEvent>(SecondaryHandler);
            }
        }
    }
}
