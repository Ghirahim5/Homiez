using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathFinding
{
    private EnemyAI _ec;
    float collisionTimer = 0f;

    public PathFinding(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
    public void HandleMovement()
    {
        if (_ec.enemyAgent.isOnNavMesh && !_ec.IsColliding)
        {
            _ec.animator.SetBool("Collision", false);
            _ec.enemyAgent.destination = _ec.target.position;

        }

    }
    public void CollisionHandler(playerController player)
    {
        _ec.enemyAgent.enabled = false;
        _ec.rb.isKinematic = false;
        _ec.rb.useGravity = true;

        Vector3 playerSpeed = player.rb.linearVelocity;
        float playerMomentum = new Vector3(playerSpeed.x, 0f, playerSpeed.z).magnitude;

        Vector3 hitDirection = (_ec.transform.position - player.transform.position).normalized;

        float knockbackStrength = playerMomentum * _ec.pushForce;

        _ec.animator.SetBool("Collision", true);
        _ec.rb.AddForce(hitDirection * knockbackStrength, ForceMode.Impulse);

    }
    private bool IsStillColliding()
    {
        if (_ec.currentPlayer == null)
            return false;

        float distance = Vector3.Distance(_ec.transform.position, _ec.currentPlayer.transform.position);
        float collisionRadius = _ec.capsuleCollider.radius; // tweak as needed
        return distance < collisionRadius;
    }

    public void HandleCollisionTimer()
    {
        if (!_ec.IsColliding)
            return;

        collisionTimer += Time.fixedDeltaTime;

        // Check if collision has ended or timer expired
        if (collisionTimer >= _ec.recoveryTime && !IsStillColliding())
        {
            _ec.IsColliding = false;
            _ec.animator.SetBool("Collision", false);
            _ec.enemyAgent.enabled = true;
            _ec.rb.isKinematic = true;
            _ec.rb.useGravity = false;
            _ec.currentPlayer = null;
            collisionTimer = 0f;
        }
    }
}
