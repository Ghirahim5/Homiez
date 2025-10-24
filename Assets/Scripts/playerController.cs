using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float crouchMultiplier = 0.5f;  
    [SerializeField] private Transform PlayerHead;
    [SerializeField] private float followSmoothness = 0.5f;

    [Header("Crouch Settings")]
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float minCrouchInterval = 0.15f;
    [SerializeField] private bool isCrouched;
    private bool crouchButtonHeldLastFrame = false;
    private float crouchLockTimer = 0f; 

    [Header("Slide Settings")]
    [SerializeField] private float slideDuration = 1.0f;
    [SerializeField] private float slideSpeed = 10.0f;
    [SerializeField] private float slideCrouchHeight = 0.8f;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private Vector3 slideDirection;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airControlMultiplier = -0.5f;
    private bool jumpButtonHeldLastFrame = false;
    private float takeoffSpeed;
    private float lastJumpTime = -Mathf.Infinity;

    [Header("Stair stepping settings")]
    [SerializeField] private GameObject feetRayPos;
    [SerializeField] private float stepBoost = 0.1f;    

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownLookRange = 90.0f;
    [SerializeField] private float verticalRotation;

    [Header ("References")]
    [SerializeField] private Rigidbody rb; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Transform playerSkin;
    [SerializeField] private Transform Center;
    [SerializeField] private CapsuleCollider capsuleCollider;

    // Animator component
    public animationStateController Animator;

    // Runs once at the start
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Keeps the player from tipping over
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Set up the player's collider size and position
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.height = standHeight;

        if (capsuleCollider != null)
        {
            capsuleCollider.center = Center.localPosition;
        }
        // Lock the mouse cursor so it doesn't wander off and hide it from view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Happens every frame
    void Update()
    {
        HandleRotation();

        if (mainCamera != null)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                PlayerHead.position,
                followSmoothness * Time.deltaTime
            );
        }
    }

    // Physics updates happen here (fixed update time = every 0.02 seconds)
    void FixedUpdate()
    {
        // Calculate horizontalVelocity
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Call movement in FixedUpdate (FPS independance)
        HandleMovement(
        horizontalVelocity,
        playerInputHandler.MovementInput,
        playerInputHandler.SprintTriggered,
        playerInputHandler.CrouchTriggered);
        
        // Additional movement "abilities"
        HandleJumping(playerInputHandler.JumpTriggered, horizontalVelocity);
        HandleCrouching(playerInputHandler.CrouchTriggered);
        if (crouchLockTimer > 0f) 
        crouchLockTimer -= Time.fixedDeltaTime;
        HandleSlide(playerInputHandler.CrouchTriggered, 
        playerInputHandler.MovementInput, horizontalVelocity);
        StairStep();
    }

    // Debugging "wireframes" to help see the various "hitboxes"
    private void OnDrawGizmos()
    {
        if (capsuleCollider == null) return;

        // Ground check sphere
        Vector3 spherePosition = feetRayPos.transform.position + Vector3.up * -0.2f;
        float sphereRadius = 0.2f;
        Gizmos.color = isGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, sphereRadius);

        // Draw Feet Ray (Lower)
        if (feetRayPos != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            Gizmos.DrawSphere(feetRayPos.transform.position, 0.05f);
            Gizmos.DrawLine(feetRayPos.transform.position, feetRayPos.transform.position + direction * 0.6f);
        }
    }

    // Prevents player from spam jumping
    private IEnumerator ResetJumpTrigger()
    {
        yield return null; // wait one frame
        Animator.animator.ResetTrigger("Jump");
    }

    // Checks if the player is standing on the ground by casting a small sphere below them
    private bool isGrounded()
    {
        float sphereRadius = 0.2f;
        Vector3 spherePosition = feetRayPos.transform.position + Vector3.up * -0.2f;
        int groundLayer = LayerMask.GetMask("Ground");

        return Physics.CheckSphere(spherePosition, sphereRadius, groundLayer);
    }

    // Handles the jumping logic
    private void HandleJumping(bool jumpTriggered, Vector3 horizontalVelocity)
    {
        if (jumpTriggered && !jumpButtonHeldLastFrame && isGrounded())
        {      
            // Reset vertical velocity before jump
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            // Capture horizontal speed at takeoff
            takeoffSpeed = horizontalVelocity.magnitude;

            Animator.animator.SetTrigger("Jump");
            StartCoroutine(ResetJumpTrigger());

            // Apply jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Update last jump time
            lastJumpTime = Time.time;
        }
        jumpButtonHeldLastFrame = jumpTriggered;
    }

    // Boosts the player up via force (physics based instead of transform position based)
    private void BoostStep()
    {
        // Get the player's forward direction relative to their skin
        Vector3 forwardDir = playerSkin.forward;

        // Apply a small forward boost along with the vertical boost (to prevent stair clipping)
        float forwardBoost = 0.3f;
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x + forwardDir.x * forwardBoost,
            stepBoost,
            rb.linearVelocity.z + forwardDir.z * forwardBoost
        );
    }


    private void StairStep()
    {
        takeoffSpeed = 5f;

        Vector2 movementInput = playerInputHandler.MovementInput;
        if (rb.linearVelocity.y < 0f || movementInput.magnitude == 0f) return;

        float rayDistance = 1f;
        Vector3 origin = feetRayPos.transform.position;
        RaycastHit hit;

        // Determine primary direction based on input
        Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 mainDir = playerSkin.TransformDirection(inputDir);

        // Cast ray only in the main movement direction
        if (Physics.Raycast(origin, mainDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
        {
            Animator.animator.SetBool("isWalking", true);
            BoostStep();
            return;
        }

        // Optional: diagonal adjustment if moving in two axes
        if (Mathf.Abs(movementInput.x) > 0 && Mathf.Abs(movementInput.y) > 0)
        {
            Vector3 diagonalDir = playerSkin.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y).normalized);
            if (Physics.Raycast(origin, diagonalDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
            {
                Animator.animator.SetBool("isWalking", true);
                BoostStep();
                return;
            }
        }
    }


    private bool CanStandUp()
    {
        float radius = capsuleCollider.radius * 0.9f;
        Vector3 colliderCenter = transform.TransformPoint(capsuleCollider.center);

        float tolerance = 0.05f; // 5 cm “headroom” buffer
        float halfHeight = (standHeight / 2f) - radius - tolerance;

        Vector3 bottom = colliderCenter - Vector3.up * halfHeight;
        Vector3 top = colliderCenter + Vector3.up * halfHeight;

        int obstacleLayer = LayerMask.GetMask("Ceiling");

        bool blocked = Physics.CheckCapsule(bottom, top, radius, obstacleLayer);
        Animator.animator.SetBool("CanStand", !blocked);

        return !blocked;
    }

    // Crouching logic
    private void HandleCrouching(bool crouchTriggered)
    {
        if (isSliding) return;

        bool crouchPressedThisFrame = crouchTriggered && !crouchButtonHeldLastFrame;
        bool canCrouch = crouchTriggered || crouchPressedThisFrame;
        crouchButtonHeldLastFrame = crouchTriggered;

        bool shouldCrouch = crouchTriggered && isGrounded();
        bool tryingToStand = !crouchTriggered && isCrouched && isGrounded();

        if (crouchLockTimer > 0f)
            crouchLockTimer -= Time.fixedDeltaTime;

        if (tryingToStand && !CanStandUp() && isGrounded())
            shouldCrouch = true; // stay crouched if blocked
   
        // Adjust collider height based on player state
        capsuleCollider.height = shouldCrouch ? crouchHeight : standHeight;
        capsuleCollider.center = shouldCrouch ? Center.localPosition - new Vector3(0, 0.5f, 0) : Center.localPosition;

        isCrouched = shouldCrouch;
        Animator.animator.SetBool("isCrouched", isCrouched);

        if (crouchPressedThisFrame && crouchLockTimer <= 0f)
            crouchLockTimer = minCrouchInterval;
    }

    private void HandleSlide(bool crouchTriggered, Vector2 movementInput, Vector2 horizontalVelocity)
    {
        // Start slide conditions
        if (!isSliding 
            && crouchTriggered 
            && horizontalVelocity.magnitude >= speed * sprintMultiplier / 2
            && isGrounded())
        {
            StartSlide(movementInput);
        }

        // Continue sliding
        if (isSliding)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0f || !isGrounded())
            {
                EndSlide();
            }
            else
            {
                rb.linearVelocity = new Vector3(slideDirection.x, rb.linearVelocity.y, slideDirection.z);
            }
        }
    }

    private void StartSlide(Vector2 movementInput)
    {
        isSliding = true;
        slideTimer = slideDuration;
        slideDirection = playerSkin.TransformDirection(new Vector3(movementInput.x, 0, movementInput.y)) * slideSpeed;
        capsuleCollider.height = slideCrouchHeight;
        //Animator.animator.SetTrigger("Slide");
    }

    private void EndSlide()
    {
        isSliding = false;

        float maxAllowedHorizontalSpeed = isCrouched ? speed * crouchMultiplier : speed;

        // Get current horizontal velocity and clamp it
        Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        horizontal = Vector3.ClampMagnitude(horizontal, maxAllowedHorizontalSpeed);

        // Apply clamped horizontal velocity while preserving vertical velocity
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);

        // Reset collider height to whatever crouch state dictates
        capsuleCollider.height = isCrouched ? crouchHeight : standHeight;

        // Clear slide direction so it doesn't accidentally persist
        slideDirection = Vector3.zero;

        takeoffSpeed = maxAllowedHorizontalSpeed;

        //Animator.animator.ResetTrigger("Slide");
    }

    private void HandleMovement(Vector3 horizontalVelocity, Vector2 movementInput,
    bool sprintTriggered, bool crouchTriggered)
    {
        if (isSliding) return;

        Vector3 inputDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 moveDirection = playerSkin.TransformDirection(inputDirection);
        float currentSpeed = speed;

        if (sprintTriggered) currentSpeed *= sprintMultiplier;
        if (isCrouched) currentSpeed *= crouchMultiplier;

        if (isGrounded())
        {
            Animator.animator.SetBool("isFalling", false);
            bool wasMoving = horizontalVelocity.magnitude > 0.05f;
            
            if (inputDirection.magnitude > 0.05f) // player is moving
            {
                Vector3 targetVelocity = moveDirection * speed;

                // Only accelerate if sprint is held (continuously)
                if (sprintTriggered && !isCrouched && wasMoving)
                {
                    targetVelocity *= sprintMultiplier;

                    // Smoothly interpolate velocity only if player was already moving
                    float sprintAcceleration = 4f;
                    Vector3 newHorizontalVelocity = Vector3.Lerp(
                        horizontalVelocity,
                        targetVelocity,
                        Time.fixedDeltaTime * sprintAcceleration
                    );

                    rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);

                    Animator.animator.SetBool("isRunning", true);
                    Animator.animator.SetBool("isWalking", false);
                }
                else
                {
                    // Smooth deceleration back to walking speed (only if velocity > walking speed)
                    float deceleration = 4f;
                    Vector3 newHorizontalVelocity = Vector3.Lerp(
                        horizontalVelocity,
                        moveDirection * speed, // walking speed
                        Time.fixedDeltaTime * deceleration
                    );
                    rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);

                    Animator.animator.SetBool("isRunning", false);
                    Animator.animator.SetBool("isWalking", true);
                }
            }
            else
            {
                // Idle
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                Animator.animator.SetBool("isWalking", false);
                Animator.animator.SetBool("isRunning", false);
            }
        }
        else
        {
            Animator.animator.SetBool("isFalling", true);
            // Air movement: always allow minimal air control
            float baseAirSpeed = Mathf.Max(takeoffSpeed, 1f); // ensure some speed even if takeoffSpeed = 0
            Vector3 targetAirVelocity = moveDirection * baseAirSpeed;

            // If no input, allow minimal control in the last horizontal direction
            if (inputDirection.magnitude == 0f)
            {
                targetAirVelocity = horizontalVelocity * 0.05f;
            }

            Vector3 newHorizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                targetAirVelocity,
                Time.fixedDeltaTime * Mathf.Abs(airControlMultiplier)
            );

            // Clamp to base air speed to avoid gaining too much speed
            newHorizontalVelocity = Vector3.ClampMagnitude(newHorizontalVelocity, baseAirSpeed);
            rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        }
    }
    

    // Rotates the player horizontally based on input
    private void ApplyHorizontalRotation(float rotationAmount)
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAmount, 0f));
    if (mainCamera != null)
    {
        mainCamera.transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }
    }

    // Rotates the camera vertically so the player can look up and down
    private void ApplyVerticalRotation(float rotationAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    // Reads input and applies the appropriate rotations to player and camera
    private void HandleRotation()
    {
        float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }   
}