using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using UnityEngine;

namespace Com.Voobox.Project.Entity
{
    [RequireComponent(typeof(Animator))]
    public class BossAttackEmitter : MonoBehaviour
    {
        private const string RAISE_OF_SWORDS_LEFT_ATTACK = "RaiseOfSwords_Left";
        private const string RAISE_OF_SWORDS_RIGHT_ATTACK = "RaiseOfSwords_Right";

        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();

            EventBus.Subscribe<DoRaiseOfSwordsEvent>(DoRaiseOfSwords);
        }

        private void DoRaiseOfSwords(DoRaiseOfSwordsEvent args)
        {
            m_animator.Play(RAISE_OF_SWORDS_LEFT_ATTACK);
        }
    }
}
