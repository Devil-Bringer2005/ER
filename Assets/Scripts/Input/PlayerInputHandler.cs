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
        public event Action JumpPressed;
        public event Action<bool> SpeedBoostPressed;

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

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                JumpPressed?.Invoke();
        }

        public void OnSpeedBoost(InputAction.CallbackContext context)
        {
            if (context.performed)
                SpeedBoostPressed?.Invoke(true);
            else if (context.canceled)
                SpeedBoostPressed?.Invoke(false);
        }

        public void OnDrift(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
    }
}