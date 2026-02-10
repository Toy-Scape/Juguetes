using UnityEngine;

public class NotifyStandAnimationEnded : StateMachineBehaviour
{
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage("OnStandingAnimationEnter", SendMessageOptions.RequireReceiver);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage("OnStandingAnimationFinished", SendMessageOptions.RequireReceiver);
    }
}
