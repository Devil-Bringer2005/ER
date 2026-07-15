namespace EndlessRunner.Player.Powerups.Effects
{
    using EndlessRunner.Player.Combat;
    using UnityEngine;

    /// <summary>
    /// Swaps in a different ranged AttackDefinitionSO for the powerup's duration. Deliberately
    /// does NOT store "what was equipped before" on this asset - this is a shared ScriptableObject,
    /// so a field here would be one value shared by every activation. PlayerAttackController owns
    /// a push/pop stack instead, which is what actually makes overlapping swaps behave correctly.
    /// </summary>
    [CreateAssetMenu(menuName = "Powerups/Effects/Temporary Ranged Weapon", fileName = "New Temporary Ranged Weapon Effect")]
    public class TemporaryRangedWeaponEffect : PowerupEffect
    {
        [SerializeField] private AttackDefinitionSO _attack;

        public override void Activate(GameObject target) =>
            target.GetComponent<PlayerAttackController>()?.PushRangedAttack(_attack);

        public override void Deactivate(GameObject target) =>
            target.GetComponent<PlayerAttackController>()?.PopRangedAttack();
    }
}