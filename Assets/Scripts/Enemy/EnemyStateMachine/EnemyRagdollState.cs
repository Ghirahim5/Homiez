using UnityEngine;

public class EnemyRagdollState : EnemyBaseState
{
    public EnemyRagdollState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory) : base(enemyAI, enemyStateFactory)
    {
    }
    public override void EnterState()
    {
        _ec.animator.SetBool("Ragdoll", true);
        PushRagdoll();
    }
    public override void UpdateState()
    {
        HandleRagdoll();   
        CheckSwitchStates();
    }
    public override void ExitState()
    {
        _ec.animator.SetBool("Ragdoll", false);
    }
    public override void CheckSwitchStates()
    {
        if (_ec.animator.GetCurrentAnimatorStateInfo(0).IsName("standbackup") && 
        _ec.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            SwitchState(_factory.Chase());
        }
    }
    public override void InitializeSubState(){}

    public void PushRagdoll ()
    {
        EnableRagdoll();
        foreach (var rb in _ec.RagdollRigidbodies)
        {
            rb.AddForce(_ec.collisionHandler.hitDirection * _ec.collisionHandler.strength, ForceMode.Impulse);
        }

    }

    public void HandleRagdoll()
    {
        if (_ec.StartRagdoll)
            {
                _ec.collisionTimer += Time.fixedDeltaTime;

                if (_ec.collisionTimer >= _ec.recoveryTime)
                {
                    _ec.currentPlayer = null;
                    _ec.collisionTimer = 0f;
                    AllignPosition();
                    DisableRagdoll();
                    
                }
            }
    }
    public void DisableRagdoll()
    {
        foreach (var rigidbody in _ec.RagdollRigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
        foreach (var col in _ec.RagdollColliders)
        {
            col.enabled = false;
        }
        _ec.StartRagdoll = false;
        //_ec.mainCollider.enabled = true;
        //_ec.mainRigidbody.isKinematic = false;
        _ec.animator.enabled = true;
        _ec.animator.Play("standbackup", 0, 0f);
    }
    private void EnableRagdoll()
    {
        _ec.animator.enabled = false;
        //_ec.mainRigidbody.isKinematic = true;
        //_ec.mainCollider.enabled = false;

        foreach (var rigidbody in _ec.RagdollRigidbodies)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }
        foreach (var col in _ec.RagdollColliders)
        {
            col.enabled = true;
        }
    }
    private void AllignPosition()
    {
        Vector3 originalHipsPosition = _ec.ragdollRoot.position;
        _ec.transform.position = _ec.ragdollRoot.position;

        if (Physics.Raycast(_ec.transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            _ec.transform.position = new Vector3(
                _ec.transform.position.x,
                hitInfo.point.y,
                _ec.transform.position.z
            );
        }

        _ec.ragdollRoot.position = originalHipsPosition;
    }
}