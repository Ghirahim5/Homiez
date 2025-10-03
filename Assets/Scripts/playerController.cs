using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;


    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;
    [SerializeField] private float airResistance = 0.02f;
    [SerializeField] private float groundFriction = 8.0f;


    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownLookRange = 90.0f;

    [Header ("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;


    private Vector3 currentMovement;
    private Vector3 momentumDirection;
    private Vector3 cachedWorldDirection;
    private bool worldDirectionDirty = true;
    private float verticalRotation;
    private float momentumSpeed;

    private float currentSpeed 
    {
        get
        {
            if (playerInputHandler.MovementInput.magnitude < 0.1f) return 0f;
            return walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1);
        }
    }

    private Vector3 CalculateWorldDirection()
    {
        // Only recalculate if the cache is "dirty" (outdated)
        if (worldDirectionDirty)
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
            cachedWorldDirection = transform.TransformDirection(inputDirection).normalized;
            worldDirectionDirty = false;
        }
        return cachedWorldDirection;
    }   


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // Update is called once per frame
    void Update()
    {
        worldDirectionDirty = true; // Mark the cache as dirty
        HandleMovement();
        HandleRotation();

        if (!characterController.isGrounded) 
        {
            float airDecay = 1f - (airResistance * Time.deltaTime * 60f);
            momentumSpeed *= airDecay;
        }
        else
        {
            momentumSpeed = Mathf.MoveTowards(momentumSpeed, currentSpeed, groundFriction * Time.deltaTime);
        }
    }


    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if (playerInputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
                // Capture momentum when jumping
                Vector3 currentWorldDirection = CalculateWorldDirection();
                if (currentWorldDirection.magnitude > 0.1f)
                {
                    momentumDirection = currentWorldDirection;
                    momentumSpeed = currentSpeed;
                }
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }


    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        float speed = currentSpeed; // Cache current speed
        
        if (characterController.isGrounded)
        {
            currentMovement.x = worldDirection.x * speed;
            currentMovement.z = worldDirection.z * speed;

            if(worldDirection.magnitude > 0.1f)
            {
                momentumDirection = worldDirection;
                momentumSpeed = speed;
            }
        }
        else
        {
            currentMovement.x = momentumDirection.x * momentumSpeed;
            currentMovement.z = momentumDirection.z * momentumSpeed;
        }

        HandleJumping();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    
    private void ApplyHorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }


    private void ApplyVerticalRotation(float rotationAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }


    private void HandleRotation()
    {
        float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;


        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

}
