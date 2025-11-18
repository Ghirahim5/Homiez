using UnityEngine;

public class StateHandler
{
    private EnemyAI _ec;

    public StateHandler(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
    public void StandUp()
    {
        AnimatorStateInfo info = _ec.animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("standbackup"))
        {
            if (info.normalizedTime >= 1f)
            {
                _ec.enemyAgent.enabled = true;
                _ec.animator.SetBool("Ragdoll", false);
                _ec.animator.SetBool("Chasing", true);
                _ec.animator.ResetTrigger("Attack");
                _ec.animator.ResetTrigger("StandUp");
                _ec.currentState = EnemyAI.EnemyState.Chasing;
            }
        }
    }

    public void Attack()
    {
        AnimatorStateInfo info = _ec.animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("attack") && info.normalizedTime >= 1f)
        {
            _ec.enemyAgent.enabled = true;
            _ec.animator.SetBool("Chasing", true);
            _ec.animator.ResetTrigger("Attack");
            _ec.animator.ResetTrigger("StandUp");
            _ec.currentState = EnemyAI.EnemyState.Chasing;
        }
    }
}
