using UnityEngine;

public class PlayerSlide
{
    private float _slideTimer = 0f;
    private Vector3 _slideDirection;

    private playerController _pc;

    public PlayerSlide(playerController controller)
    {
        _pc = controller;
    }

    public void HandleSlide(bool crouchTriggered, Vector2 movementInput, Vector2 horizontalVelocity)
    {
        // Start slide conditions
        if (!_pc.isSliding 
            && crouchTriggered 
            && horizontalVelocity.magnitude >= _pc.speed * _pc.sprintMultiplier / 2
            && _pc.movement.isGrounded())
        {
            StartSlide(movementInput);
        }

        if (_pc.isSliding)
        {
            // If timer runs out end the slide
            _slideTimer -= Time.fixedDeltaTime;
            if (_slideTimer <= 0f || !_pc.movement.isGrounded())
            {
                EndSlide();
            }
            else
            {
                // Continue sliding
                _pc.rb.linearVelocity = new Vector3(_slideDirection.x, _pc.rb.linearVelocity.y, _slideDirection.z);
            }
        }
    }

    private void StartSlide(Vector2 movementInput)
    {
        _pc.isSliding = true;
        _slideTimer = _pc.slideDuration;
        _slideDirection = _pc.playerSkin.TransformDirection(new Vector3(movementInput.x, 0, movementInput.y)) * _pc.slideSpeed;
        _pc.capsuleCollider.height = _pc.slideCrouchHeight;
        //_pc.Animator.animator.SetTrigger("Slide");
    }

    private void EndSlide()
    {
        _pc.isSliding = false;

        // Calculate max speed based on crouch
        float maxAllowedHorizontalSpeed = _pc.isCrouched ? _pc.speed * _pc.crouchMultiplier : _pc.speed;

        // Get current horizontal velocity and clamp it
        Vector3 horizontal = new Vector3(_pc.rb.linearVelocity.x, 0f, _pc.rb.linearVelocity.z);
        horizontal = Vector3.ClampMagnitude(horizontal, maxAllowedHorizontalSpeed);

        // Apply clamped horizontal velocity while preserving vertical velocity
        _pc.rb.linearVelocity = new Vector3(horizontal.x, _pc.rb.linearVelocity.y, horizontal.z);
        _pc.capsuleCollider.height = _pc.isCrouched ? _pc.crouchHeight : _pc.standHeight; // Reset collider height
        _slideDirection = Vector3.zero; // Clear slide direction

        _pc.takeoffSpeed = maxAllowedHorizontalSpeed;
        //pc.Animator.animator.ResetTrigger("Slide");
    }

}