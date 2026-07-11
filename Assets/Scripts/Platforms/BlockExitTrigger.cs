using System;
using UnityEngine;

namespace PlatformRunner.Platforms
{
    /// <summary>
    /// Attach to a trigger collider positioned at (or just past) one of a block's
    /// exits. When the player crosses it, PlatformGenerator recycles the block
    /// chain up to and including this block. A block with several exits can carry
    /// several of these, one per branch.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BlockExitTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";

        public event Action<PlatformBlock> PlayerExited;

        private PlatformBlock _owner;

        private void Awake()
        {
            _owner = GetComponentInParent<PlatformBlock>();
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            PlayerExited?.Invoke(_owner);
        }
    }
}
