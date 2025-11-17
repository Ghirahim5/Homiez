using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    #region Reference
    [Header("Reference")]
    
    [SerializeField] private Rigidbody Rb;
    [SerializeField] private Transform EnemySkin;
    [SerializeField] private Transform Target;
    [SerializeField] private NavMeshAgent EnemyAgent;
    [SerializeField] private Animator Animator;
    [SerializeField] private Vector3 StandPos;
    [SerializeField] private Transform FeetPos;
    
    public Rigidbody rb { get; private set; }
    public Transform enemySkin { get; private set; }
    public Transform target { get => Target; private set => Target = value; }
    public NavMeshAgent enemyAgent { get; private set; }
    public Animator animator { get; private set; }
    public Vector3 standPos { get => StandPos; private set => StandPos = value; }
    public Transform feetPos { get => FeetPos; private set => FeetPos = value; }

    #endregion

    #region Collider settings
    [Header("Collider settings")]
    [SerializeField] private CapsuleCollider CapsuleCollider;
    [SerializeField] private float RecoveryTime = 1f;
    [SerializeField] private float PushForce = 1f;
    public CapsuleCollider capsuleCollider { get; private set; }
    public float recoveryTime { get => RecoveryTime; private set => RecoveryTime = value; }
    public float pushForce { get => PushForce; private set => PushForce = value; }
    #endregion

    #region Agent settings
    [SerializeField] private float EnemyStandHeight = 1f;
    [SerializeField] private float EnemyStandHeightRadius = 1f;
    public float enemyStandHeightRadius { get => EnemyStandHeightRadius; private set => EnemyStandHeightRadius = value; }
    public float enemyStandHeight { get => EnemyStandHeight; private set => EnemyStandHeight = value; }

    #endregion

    #region Path Finding
    [Header("Path Finding")]
    [SerializeField] private float MovementSpeed = 1f;

    public float movementSpeed { get => MovementSpeed; private set => MovementSpeed = value; }
    #endregion

    public PathFinding path { get; private set; }
    public bool IsColliding;
    public playerController currentPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        enemyAgent = GetComponentInParent<NavMeshAgent>();
        animator = Animator;
        rb = Rb ? Rb : GetComponent<Rigidbody>();
        enemySkin = EnemySkin ? EnemySkin : transform;
        capsuleCollider = CapsuleCollider ? CapsuleCollider : GetComponent<CapsuleCollider>();

        path = new PathFinding(this);

        enemyAgent.radius = enemyStandHeightRadius;
        enemyAgent.height = enemyStandHeight;
        capsuleCollider.radius = enemyStandHeightRadius;
        capsuleCollider.height = enemyStandHeight;
        capsuleCollider.center = new Vector3(0f, enemyStandHeight / 2f, 0f);
        IsColliding = false;
    }
    void FixedUpdate()
    {
        //Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (!IsColliding)
        {

            path.HandleMovement();

        }
        else path.HandleCollisionTimer();
    }
    private void OnCollisionEnter(Collision collision)
    {
        //on contact with an object that has a player script
        playerController player = collision.transform.GetComponent<playerController>();
        if (player)
        {
            IsColliding = true;
            currentPlayer = player;
            path.CollisionHandler(player);
        }
    }

}
