namespace EndlessRunner.Collectibles
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Collectibles/Score Collectible", fileName = "New Score Collectible")]
    public class ScoreCollectibleData : CollectibleData
    {
        [SerializeField] private int _scoreValue = 10;

        public int ScoreValue => _scoreValue;

        public override void Collect(GameObject collector) { }
    }
}