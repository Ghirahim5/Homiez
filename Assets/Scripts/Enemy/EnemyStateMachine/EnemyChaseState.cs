using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory) : base(enemyAI, enemyStateFactory){}
    public override void EnterState()
    {
        _ec.enemyAgent.enabled = true;
        _ec.animator.SetBool("Chasing", true);
    }
    public override void UpdateState()
    {
        ChaseTarget();
        CheckSwitchStates();
    }
    public override void ExitState()
    {
        _ec.enemyAgent.enabled = false;
        _ec.animator.SetBool("Chasing", false);
    }
    public override void CheckSwitchStates()
    {
        if (Vector3.Distance(_ec.target.position, _ec.transform.position) < _ec.attackRange)
        {
            SwitchState(_factory.Attack());
        }
        if (_ec.StartRagdoll)
        {
            SwitchState(_factory.Ragdoll());
        }
    }
    public override void InitializeSubState(){}
    public void ChaseTarget() 
    {
        _ec.enemyAgent.SetDestination(_ec.target.position);
    }
}