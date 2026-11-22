using System;
using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Entity;
using Com.Voobox.Project.Events.BossEvents;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class BossAttackEmitterTests
    {
        private GameObject m_emitterGo;
        private BossAttackEmitter m_emitter;

        [TearDown]
        public void TearDown()
        {
            if (m_emitter != null)
            {
                MethodInfo method = typeof(BossAttackEmitter).GetMethod(
                    "DoRaiseOfSwords",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    var handler = (Action<DoRaiseOfSwordsEvent>)Delegate.CreateDelegate(
                        typeof(Action<DoRaiseOfSwordsEvent>),
                        m_emitter,
                        method);
                    EventBus.Unsubscribe(handler);
                }
            }

            if (m_emitterGo != null)
            {
                Object.DestroyImmediate(m_emitterGo);
            }

            m_emitter = null;
            m_emitterGo = null;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        [Test]
        public void Awake_CachesAnimator_AndSubscribesToDoRaiseOfSwordsEvent()
        {
            m_emitterGo = new GameObject("BossAttackEmitterTest");
            m_emitter = m_emitterGo.AddComponent<BossAttackEmitter>();
            var expectedAnimator = m_emitterGo.GetComponent<Animator>();

            InvokePrivateMethod(m_emitter, "Awake");

            var cachedAnimator = GetPrivateField<Animator>(m_emitter, "m_animator");
            Assert.IsNotNull(cachedAnimator);
            Assert.AreSame(expectedAnimator, cachedAnimator);

            FieldInfo subscribersField = typeof(EventBus).GetField(
                "m_subscribers",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(subscribersField, "Field 'm_subscribers' not found on EventBus.");

            var subscribers = subscribersField.GetValue(null) as Dictionary<Type, List<Delegate>>;
            Assert.IsNotNull(subscribers);
            Assert.IsTrue(subscribers.TryGetValue(typeof(DoRaiseOfSwordsEvent), out List<Delegate> callbacks));

            MethodInfo method = typeof(BossAttackEmitter).GetMethod(
                "DoRaiseOfSwords",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var expectedHandler = (Action<DoRaiseOfSwordsEvent>)Delegate.CreateDelegate(
                typeof(Action<DoRaiseOfSwordsEvent>),
                m_emitter,
                method);
            Assert.IsTrue(callbacks.Contains(expectedHandler));
        }

        [Test]
        public void NotifyDoRaiseOfSwordsEvent_InvokesDoRaiseOfSwords_WithoutThrowing()
        {
            m_emitterGo = new GameObject("BossAttackEmitterTest");
            m_emitter = m_emitterGo.AddComponent<BossAttackEmitter>();

            InvokePrivateMethod(m_emitter, "Awake");

            Assert.DoesNotThrow(() => EventBus.Notify(new DoRaiseOfSwordsEvent()));
        }
    }
}
