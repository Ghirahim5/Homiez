using UnityEngine;

public class PlayerMovement
{

    private bool _isGroundedState = false;
    private float _sprintHoldTime = 0f;
    private float _sprintHoldThreshold = 0.2f;
    public bool isSprinting;

    private playerController _pc;
    
    public PlayerMovement(playerController controller)
    {
        _pc = controller;
    }

    // Debugging "wireframes" to help see the various "hitboxes"
    public void DrawGizmos()
    {        
        if (_pc.capsuleCollider == null) return;
        Gizmos.color = _pc.movement.isGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_pc.feetRayPos, 0.3f);

        // Draw Feet Ray (Lower)
        if (_pc.feetRayPos != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = _pc.transform.TransformDirection(Vector3.forward);
            Gizmos.DrawLine(_pc.feetRayPos, _pc.feetRayPos + direction * 0.6f);
        }
    }

    // Checks if the player is standing on the ground by casting a small sphere below them
    public bool isGrounded()
    {
        float sphereRadius = 0.3f;

        int groundLayer = LayerMask.GetMask("Ground", "Stairs");
        Vector3 spherePosition = _pc.feetRayPos;

        _isGroundedState = Physics.CheckSphere(spherePosition, sphereRadius, groundLayer);
        return _isGroundedState;
    }
    
    // The "brains" of core movement mechanics
    public void HandleMovement(Vector3 horizontalVelocity, Vector2 movementInput,
        bool sprintTriggered, bool crouchTriggered)
    {
        if (_pc.isSliding) return;

        Vector3 inputDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 moveDirection = Vector3.zero;
        if (_pc.playerSkin != null)
            moveDirection = _pc.playerSkin.TransformDirection(inputDirection);
        else
            moveDirection = _pc.transform.TransformDirection(inputDirection); // fallback

        float walkSpeed = _pc.speed;
        if (_pc.isCrouched)
            walkSpeed *= _pc.crouchMultiplier;

        float sprintSpeed = _pc.speed * _pc.sprintMultiplier;

        if (sprintTriggered)
        {
            _sprintHoldTime += Time.fixedDeltaTime;
        }
        else
        {
            _sprintHoldTime = 0f;
        }

        bool canSprint = _sprintHoldTime >= _sprintHoldThreshold;
        isSprinting = canSprint && !_pc.isCrouched && inputDirection.magnitude > 0.05f;

        if (isGrounded())
        {
            _pc.Animator.animator.SetBool("isFalling", false);

            // Current horizontal velocity (from Rigidbody)
            Vector3 currentHorizontalVelocity = horizontalVelocity;

            // If there is movement input
            if (inputDirection.magnitude > 0.05f)
            {
                // Determine desired target for this frame depending on sprint/walk state
                if (isSprinting)
                {
                    // Desired direction and target sprint speed
                    Vector3 currentDir = moveDirection.normalized;
                    float currentSpeed = currentHorizontalVelocity.magnitude;
                    float newSpeed = Mathf.Lerp(
                        currentSpeed, 
                        sprintSpeed, 
                        Time.fixedDeltaTime * _pc.sprintAcceleration
                    );
                    // Keep direction responsive
                    Vector3 newVelocity = currentDir * newSpeed;
                    _pc.rb.linearVelocity = new Vector3(newVelocity.x, _pc.rb.linearVelocity.y, newVelocity.z);

                    // Animator
                    _pc.Animator.animator.SetBool("isRunning", true);
                    _pc.Animator.animator.SetBool("isWalking", false);
                }
                else
                {
                    // Not sprinting = walking is instant
                    Vector3 targetWalkVelocity = moveDirection * walkSpeed;

                    // If we were previously moving faster than walkSpeed (coming off a sprint),
                    // smoothly decelerate toward walking speed. Otherwise snap to walking speed.
                    if (!isSprinting && !canSprint && currentHorizontalVelocity.magnitude > walkSpeed + 0.05f)
                    {
                        Vector3 currentDir = moveDirection.normalized;
                        float currentSpeed = currentHorizontalVelocity.magnitude;
                        float newSpeed = Mathf.Lerp(
                            currentSpeed,
                            walkSpeed,
                            Time.fixedDeltaTime * _pc.sprintAcceleration
                        );
                        Vector3 newVelocity = currentDir * newSpeed;
                        _pc.rb.linearVelocity = new Vector3(newVelocity.x, _pc.rb.linearVelocity.y, newVelocity.z);
                    }
                    else
                    {
                        _pc.rb.linearVelocity = new Vector3(targetWalkVelocity.x, _pc.rb.linearVelocity.y, targetWalkVelocity.z);
                    }

                    // Animator
                    _pc.Animator.animator.SetBool("isRunning", false);
                    _pc.Animator.animator.SetBool("isWalking", true);
                }
            }
            else
            {
                // No input: smoothly decelerate to zero horizontal velocity
                Vector3 newHorizontalVelocity = Vector3.Lerp(
                    currentHorizontalVelocity,
                    Vector3.zero,
                    Time.fixedDeltaTime * Mathf.Max(0.0001f, _pc.sprintDeceleration)
                );
                _pc.rb.linearVelocity = new Vector3(newHorizontalVelocity.x, _pc.rb.linearVelocity.y, newHorizontalVelocity.z);

                _pc.Animator.animator.SetBool("isWalking", false);
                _pc.Animator.animator.SetBool("isRunning", false);
            }
        }
        else
        {
            // Airborne: minimal air control
            _pc.Animator.animator.SetBool("isFalling", true);

            float baseAirSpeed = Mathf.Max(_pc.takeoffSpeed, 1f);
            Vector3 targetAirVelocity = moveDirection * baseAirSpeed;

            if (inputDirection.magnitude == 0f)
            {
                targetAirVelocity = horizontalVelocity * 0.05f;
            }

            Vector3 newHorizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                targetAirVelocity,
                Time.fixedDeltaTime * Mathf.Abs(_pc.airControlMultiplier)
            );

            newHorizontalVelocity = Vector3.ClampMagnitude(newHorizontalVelocity, baseAirSpeed);
            _pc.rb.linearVelocity = new Vector3(newHorizontalVelocity.x, _pc.rb.linearVelocity.y, newHorizontalVelocity.z);
        }
    }
}