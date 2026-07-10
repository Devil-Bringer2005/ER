namespace EndlessRunner.Player.Controls
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [AddComponentMenu("Player/Controls/Player Input Handler")]
    public class PlayerInputHandler : MonoBehaviour, PlayerControls.IPlayerActions, IPlayerInputSource
    {
        public float LateralInput { get; private set; }

        public event Action MeleeAttackPressed;
        public event Action RangedAttackPressed;

        private PlayerControls controls;

        private void Awake()
        {
            controls = new();
            controls.Player.SetCallbacks(this);
            controls.Enable();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            MeleeAttackPressed?.Invoke();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (context.performed)
                LateralInput = -1f;
            else if (context.canceled && LateralInput < 0f)
                LateralInput = 0f;
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (context.performed)
                LateralInput = 1f;
            else if (context.canceled && LateralInput > 0f)
                LateralInput = 0f;
        }
    }
}