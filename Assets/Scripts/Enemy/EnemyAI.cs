using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using UnityEngine.AI;
using Unity.Mathematics;
using NUnit.Framework;
using JetBrains.Annotations;

public class EnemyAI : MonoBehaviour
{
    #region Reference
    [Header("Reference")]
    
    [SerializeField] private Transform EnemySkin;
    [SerializeField] private NavMeshAgent EnemyAgent;
    [SerializeField] private Animator Animator;

    
    public Transform enemySkin { get; private set; }
    public NavMeshAgent enemyAgent { get; private set; }
    public Animator animator { get; private set; }

    #endregion

    #region Main Collider/Rigidbody settings
    [Header("Main Collider/Rigidbody settings")]
    [SerializeField] private CapsuleCollider MainCollider;
    [SerializeField] private Rigidbody MainRigidbody;
    [SerializeField] private Vector3 StandPos;
    [SerializeField] private Transform FeetPos;
    [SerializeField] private Transform CenterPos;
    [SerializeField] private float StandHeight = 1f;
    [SerializeField] private float EnemyStandHeightRadius = 1f;
 
    public CapsuleCollider mainCollider { get; private set; }
    public Rigidbody mainRigidbody { get; private set; }
    public Vector3 standPos { get => StandPos; private set => StandPos = value; }
    public Transform feetPos { get => FeetPos; private set => FeetPos = value; }
    public Transform centerPos { get => CenterPos; private set => CenterPos = value; }
    public float standHeight { get => StandHeight; private set => StandHeight = value; }
    public float enemyStandHeightRadius { get => EnemyStandHeightRadius; private set => EnemyStandHeightRadius = value; }
    #endregion


    #region AI logic
    [Header("AI logic")]
    [SerializeField] private GameObject Target;
    [SerializeField] private float AttackRange = 1f;
    [SerializeField] private float ChaseSpeed = 1f;

    public playerController currentPlayer;
    public GameObject target { get => Target; private set => Target = value; }
    public float attackRange { get => AttackRange; private set => AttackRange = value; }
    public float chaseSpeed { get => ChaseSpeed; private set => ChaseSpeed = value; }
    #endregion

    #region Attack Settings
    [Header("Attack settings")]
    [SerializeField] private Collider AttackHitbox;
    [SerializeField] private Rigidbody AttackRigidbody;
    [SerializeField] private float AttackDamage = 1f;
    public Collider attackHitbox {get; private set;} 
    public Rigidbody attackRigidbody {get; private set;}
    public float attackDamage { get => AttackDamage; private set => AttackDamage = value; }
    #endregion

    public CollisionHandler collisionHandler {get; private set;}
    #region Ragdoll settings
    [Header("Ragdoll settings")]
    [SerializeField] private Transform RagdollRoot;
    [SerializeField] private Rigidbody MainBone;
    [SerializeField] private float RecoveryTime = 1f;
    [SerializeField] private float PushForce = 1f;
    [SerializeField] private float RequiredPushForce = 1f;
    public Rigidbody[] RagdollRigidbodies;
    public Collider[] RagdollColliders;

    public Transform ragdollRoot { get => RagdollRoot; private set => RagdollRoot = value; }
    public Rigidbody mainBone {get; private set;}
    public float recoveryTime { get => RecoveryTime; private set => RecoveryTime = value; }
    public float pushForce { get => PushForce; private set => PushForce = value; }
    public float requiredPushForce { get => RequiredPushForce; private set => RequiredPushForce = value; }
    public bool StartRagdoll = false;
    public float collisionTimer = 0f;
    #endregion

    EnemyBaseState CurrentState;
    EnemyStateFactory states;
    public EnemyBaseState currentState { get { return CurrentState; } set { CurrentState = value; } }
    void Awake()
    {
        mainRigidbody = MainRigidbody ? MainRigidbody : GetComponent<Rigidbody>();
        mainCollider = MainCollider ? MainCollider : GetComponent<CapsuleCollider>();

        mainBone = MainBone ? MainBone : GetComponent<Rigidbody>();
        RagdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        RagdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();

        attackHitbox = AttackHitbox ? AttackHitbox : GetComponent<Collider>();
        attackRigidbody = AttackRigidbody ? AttackRigidbody : GetComponent<Rigidbody>();
        
        target = Target ? Target : GetComponent<GameObject>();
        enemyAgent = GetComponentInParent<NavMeshAgent>();
        animator = Animator;
        enemySkin = EnemySkin ? EnemySkin : transform;

        mainCollider.height = standHeight;
        mainRigidbody.freezeRotation = true; 
        mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        if (mainCollider != null)
        {
            mainCollider.center = new Vector3(0f, StandHeight/2, 0f);
        }

        collisionHandler = new CollisionHandler(this);
        states = new EnemyStateFactory(this);
        currentState = states.Chase();

    }
       void FixedUpdate()
    {
        currentState.UpdateState();
    }
    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.transform.GetComponent<playerController>();
        if (player != null)
        {
            currentPlayer = player;
            collisionHandler.Collision(currentPlayer);
        }
    }

}
