namespace EndlessRunner.Collectibles
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Fires for every pickup regardless of category. Systems that only care "something was
    /// collected" (UI popups, SFX, save data) subscribe here instead of referencing Collectible
    /// or any specific CollectibleData subclass.
    /// </summary>
    public static class CollectibleEvents
    {
        public static event Action<CollectibleData, GameObject> Collected;

        /// <summary>Raised whenever something grants currency directly.
        public static event Action<int> CurrencyGranted;

        public static void RaiseCollected(CollectibleData data, GameObject collector) =>
            Collected?.Invoke(data, collector);

        public static void RaiseCurrencyGranted(int amount) => CurrencyGranted?.Invoke(amount);
    }
}
