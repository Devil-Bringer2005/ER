namespace EndlessRunner.Collectibles
{
    using UnityEngine;

    /// <summary>
    /// Base type for every collectible category. A new category is a new subclass, not a new
    /// branch in Collectible.cs - drop the asset in, assign it to a pickup, done.
    /// </summary>
    public abstract class CollectibleData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;

        /// <summary>Applies this collectible's gameplay effect to whatever picked it up.</summary>
        public abstract void Collect(GameObject collector);
    }
}