using UnityEngine;
using UnityEngine.AI;

public class EnemyCrouchState : EnemyBaseState
{
    private float originalSpeed;
    private bool AnimationFinished;
    private bool exitTriggered;
    

    public EnemyCrouchState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory) : base(enemyAI, enemyStateFactory) {}

    public override void EnterState()
    {
        AnimationFinished = false;
        exitTriggered = false;
        originalSpeed = _ec.enemyAgent.speed;
        _ec.mainRigidbody.isKinematic = true;
        _ec.animator.Play("crouch1", 0, 0f);
        _ec.enemyAgent.isStopped = true;

        _ec.enemyAgent.speed = _ec.chaseSpeed/2; 
    }

    public override void UpdateState()
    {
        UpdateCrouchMovement();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ec.enemyAgent.speed = originalSpeed;
        _ec.mainRigidbody.isKinematic = false;
    }

    public override void CheckSwitchStates()
    {
        if (_ec.StartRagdoll)
        {
            SwitchState(_factory.Ragdoll());
            return;
        }

        if (_ec.animator.GetCurrentAnimatorStateInfo(0).IsName("crouch1") &&
            _ec.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            AnimationFinished = true;
        }

        if (!_ec.IsOnCrouchArea())
        {
            if (!exitTriggered)
            {
                _ec.animator.Play("crouch2", 0, 0f);
                _ec.enemyAgent.isStopped = true;
                exitTriggered = true;
                return;
            }
        }
        var stateInfo = _ec.animator.GetCurrentAnimatorStateInfo(0);

        if (exitTriggered && stateInfo.normalizedTime >= 1f)
        {
            if (_ec.enemyAgent.remainingDistance < _ec.attackRange)
            {
                SwitchState(_factory.Attack());
                return;
            }
            SwitchState(_factory.Chase());
            return;
        }
    }

    public override void InitializeSubState() {}

    private void UpdateCrouchMovement()
    {
        if (AnimationFinished && !exitTriggered)
        {
            _ec.enemyAgent.isStopped = false;
            _ec.animator.Play("crouch walk", 0);
            _ec.enemyAgent.SetDestination(_ec.target.transform.position);
        }
    }
}
