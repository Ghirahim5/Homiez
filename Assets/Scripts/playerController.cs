using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float crouchMultiplier = 0.5f;  


    [Header("Stair stepping settings")]
    [SerializeField] private GameObject feetRayPos;
    [SerializeField] private float stepBoost = 0.1f;

    
    [Header("Crouch Settings")]
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standCameraHeight = 1.5f;
    [SerializeField] private float crouchCameraHeight = 1.0f;
    [SerializeField] private bool isCrouched;


    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airControlMultiplier = -0.5f;
    private bool jumpButtonHeldLastFrame = false;
    private float takeoffSpeed;


    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownLookRange = 90.0f;
    [SerializeField] private float verticalRotation;


    [Header ("References")]
    [SerializeField] private Rigidbody rb; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Transform playerSkin;
    [SerializeField] private CapsuleCollider capsuleCollider;


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
        capsuleCollider.center = Vector3.zero;

        // Lock the mouse cursor so it doesn't wander off and hide it from view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called every frame
    void Update()
    {
        HandleRotation();
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
        HandleJumping(playerInputHandler.JumpTriggered);
        HandleCrouching(playerInputHandler.CrouchTriggered);
        StairStep();

        // Smoothly move the camera up or down based on whether the player is crouching
        float targetCameraHeight = isCrouched ? crouchCameraHeight : standCameraHeight;
        Vector3 cameraPos = mainCamera.transform.localPosition;
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetCameraHeight, Time.fixedDeltaTime * 10f);
        mainCamera.transform.localPosition = cameraPos;
    }

    // Debugging "wireframes" to help see the various "hitboxes"
    private void OnDrawGizmos()
    {
        if (capsuleCollider == null) return;

        // Ground check sphere
        Vector3 spherePosition = transform.position + Vector3.down * (capsuleCollider.height / 2f - 0.35f);
        float sphereRadius = 0.4f;
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

    // Checks if the player is standing on the ground by casting a small sphere below them
    private bool isGrounded()
    {
        float sphereRadius = 0.4f;
        float groundOffset = standHeight / 2f - 0.35f;
        Vector3 spherePosition = transform.position + Vector3.down * groundOffset;
        int groundLayer = LayerMask.GetMask("Ground");

        return Physics.CheckSphere(spherePosition, sphereRadius, groundLayer);
    }

    private void HandleJumping(bool jumpTriggered)
    {
        if (jumpTriggered && !jumpButtonHeldLastFrame && isGrounded())
        {
            if (!isCrouched && !CanStandUp()) return;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            // Capture the horizontal speed at the moment of takeoff for use in air
            Vector3 horizontalVelocityBeforeJump = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            takeoffSpeed = horizontalVelocityBeforeJump.magnitude;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        jumpButtonHeldLastFrame = jumpTriggered;
    }

    private void BoostStep()
    {
        // Get the player's forward direction relative to their skin
        Vector3 forwardDir = playerSkin.forward;

        // Apply a small forward boost along with the vertical boost
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

        float rayDistance = 0.8f;
        Vector3 origin = feetRayPos.transform.position;
        RaycastHit hit;

        // Determine primary direction based on input
        Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 mainDir = playerSkin.TransformDirection(inputDir);

        // Cast ray only in the main movement direction
        if (Physics.Raycast(origin, mainDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
        {
            BoostStep();
            return;
        }

        // Optional: diagonal adjustment if moving in two axes
        if (Mathf.Abs(movementInput.x) > 0 && Mathf.Abs(movementInput.y) > 0)
        {
            Vector3 diagonalDir = playerSkin.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y).normalized);
            if (Physics.Raycast(origin, diagonalDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
            {
                BoostStep();
                return;
            }
        }
    }


    // Checks if there's enough headroom for the player to stand up safely
    private bool CanStandUp()
    {
        float radius = capsuleCollider.radius * 0.9f;
        Vector3 bottom = transform.position + Vector3.up * radius;
        Vector3 top = transform.position + Vector3.up * (standHeight - radius);
        int obstacleLayer = LayerMask.GetMask("Ground");

        // Returns true if no obstacles are blocking the space above the player
        return !Physics.CheckCapsule(bottom, top, radius, obstacleLayer);
    }


    private void HandleCrouching(bool crouchTriggered)
    {
        // Prevent crouching midair: only allow if grounded
        bool shouldCrouch = (crouchTriggered && isGrounded()) || !CanStandUp();

        // Smoothly adjust the player's collider size and position for crouching
        float targetHeight = shouldCrouch ? crouchHeight : standHeight;
        Vector3 targetCenter = shouldCrouch ? new Vector3(0, -0.5f, 0) : Vector3.zero;

        capsuleCollider.height = targetHeight;
        capsuleCollider.center = targetCenter;

        isCrouched = shouldCrouch;
    }


    private void HandleMovement(Vector3 horizontalVelocity, Vector2 movementInput, bool sprintTriggered, bool crouchTriggered)
    {
        Vector3 inputDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 moveDirection = playerSkin.TransformDirection(inputDirection);
        float currentSpeed = speed;

        if (sprintTriggered) currentSpeed *= sprintMultiplier;
        if (isCrouched) currentSpeed *= crouchMultiplier;

        if (isGrounded())
        {
            if (inputDirection.magnitude > 0f)
            {
                Vector3 targetVelocity = moveDirection * currentSpeed;

                if (sprintTriggered)
                {
                    float sprintAcceleration = 3.3f;
                    Vector3 newHorizontalVelocity = Vector3.Lerp(
                        horizontalVelocity,
                        targetVelocity,
                        Time.fixedDeltaTime * sprintAcceleration
                    );
                    rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
                }
                else
                {
                    rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
                }
            }
            else
            {
                // No input, stay in place on the ground
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
        }
        else
        {
            // Air movement: always allow minimal air control
            float baseAirSpeed = Mathf.Max(takeoffSpeed, 1f); // ensure some speed even if takeoffSpeed = 0
            Vector3 targetAirVelocity = moveDirection * baseAirSpeed;

            // If no input, allow minimal control in the last horizontal direction
            if (inputDirection.magnitude == 0f)
            {
                targetAirVelocity = horizontalVelocity * 0.05f; // tiny nudge
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
