using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory) : base(enemyAI, enemyStateFactory){}
    public override void EnterState()
    {
        //_ec.enemyAgent.enabled = true;
        _ec.mainRigidbody.isKinematic = true;
        _ec.enemyAgent.isStopped = false;
        
    }
    public override void UpdateState()
    {
        ChaseTarget();
        CheckSwitchStates();
    }
    public override void ExitState()
    {
        //_ec.enemyAgent.enabled = false;
        _ec.mainRigidbody.isKinematic = false;
        _ec.enemyAgent.isStopped = true;
    }
    public override void CheckSwitchStates()
    {
        if (_ec.enemyAgent.remainingDistance < _ec.attackRange)
        {
            SwitchState(_factory.Attack());
        }
        if (_ec.StartRagdoll)
        {
            SwitchState(_factory.Ragdoll());
        }
        if (_ec.IsOnCrouchArea())
        {
            SwitchState(_factory.Crouch());
        }
    }
    public override void InitializeSubState(){}
    public void ChaseTarget() 
    {
        _ec.animator.Play("run", 0);
        _ec.enemyAgent.speed = _ec.chaseSpeed;
        _ec.enemyAgent.SetDestination(_ec.target.transform.position);
    }
}