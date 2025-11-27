using UnityEngine;
using UnityEngine.Rendering;

public class EnemyAttackState : EnemyBaseState
{
    public EnemyAttackState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory) : base(enemyAI, enemyStateFactory)
    {
    }
    public override void EnterState()
    {
        _ec.mainRigidbody.isKinematic = true;
    }
    public override void UpdateState()
    {
        Attack();
        CheckSwitchStates();
    }
    public override void ExitState()
    {
        _ec.mainRigidbody.isKinematic = false;
        DisableAttackHitbox();
    }
    public override void CheckSwitchStates()
    {
        if (Vector3.Distance(_ec.target.transform.position, _ec.transform.position) > _ec.attackRange &&
        _ec.animator.GetCurrentAnimatorStateInfo(0).IsName("attack") &&
        _ec.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            SwitchState(_factory.Chase());
        }
        if (_ec.StartRagdoll)
        {
            SwitchState(_factory.Ragdoll());
        }
    }
    public override void InitializeSubState(){}
    public void Attack()
    {
        _ec.animator.Play("attack");
        float animationTime = _ec.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (animationTime > 0.1f && animationTime < 0.5f) 
        {
            EnableAttackHitbox();
        }
        else
        {
            DisableAttackHitbox();
        }
        if (animationTime >= 1f)
        {
            _ec.animator.Play("attack", 0, 0f);
        }
        
        Quaternion lookRotation = Quaternion.LookRotation((_ec.target.transform.position - _ec.transform.position).normalized);
        _ec.transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }
    private void EnableAttackHitbox()
    {
        _ec.attackRigidbody.isKinematic = false;
        _ec.attackHitbox.enabled = true;
    }
    private void DisableAttackHitbox()
    {
        _ec.attackRigidbody.isKinematic = true;
        _ec.attackHitbox.enabled = false;
    }
}
