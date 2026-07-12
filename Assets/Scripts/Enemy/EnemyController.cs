using MoreMountains.Feedbacks;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyDetector detector;
    [SerializeField] private Health health;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Combat Tuning")]
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float detectDelay = 0.5f; // "alert" pause before first attack

    [Header("Projectile Tuning")]
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Feedbacks")]
    [SerializeField] public MMF_Player idleFeedback;
    [SerializeField] public MMF_Player detectFeedback;
    [SerializeField] public MMF_Player attackFeedback;
    [SerializeField] public MMF_Player hitfeedback;

    public EnemyDetector Detector => detector;
    public Health Health => health;
    public Transform FirePoint => firePoint;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float DetectDelay => detectDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDamage => projectileDamage;
    public float ProjectileLifetime => projectileLifetime;

    private Transform _playerTarget;
    public Transform PlayerTarget
    {
        get => _playerTarget;
        set
        {
            _playerTarget = value;
            PlayerVelocityTracker = value ? value.GetComponent<VelocityTracker>() : null;
        }
    }

    public VelocityTracker PlayerVelocityTracker { get; private set; }

    private EnemyStateMachine _stateMachine;

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();
        _stateMachine = new EnemyStateMachine(this);
    }

    private void Start()
    {
        _stateMachine.ChangeState(new EnemyIdleState());
    }

    private void Update()
    {
        _stateMachine.Tick();
    }

    public void ChangeState(IEnemyState newState) => _stateMachine.ChangeState(newState);
}