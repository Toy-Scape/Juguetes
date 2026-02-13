using System.Collections;
using System.Dynamic;
using UnityEngine;

public class TutorialIntroBehaviour : StateMachineBehaviour
{
    [SerializeField] float introDuration = 5f;
    [SerializeField] string triggerName = "";

    float timer;
    bool triggered;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InputMapManager.Instance.HandleCinematicStart();
        timer = 0f;
        triggered = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (triggered) return;

        timer += Time.deltaTime;

        if (timer >= introDuration)
        {
            animator.SetTrigger(triggerName);
            triggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InputMapManager.Instance.HandleCinematicEnd();
        CameraManager.Instance.LowPriorityIntroTutorial();
    }
}
