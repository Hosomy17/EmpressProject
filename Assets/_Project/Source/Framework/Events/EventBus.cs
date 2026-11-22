using System;
using System.Collections.Generic;

namespace Com.Voobox.Framework.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> m_subscribers = new();

        public static void Subscribe<T>(Action<T> callback) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!m_subscribers.ContainsKey(eventType))
                m_subscribers[eventType] = new List<Delegate>();

            m_subscribers[eventType].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!m_subscribers.TryGetValue(eventType, out List<Delegate> subscriber)) return;
            subscriber.Remove(callback);

            if (m_subscribers[eventType].Count == 0)
                m_subscribers.Remove(eventType);
        }

        public static void Notify<T>(T eventArgs) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!m_subscribers.TryGetValue(eventType, out List<Delegate> callbacks)) return;
            foreach (Delegate callback in callbacks)
            {
                (callback as Action<T>)?.Invoke(eventArgs);
            }
        }
    }

    public interface IEvent
    {
    }
}