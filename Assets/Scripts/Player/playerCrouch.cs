using UnityEngine;

public class PlayerCrouch
{
    private playerController _pc;
    
    public PlayerCrouch(playerController controller)
    {
        _pc = controller;
    }
    
    public bool CanStandUp()
    {
        float radius = _pc.capsuleCollider.radius * 0.2f;

        Vector3 bottom = _pc.feetRayPos.transform.position;
        Vector3 top = _pc.playerHeadTop.transform.position;

        int obstacleLayer = LayerMask.GetMask("Ceiling");

        bool blocked = Physics.CheckCapsule(bottom, top, radius, obstacleLayer);
        _pc.Animator.animator.SetBool("CanStand", !blocked);

        return !blocked;
    }

    // Crouching logic
    public void HandleCrouching(bool crouchTriggered)
    {
        if (_pc.isSliding) return;

        bool shouldCrouch = crouchTriggered && _pc.movement.isGrounded();
        bool tryingToStand = !crouchTriggered && _pc.isCrouched && _pc.movement.isGrounded();

        if (tryingToStand && !_pc.crouch.CanStandUp() && _pc.movement.isGrounded())
            shouldCrouch = true; // stay crouched if blocked
   
        // Adjust collider height based on player state
        _pc.capsuleCollider.height = shouldCrouch ? _pc.crouchHeight : _pc.standHeight;

        _pc.isCrouched = shouldCrouch;
        _pc.Animator.animator.SetBool("isCrouched", _pc.isCrouched);
    }
}