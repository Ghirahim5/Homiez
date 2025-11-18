using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemyAI : MonoBehaviour
{
    #region Reference
    [Header("Reference")]
    
    //[SerializeField] private Rigidbody Rb;
    [SerializeField] private Transform EnemySkin;
    [SerializeField] private Transform Target;
    [SerializeField] private NavMeshAgent EnemyAgent;
    [SerializeField] private Animator Animator;
    [SerializeField] private Vector3 StandPos;
    [SerializeField] private Transform FeetPos;
    [SerializeField] private float TimeToResetBones;
    
    //public Rigidbody rb { get; private set; }
    public Transform enemySkin { get; private set; }
    public Transform target { get => Target; private set => Target = value; }
    public NavMeshAgent enemyAgent { get; private set; }
    public Animator animator { get; private set; }
    public Vector3 standPos { get => StandPos; private set => StandPos = value; }
    public Transform feetPos { get => FeetPos; private set => FeetPos = value; }

    #endregion

    #region Collider settings
    [Header("Collider settings")]
    //[SerializeField] private CapsuleCollider CapsuleCollider;
    [SerializeField] private Transform RagdollRoot;
    [SerializeField] private float RecoveryTime = 1f;
    [SerializeField] private float PushForce = 1f;
    [SerializeField] private float RequiredPushForce = 1f;
    //public CapsuleCollider capsuleCollider { get; private set; }
    public Transform ragdollRoot { get => RagdollRoot; private set => RagdollRoot = value; }
    public float recoveryTime { get => RecoveryTime; private set => RecoveryTime = value; }
    public float pushForce { get => PushForce; private set => PushForce = value; }
    public float requiredPushForce { get => RequiredPushForce; private set => RequiredPushForce = value; }
    public Rigidbody[] RagdollRigidbodies;
    public CharacterJoint[] Joints;
    public Rigidbody mainBone;
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
    public CollisionHandler collisionHandler {get; private set;}
    public StateHandler stateHandler {get; private set;}
    public bool StartRagdoll = false;
    public playerController currentPlayer;
    public RagdollCollisionHandler relay;
    public enum EnemyState
    {
        Chasing,
        Ragdoll,
        Attacking, 
        StandUp,
        ResetBones
    }
    private Transform hipsBone;
    public EnemyState currentState = EnemyState.Chasing;

    void Awake()
    {
        collisionHandler = new CollisionHandler(this);
        stateHandler = new StateHandler(this);
        enemyAgent = GetComponentInParent<NavMeshAgent>();
        animator = Animator;
        enemySkin = EnemySkin ? EnemySkin : transform;
        
        RagdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();


        foreach (var rb in RagdollRigidbodies)
        {
            var handler = rb.gameObject.AddComponent<RagdollCollisionHandler>();
            handler.Init(this);

            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
            path = new PathFinding(this);

    }
    void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Chasing:
                {
                    path.HandleMovement();
                    break;
                }
            case EnemyState.Attacking:
                {
                    stateHandler.Attack();
                    break;
                }
            case EnemyState.Ragdoll: 
                {
                    collisionHandler.HandleCollisionTimer();
                    break;
                }
            case EnemyState.StandUp:
                {
                    stateHandler.StandUp();
                    break;
                }
        }
    }
    public void OnRagdollHit(Collision collision, playerController player)
    {
        if (!StartRagdoll)
        {
            currentPlayer = player;
            collisionHandler.Collision(player, collision);
        }
    }

}
