namespace EndlessRunner.Player.Controls
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Default IPlayerInputSource implementation using the legacy Input class, so this
    /// package works with zero extra setup. Replace this single file with a new Input
    /// System version later - nothing downstream needs to change since everything else
    /// only depends on the IPlayerInputSource interface.
    /// </summary>
    [AddComponentMenu("Player/Controls/Player Input Handler")]
    public class PlayerInputHandler : MonoBehaviour, IPlayerInputSource
    {
        [Header("Keybinds (swap this component for the new Input System later)")]
        [SerializeField] private KeyCode _leftKey = KeyCode.A;
        [SerializeField] private KeyCode _rightKey = KeyCode.D;
        [SerializeField] private KeyCode _meleeKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode _rangedKey = KeyCode.Mouse1;

        public float LateralInput { get; private set; }

        public event Action MeleeAttackPressed;
        public event Action RangedAttackPressed;

        private void Update()
        {
            float raw = 0f;
            if (UnityEngine.Input.GetKey(_leftKey)) raw -= 1f;
            if (UnityEngine.Input.GetKey(_rightKey)) raw += 1f;
            LateralInput = raw;

            if (UnityEngine.Input.GetKeyDown(_meleeKey)) MeleeAttackPressed?.Invoke();
            if (UnityEngine.Input.GetKeyDown(_rangedKey)) RangedAttackPressed?.Invoke();
        }
    }
}
