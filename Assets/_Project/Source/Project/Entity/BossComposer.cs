using ArtificeToolkit.Attributes;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Events.BossEvents;
using UnityEngine;

namespace Com.Voobox.Project.Entity
{
    [RequireComponent(typeof(Collider2D))]
    public class BossComposer : MonoBehaviour
    {
        [SerializeField, InlineObject] private BossDataScriptableObject m_bossData;

        private HealthComponent m_healthComponent;

        private void Awake()
        {
            m_bossData = Instantiate(m_bossData);
            m_healthComponent = new HealthComponent(m_bossData.BossData);
            EventBus.Subscribe<InitializeBossEvent>(InitializeBoss);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<InitializeBossEvent>(InitializeBoss);
        }

        private void InitializeBoss(InitializeBossEvent args)
        {
            EventBus.Notify(new DoRaiseOfSwordsEvent());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (m_healthComponent.IsDead) return;
            if (((1 << collision.gameObject.layer) & m_bossData.BossData.HurtLayer) == 0) return;
            TakeDamage();
        }

        private void TakeDamage()
        {
            m_healthComponent.TakeDamage();

            if (m_healthComponent.IsDead)
                EventBus.Notify(new BossDiedEvent());
        }
    }
}
