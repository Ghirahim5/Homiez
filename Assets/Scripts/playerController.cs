using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour
{
    // Settings
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;
    [SerializeField] private float crouchMultiplier = 0.5f;  
    [SerializeField] private float groundFriction = 8.0f;
    [SerializeField] private float downwardForce = 10f;
    

    [Header("Crouch Settings")]
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standCameraHeight = 1.5f;
    [SerializeField] private float crouchCameraHeight = 1.0f;
    [SerializeField] private bool isCrouched;


    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airControlMultiplier = 5.0f;


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


    // Physics updates happen here
    void FixedUpdate()
    {
        HandleMovement(
        playerInputHandler.MovementInput,
        playerInputHandler.SprintTriggered,
        playerInputHandler.CrouchTriggered);
        HandleJumping(playerInputHandler.JumpTriggered);
        HandleCrouching(playerInputHandler.CrouchTriggered);

        // Smoothly move the camera up or down based on whether we're crouching
        float targetCameraHeight = isCrouched ? crouchCameraHeight : standCameraHeight;
        Vector3 cameraPos = mainCamera.transform.localPosition;
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetCameraHeight, Time.fixedDeltaTime * 10f);
        mainCamera.transform.localPosition = cameraPos;
    }


    // Gizmo to help us see if we're touching the ground in the editor
    private void OnDrawGizmos()
    {
        if (playerSkin == null) return;

        Vector3 spherePosition = transform.position + Vector3.down * (capsuleCollider.height / 2f - 0.2f);
        float sphereRadius = 0.25f;

        // Green means grounded, red means not grounded
        Gizmos.color = isGrounded() ? Color.green : Color.red;

        // Draw the wireframe sphere in the scene view
        Gizmos.DrawWireSphere(spherePosition, sphereRadius);
    }


    // Checks if the player is standing on the ground by casting a small sphere below them
    private bool isGrounded()
    {
        float sphereRadius = 0.25f;
        float groundOffset = standHeight / 2f - 0.2f;
        Vector3 spherePosition = transform.position + Vector3.down * groundOffset;
        int groundLayer = LayerMask.GetMask("Ground");

        return Physics.CheckSphere(spherePosition, sphereRadius, groundLayer);
    }


    private void HandleJumping(bool jumpTriggered)
    {
        if (jumpTriggered && isGrounded())
        {
            // Getting stuck on narrow gaps fix
            if (!isCrouched && !CanStandUp()) return;

            // Reset any downward or upward velocity before jumping
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            // Add an instant upward force to make the player jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        // Decide if we should crouch based on input or if there's no space to stand
        bool shouldCrouch = crouchTriggered || !CanStandUp();

        // Smoothly adjust the player's collider size and position for crouching
        float targetHeight = shouldCrouch ? crouchHeight : standHeight;
        Vector3 targetCenter = shouldCrouch ? new Vector3(0, -0.5f, 0) : Vector3.zero;

        capsuleCollider.height = targetHeight;
        capsuleCollider.center = targetCenter;

        isCrouched = shouldCrouch;
    }


    private void HandleMovement(Vector2 movementInput, bool sprintTriggered, bool crouchTriggered)
    {
        Vector3 inputDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 moveDirection = playerSkin.TransformDirection(inputDirection);
        float currentSpeed = speed;

        // Adjust speed if sprinting or crouching
        if (sprintTriggered) currentSpeed *= sprintMultiplier;
        if (isCrouched) currentSpeed *= crouchMultiplier;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (isGrounded())
        {
            // When on the ground, directly set horizontal velocity if there's input
            if (inputDirection.magnitude > 0f)
            {
                Vector3 moveVelocity = moveDirection * currentSpeed;
                rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
            }
            else
            {
                // If no input, apply friction to slow down smoothly
                Vector3 frictionVelocity = horizontalVelocity * Mathf.Max(0f, 1f - groundFriction * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector3(frictionVelocity.x, rb.linearVelocity.y, frictionVelocity.z);
            }
            // If no input, keep current momentum going
        }
        else
        {
            // In the air, apply input as acceleration instead of direct velocity changes
            if (inputDirection.magnitude > 0f)
            {
                Vector3 moveVelocity = moveDirection * currentSpeed;
                Vector3 velocityChange = moveVelocity - horizontalVelocity;
                rb.AddForce(velocityChange * airControlMultiplier, ForceMode.Acceleration);
            }
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
