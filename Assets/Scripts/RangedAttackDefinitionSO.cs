namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Player/Combat/Ranged Attack", fileName = "NewRangedAttack")]
    public class RangedAttackDefinitionSO : AttackDefinitionSO
    {
        [Header("Projectile")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 25f;
        [SerializeField] private float _projectileLifetime = 3f;

        public override void Execute(in PlayerAttackContext context)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning($"{name}: no projectile prefab assigned, ranged attack did nothing.");
                return;
            }

            Transform muzzle = context.MuzzlePoint != null ? context.MuzzlePoint : context.Owner.transform;
            Projectile instance = Object.Instantiate(_projectilePrefab, muzzle.position, muzzle.rotation);
            instance.Launch(muzzle.forward, _projectileSpeed, Damage, _projectileLifetime, context.Owner);
        }
    }
}
