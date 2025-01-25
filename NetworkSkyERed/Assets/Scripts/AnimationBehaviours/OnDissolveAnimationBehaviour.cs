using UnityEngine;

namespace AnimationBehaviours
{
    public class OnDissolveAnimationBehaviour : StateMachineBehaviour
    {
        public new void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("Animation disolve ON EXIT");
        }
    }
}