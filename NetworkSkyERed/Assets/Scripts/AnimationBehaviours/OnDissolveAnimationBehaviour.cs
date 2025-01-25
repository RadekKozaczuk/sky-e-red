using Unity.Netcode;
using UnityEngine;

namespace AnimationBehaviours
{
    public class OnDissolveAnimationBehaviour : StateMachineBehaviour
    {
        static readonly int _dissolve = Animator.StringToHash("Dissolve");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(_dissolve, false);
            var view = animator.gameObject.GetComponent<CharacterView>();
            view.OnDissolveStart();
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // todo probably to be deleted
            if (!NetworkManager.Singleton.IsHost)
                return;
            
            var view = animator.gameObject.GetComponent<CharacterView>();
            
            GameController.Singleton.OnCharacterDeath(view.PlayerId, view.Id);
        }
    }
}