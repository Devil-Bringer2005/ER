namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Player/Movement/Speed Boost Config", fileName = "New Speed Boost Config")]
    public class SpeedBoostConfig : ScriptableObject
    {
        [field: SerializeField] public float BoostSpeed { get; private set; } = 6f;
        [field: SerializeField] public float DrainPerSecond { get; private set; } = 20f;
    }
}