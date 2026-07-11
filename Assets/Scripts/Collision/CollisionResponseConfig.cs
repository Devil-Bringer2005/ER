namespace EndlessRunner.Player.Collision
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Player/Collision/Collision Response Config", fileName = "CollisionResponseConfig")]
    public class CollisionResponseConfig : ScriptableObject
    {
        [Header("Classification")]
        [Tooltip("Obstacle layers this handler reacts to. Exclude your ground/floor layer.")]
        [SerializeField] private LayerMask _obstacleLayers = ~0;

        [Tooltip("Minimum forward speed required for an impact to register - filters out incidental taps at low speed.")]
        [SerializeField] private float _minImpactSpeed = 1f;

        [Tooltip("How strongly the hit surface's normal must oppose the player's forward axis to count as frontal. 1 = must point directly back at the player, 0 = any forward component counts.")]
        [SerializeField, Range(0f, 1f)] private float _frontalNormalThreshold = 0.5f;

        [Tooltip("Lateral distance (meters) from the player's center line within which a frontal hit counts as dead-center rather than off-center.")]
        [SerializeField] private float _deadCenterThreshold = 0.35f;

        [Tooltip("Minimum time between processed collisions, so sliding along one obstacle doesn't reapply the response every frame.")]
        [SerializeField] private float _responseCooldown = 0.5f;

        [Header("Side-On Response")]
        [SerializeField] private float _sideSpeedPenalty = 3f;
        [SerializeField] private float _sideRecoveryTime = 0.6f;
        [Tooltip("Lateral velocity (units/sec) injected away from the side that was grazed.")]
        [SerializeField] private float _sideDeflectionSpeed = 3f;
        [SerializeField] private string _sideAnimatorTrigger = "SideHit";

        [Header("Off-Center Frontal Response")]
        [SerializeField] private float _offCenterSpeedPenalty = 6f;
        [SerializeField] private float _offCenterRecoveryTime = 1f;
        [Tooltip("Lateral velocity (units/sec) injected away from the side that was hit.")]
        [SerializeField] private float _offCenterDeflectionSpeed = 6f;
        [SerializeField] private string _offCenterAnimatorTrigger = "Stagger";

        [Header("Dead-Center Frontal Response")]
        [SerializeField] private float _deadCenterSpeedPenalty = 10f;
        [SerializeField] private float _deadCenterRecoveryTime = 1.5f;
        [Tooltip("Lateral velocity (units/sec) injected away from center. Kept small by default since a dead-center hit is nearly centered by definition.")]
        [SerializeField] private float _deadCenterDeflectionSpeed = 2f;
        [SerializeField] private float _deadCenterDamage = 20f;
        [SerializeField] private string _deadCenterAnimatorTrigger = "BigHit";

        public LayerMask ObstacleLayers => _obstacleLayers;
        public float MinImpactSpeed => _minImpactSpeed;
        public float FrontalNormalThreshold => _frontalNormalThreshold;
        public float DeadCenterThreshold => _deadCenterThreshold;
        public float ResponseCooldown => _responseCooldown;

        public float SideSpeedPenalty => _sideSpeedPenalty;
        public float SideRecoveryTime => _sideRecoveryTime;
        public float SideDeflectionSpeed => _sideDeflectionSpeed;
        public string SideAnimatorTrigger => _sideAnimatorTrigger;

        public float OffCenterSpeedPenalty => _offCenterSpeedPenalty;
        public float OffCenterRecoveryTime => _offCenterRecoveryTime;
        public float OffCenterDeflectionSpeed => _offCenterDeflectionSpeed;
        public string OffCenterAnimatorTrigger => _offCenterAnimatorTrigger;

        public float DeadCenterSpeedPenalty => _deadCenterSpeedPenalty;
        public float DeadCenterRecoveryTime => _deadCenterRecoveryTime;
        public float DeadCenterDeflectionSpeed => _deadCenterDeflectionSpeed;
        public float DeadCenterDamage => _deadCenterDamage;
        public string DeadCenterAnimatorTrigger => _deadCenterAnimatorTrigger;
    }
}