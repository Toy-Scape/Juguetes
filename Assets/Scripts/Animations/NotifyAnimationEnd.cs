using UnityEngine;

public class NotifyClimbAnimationEnd : StateMachineBehaviour
{
    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger("ClimbFinished");
        animator.SendMessage("OnClimbAnimationFinished", SendMessageOptions.DontRequireReceiver);
    }
}
