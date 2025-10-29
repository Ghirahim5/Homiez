using UnityEngine;

public class PlayerCrouch
{
    private playerController _pc;
    private float _lastCrouchTime;
    
    public PlayerCrouch(playerController controller)
    {
        _pc = controller;
        _lastCrouchTime = -_pc.minCrouchInterval;
    }
    
    public bool CanStandUp()
    {
        float radius = _pc.capsuleCollider.radius * 0.2f;
        Vector3 bottom = _pc.feetRayPos;
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

        // Prevent spam
        if (Time.time - _lastCrouchTime < _pc.minCrouchInterval)
            return;

        bool shouldCrouch = crouchTriggered && _pc.movement.isGrounded();
        bool tryingToStand = !crouchTriggered && _pc.isCrouched && _pc.movement.isGrounded();
        float groundCheckDistance = 2f;
        RaycastHit hit;

        if (tryingToStand && !_pc.crouch.CanStandUp() && _pc.movement.isGrounded())
            shouldCrouch = true; // stay crouched if blocked

        if (crouchTriggered && (Physics.Raycast(_pc.transform.position, Vector3.down, out hit, groundCheckDistance) && hit.collider.CompareTag("Stairs")))
            shouldCrouch = true;

        // Only change state if different
        if (shouldCrouch != _pc.isCrouched)
        {
            _pc.capsuleCollider.height = shouldCrouch ? _pc.crouchHeight : _pc.standHeight;
            _pc.isCrouched = shouldCrouch;
            _pc.Animator.animator.SetBool("isCrouched", _pc.isCrouched);
            _lastCrouchTime = Time.time; // reset timer
        }
    }
}