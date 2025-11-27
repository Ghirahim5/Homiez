using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class playerController : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private Rigidbody Rb;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private PlayerInputHandler PlayerInputHandler;
    [SerializeField] private Transform PlayerSkin;
    [SerializeField] private Transform Center;
    [SerializeField] private CapsuleCollider CapsuleCollider;

    public Rigidbody rb { get; private set; }
    public Camera mainCamera { get; private set; }
    public PlayerInputHandler playerInputHandler { get; private set; }
    public Transform playerSkin { get; private set; }
    public Transform center { get; private set; }
    public CapsuleCollider capsuleCollider { get; private set; }
    public Vector3 feetRayPos => isCrouched 
    ? Center.position - Vector3.up * 0.65f 
    : Center.position - Vector3.up * 0.8f;
    #endregion

    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float Speed = 2.0f;
    [SerializeField] private float SprintMultiplier = 1.5f;
    [SerializeField] private float SprintAcceleration = 10f;
    [SerializeField] private float SprintDeceleration = 10f;
    [SerializeField] private float CrouchMultiplier = 0.5f;  

    public float speed { get => Speed; private set => Speed = value; }
    public float sprintMultiplier { get => SprintMultiplier; private set => SprintMultiplier = value; }
    public float crouchMultiplier { get => CrouchMultiplier; private set => CrouchMultiplier = value; }
    public float sprintAcceleration { get => SprintAcceleration; private set => SprintAcceleration = value; }
    public float sprintDeceleration { get => SprintDeceleration; private set => SprintDeceleration = value; }
    #endregion

    #region Jump Settings
    [Header("Jump Settings")]
    [SerializeField] private float JumpForce = 5.0f;
    [SerializeField] private float AirControlMultiplier = -0.5f;
    [SerializeField] private float TakeoffSpeed;

    public float jumpForce { get => JumpForce; private set => JumpForce = value; }
    public float airControlMultiplier { get => AirControlMultiplier; private set => AirControlMultiplier = value; }
    public float takeoffSpeed { get => TakeoffSpeed; set => TakeoffSpeed = value; }
    #endregion

    #region Slide Settings
    [Header("Slide Settings")]
    [SerializeField] private float SlideDuration = 1.0f;
    [SerializeField] private float SlideSpeed = 10.0f;
    [SerializeField] private float SlideCrouchHeight = 0.8f;
    [SerializeField] private bool IsSliding = false;

    public float slideDuration { get => SlideDuration; private set => SlideDuration = value; }
    public float slideSpeed { get => SlideSpeed; private set => SlideSpeed = value; }
    public float slideCrouchHeight { get => SlideCrouchHeight; private set => SlideCrouchHeight = value; }
    public bool isSliding { get => IsSliding; set => IsSliding = value; }
    #endregion

    #region Stair Settings
    [Header("Stair stepping settings")]
    [SerializeField] private float StepBoost = 0.0f; 
    
    public float stepBoost { get => StepBoost; private set => StepBoost = value; }
    #endregion

    #region Look Settings
    [Header("Look Settings")]
    [SerializeField] private float MouseSensitivity = 2.0f;
    [SerializeField] private float UpDownLookRange = 90.0f;
    [SerializeField] private float VerticalRotation;
    [SerializeField] Transform PlayerHead;

    public float mouseSensitivity { get => MouseSensitivity; private set => MouseSensitivity = value; }
    public float upDownLookRange { get => UpDownLookRange; private set => UpDownLookRange = value; }
    public float verticalRotation { get => VerticalRotation; set => VerticalRotation = value; }
    #endregion

    #region Crouch Settings
    [Header("Crouch Settings")]
    [SerializeField] private float StandHeight = 7.0f;
    [SerializeField] private float CrouchHeight = 3.5f;
    [SerializeField] private float MinCrouchInterval = 0.15f;
    [SerializeField] private bool IsCrouched;
    [SerializeField] Transform PlayerHeadTop;

    public float standHeight { get => StandHeight; private set => StandHeight = value; }
    public float crouchHeight { get => CrouchHeight; private set => CrouchHeight = value; }
    public float minCrouchInterval { get => MinCrouchInterval; private set => MinCrouchInterval = value; }
    public bool isCrouched { get => IsCrouched; set => IsCrouched = value; }
    public Transform playerHeadTop { get => PlayerHeadTop; private set => PlayerHeadTop = value; }
    #endregion

    #region Health Settings
    [Header("Health Settings")]
    [SerializeField] private int CurrentHealth = 20;
    [SerializeField] private int MaxHealth = 20;
    [SerializeField] private Image[] Hearts;
    public int currentHealth { get => CurrentHealth; set => CurrentHealth = value; }
    public int maxHealth { get => MaxHealth; private set => MaxHealth = value; }
    public Image[] hearts { get => Hearts; private set => Hearts = value; }

    #endregion  
    // Animator
    public animationStateController Animator { get; private set; }

    // Subsystems
    public PlayerHealth health { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerJump jump { get; private set; }
    public PlayerCrouch crouch { get; private set; }
    public PlayerSlide slide { get; private set; }
    public PlayerRotation rotation { get; private set; }
    public PlayerStairStepSystem stairStepSystem { get; private set; }

    void Awake()
    {
        Animator = GetComponent<animationStateController>();
        rb = Rb ? Rb : GetComponent<Rigidbody>();
        capsuleCollider = CapsuleCollider ? CapsuleCollider : GetComponent<CapsuleCollider>();
        playerSkin = PlayerSkin ? PlayerSkin : transform;
        mainCamera = MainCamera ? MainCamera : Camera.main;
        playerInputHandler = PlayerInputHandler ? PlayerInputHandler : GetComponent<PlayerInputHandler>();

        // Initialize rigidbody settings
        rb.freezeRotation = true; // Keeps the player from tipping over
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Set up the player's collider heght
        capsuleCollider.height = standHeight;

        if (capsuleCollider != null)
            capsuleCollider.center = Center.localPosition;

        // Lock and hide the mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize subsystems
        health = new PlayerHealth(this);
        movement = new PlayerMovement(this);
        jump = new PlayerJump(this);
        crouch = new PlayerCrouch(this);
        slide = new PlayerSlide(this);
        rotation = new PlayerRotation(this);
        stairStepSystem = new PlayerStairStepSystem(this);
    }

    // Runs once at the start
    void Start()
    {
        health.InitializeHearts();
    }

    // Happens every frame
    void Update()
    {
        rotation.HandleRotation();

        if (mainCamera != null)
            mainCamera.transform.position = PlayerHead.position;

    }

    // Physics updates happen here (fixed update time = every 0.02 seconds)
    void FixedUpdate()
    {
        // Calculate horizontalVelocity
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Call movement in FixedUpdate
        movement.HandleMovement(
        horizontalVelocity,
        playerInputHandler.MovementInput,
        playerInputHandler.SprintTriggered,
        playerInputHandler.CrouchTriggered);

        // Additional "abilities"
        jump.HandleJumping(playerInputHandler.JumpTriggered, horizontalVelocity);
        crouch.HandleCrouching(playerInputHandler.CrouchTriggered);
        slide.HandleSlide(playerInputHandler.CrouchTriggered, 
        playerInputHandler.MovementInput, horizontalVelocity);
        stairStepSystem.StairStep();
        health.UpdateCurrentHealth();
    }

    // Draw the wireframes
    private void OnDrawGizmos()
    {
        if (movement != null)
            movement.DrawGizmos();
    }
    private void OnCollisionEnter(Collision collision)
    {
        var dmgcollider = collision.transform.GetComponent<DamageCollider>();
        if (dmgcollider != null)
        {
            health.HandleDamage(dmgcollider);
        }
    }
}