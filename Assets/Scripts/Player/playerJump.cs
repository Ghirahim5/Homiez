using UnityEngine;
using System.Collections;

public class PlayerJump
{
    private bool _jumpButtonHeldLastFrame = false;
    private float _lastJumpTime = -Mathf.Infinity;

    private playerController _pc;

    public PlayerJump(playerController controller)
    {
        _pc = controller;
    }

    // Prevents player from spam jumping
    private IEnumerator ResetJumpTrigger()
    {
        yield return null; // wait one frame
        _pc.Animator.animator.ResetTrigger("Jump");
    }

     // Handles the jumping logic
    public void HandleJumping(bool jumpTriggered, Vector3 horizontalVelocity)
    {
        if (jumpTriggered && !_jumpButtonHeldLastFrame && _pc.movement.isGrounded())
        {      
            // Reset vertical velocity before jump
            Vector3 velocity = _pc.rb.linearVelocity;
            velocity.y = 0f;
            _pc.rb.linearVelocity = velocity;

            // Capture horizontal speed at takeoff
            _pc.takeoffSpeed = horizontalVelocity.magnitude;

            _pc.Animator.animator.SetTrigger("Jump");
            _pc.StartCoroutine(ResetJumpTrigger());

            // Apply jump force
            _pc.rb.AddForce(Vector3.up * _pc.jumpForce, ForceMode.Impulse);

            // Update last jump time
            _lastJumpTime = Time.time;
        }
        _jumpButtonHeldLastFrame = jumpTriggered;
    }
}