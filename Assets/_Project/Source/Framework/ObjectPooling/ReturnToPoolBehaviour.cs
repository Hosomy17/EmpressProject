using UnityEngine;

namespace Com.Voobox.Framework.ObjectPooling
{
    public class ReturnToPoolBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ObjectPooler.Instance.ReturnToPool(animator.gameObject);
        }
    }
}