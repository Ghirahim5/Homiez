using Unity.VisualScripting;
using UnityEngine;

public class CollisionHandler
{
    private EnemyAI _ec;
    public Vector3 hitDirection;
    public float strength;

    public CollisionHandler(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
 public void Collision(playerController player)
    {
        Vector3 playerSpeed = player.rb.linearVelocity;
        float playerMomentum = new Vector3(playerSpeed.x, 0f, playerSpeed.z).magnitude;
            
        hitDirection = (_ec.transform.position - player.transform.position).normalized;
        strength = Mathf.Clamp(playerMomentum * _ec.pushForce, 0f, 50f);
        
        bool isSprinting = player.movement.isSprinting;
        bool isSliding = player.isSliding;
        bool isMidAir = !player.movement.isGrounded(); //true, if isgrounded is false
        if (isSprinting || isSliding || isMidAir) _ec.StartRagdoll = true;
        else _ec.StartRagdoll = false;
    }
}