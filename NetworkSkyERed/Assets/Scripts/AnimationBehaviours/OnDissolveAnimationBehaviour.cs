using UnityEngine;

namespace AnimationBehaviours
{
    public class OnDissolveAnimationBehaviour : StateMachineBehaviour
    {
        static readonly int _dissolve = Animator.StringToHash("Dissolve");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(_dissolve, false);
            CharacterView[] view = animator.gameObject.GetComponentsInParent<CharacterView>();
            view[0].OnDissolveStart();
        }
    }
}