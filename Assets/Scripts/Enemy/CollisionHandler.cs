using Unity.VisualScripting;
using UnityEngine;

public class CollisionHandler
{
    private EnemyAI _ec;
    float collisionTimer = 0f;

    public CollisionHandler(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
    public void Collision(playerController player, Collision collision)
    {
        Vector3 playerSpeed = player.rb.linearVelocity;
        float playerMomentum = new Vector3(playerSpeed.x, 0f, playerSpeed.z).magnitude;
        
        Rigidbody hitbone = collision.rigidbody;
        Vector3 hitDirection = (_ec.transform.position - player.transform.position).normalized;
        float strength = playerMomentum * _ec.pushForce;
        strength = Mathf.Clamp(strength, 0f, 50f);
        _ec.animator.SetBool("Chasing", false);

        if(strength >= _ec.requiredPushForce)
        {
            _ec.currentState=EnemyAI.EnemyState.Ragdoll;
            _ec.enemyAgent.enabled = false;
            _ec.StartRagdoll = true;
            EnableRagdoll();

            hitbone.AddForce(hitDirection * strength, ForceMode.Impulse);
            foreach (var rb in _ec.RagdollRigidbodies)
            {
                rb.AddForce(hitDirection * (strength * 0.5f), ForceMode.Impulse);
            }

        }
        else
        {
            _ec.currentState = EnemyAI.EnemyState.Attacking;
            _ec.enemyAgent.enabled = false;
            _ec.animator.SetTrigger("Attack");
        } 

    }

    public void HandleCollisionTimer()
    {
        if (!_ec.StartRagdoll)
            return;

        collisionTimer += Time.fixedDeltaTime;

        if (collisionTimer >= _ec.recoveryTime)
        {
            _ec.StartRagdoll = false;
            _ec.currentPlayer = null;
            collisionTimer = 0f;
            AllignPosition();
            DisableRagdoll();
            
        }
    }
        public void DisableRagdoll()
    {
        foreach (var rigidbody in _ec.RagdollRigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
        _ec.animator.enabled = true;
        _ec.currentState = EnemyAI.EnemyState.StandUp;
        _ec.animator.Play("standbackup", 0, 0f);
    }
    private void EnableRagdoll()
    {
        _ec.animator.SetBool("Ragdoll", true);
        _ec.animator.enabled = false;
        _ec.enemyAgent.enabled = false;
        foreach (var rigidbody in _ec.RagdollRigidbodies)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
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
                hitInfo.point.y-0.55f,
                _ec.transform.position.z
            );
        }

        _ec.ragdollRoot.position = originalHipsPosition;
    }

}
