namespace EndlessRunner.Player.Collision
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Movement;
    using EndlessRunner.Player.Combat;
    using MoreMountains.Feedbacks;

    /// <summary>
    /// Classifies CharacterController impacts into side-on / off-center-frontal / dead-center-frontal
    /// and dispatches the matching speed penalty, lateral deflection, and (for dead-center hits)
    /// damage. Every tier deflects, just by a different amount and in a different direction: a
    /// side-on hit pushes away from the surface it grazed, a frontal hit pushes away from
    /// whichever side of center it landed on. Movement and health react through their own public
    /// methods/interface, so this handler never needs to know how a speed penalty or a hit point
    /// is actually applied.
    ///
    /// Classification:
    ///  - Ground/floor contacts (normal pointing mostly straight up) are ignored outright.
    ///  - Remaining hits are FRONTAL if the surface normal opposes the player's forward axis by
    ///    at least FrontalNormalThreshold and more than it opposes the right axis; otherwise SIDE-ON.
    ///  - Frontal hits are DEAD-CENTER if the contact point falls within DeadCenterThreshold of the
    ///    player's center line (measured along the right axis), otherwise OFF-CENTER.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Player/Collision/Player Collision Handler")]
    public class PlayerCollisionHandler : MonoBehaviour
    {
        [SerializeField] private CollisionResponseConfig _config;
        [SerializeField] private PlayerLateralMotor _lateralMotor;
        [SerializeField] private PlayerForwardMotor _forwardMotor;
        [SerializeField] private PlayerHealth _health;

        [Header("Feedback")]
        [SerializeField] private MMF_Player _hitfeedback;

        private float _nextResponseTime;

        /// <summary>Fired once per processed impact, after the response has already been applied.</summary>
        public event Action<CollisionImpactInfo> ImpactOccurred;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_config == null || Time.time < _nextResponseTime) return;
            if (((1 << hit.collider.gameObject.layer) & _config.ObstacleLayers) == 0) return;
            if (_forwardMotor == null || _forwardMotor.CurrentSpeed < _config.MinImpactSpeed) return;

            // Ground/floor - never a collision "impact", regardless of layer setup.
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f) return;

            _hitfeedback?.PlayFeedbacks();

            float forwardAlignment = Vector3.Dot(hit.normal, -transform.forward);
            float sideAlignment = Mathf.Abs(Vector3.Dot(hit.normal, transform.right));
            bool isFrontal = forwardAlignment >= _config.FrontalNormalThreshold && forwardAlignment >= sideAlignment;

            CollisionImpactInfo info = isFrontal ? HandleFrontal(hit) : HandleSideOn(hit);

            _nextResponseTime = Time.time + _config.ResponseCooldown;
            ImpactOccurred?.Invoke(info);
        }

        private CollisionImpactInfo HandleFrontal(ControllerColliderHit hit)
        {
            float lateralOffset = Vector3.Dot(hit.point - transform.position, transform.right);
            return Mathf.Abs(lateralOffset) <= _config.DeadCenterThreshold
                ? HandleDeadCenter(hit, lateralOffset)
                : HandleOffCenter(hit, lateralOffset);
        }

        private CollisionImpactInfo HandleSideOn(ControllerColliderHit hit)
        {
            _forwardMotor.ApplySpeedPenalty(_config.SideSpeedPenalty, _config.SideRecoveryTime);

            // The hit normal already points from the obstacle's surface back toward us, so its
            // component along our right axis directly tells us which way "away" is - no offset
            // math needed the way frontal hits require.
            float deflectionSign = Mathf.Sign(Vector3.Dot(hit.normal, transform.right));
            _lateralMotor?.ApplyLateralImpulse(deflectionSign * _config.SideDeflectionSpeed);

            return new CollisionImpactInfo(CollisionImpactType.SideOn, hit.point, _config.SideAnimatorTrigger);
        }

        private CollisionImpactInfo HandleOffCenter(ControllerColliderHit hit, float lateralOffset)
        {
            _forwardMotor.ApplySpeedPenalty(_config.OffCenterSpeedPenalty, _config.OffCenterRecoveryTime);

            // Contact to our right (positive offset) pushes us left, and vice versa.
            float deflectionSign = -Mathf.Sign(lateralOffset);
            _lateralMotor?.ApplyLateralImpulse(deflectionSign * _config.OffCenterDeflectionSpeed);

            return new CollisionImpactInfo(CollisionImpactType.OffCenterFrontal, hit.point, _config.OffCenterAnimatorTrigger);
        }

        private CollisionImpactInfo HandleDeadCenter(ControllerColliderHit hit, float lateralOffset)
        {
            _forwardMotor.ApplySpeedPenalty(_config.DeadCenterSpeedPenalty, _config.DeadCenterRecoveryTime);
            _health?.TakeDamage(_config.DeadCenterDamage, hit.gameObject);

            // Same convention as off-center, just a much smaller push since the offset is tiny
            // by definition here - mostly a "jolt" rather than a real redirect.
            float deflectionSign = -Mathf.Sign(lateralOffset);
            _lateralMotor?.ApplyLateralImpulse(deflectionSign * _config.DeadCenterDeflectionSpeed);

            return new CollisionImpactInfo(CollisionImpactType.DeadCenterFrontal, hit.point, _config.DeadCenterAnimatorTrigger);
        }
    }
}