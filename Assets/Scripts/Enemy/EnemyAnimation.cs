using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator animator;
    public string runParameter = "Run";
    public string idleParameter = "Idle";
    public string dieTriggerParameter = "Die";

    public void SetRun(bool isRunning)
    {
        if (animator == null) return;
        
        if (!string.IsNullOrEmpty(runParameter))
        {
            animator.SetBool(runParameter, isRunning);
        }
        
        if (!string.IsNullOrEmpty(idleParameter))
        {
            animator.SetBool(idleParameter, !isRunning);
        }
    }

    public void SetDie()
    {
        if (animator == null) return;
        
        if (!string.IsNullOrEmpty(dieTriggerParameter))
        {
            animator.SetTrigger(dieTriggerParameter);
        }
    }
}
