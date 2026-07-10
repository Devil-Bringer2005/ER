namespace EndlessRunner.Player.Controls
{
    using System;

    /// <summary>
    /// Abstraction over the raw input device so movement/combat scripts never talk to
    /// Unity's Input class directly. Swap PlayerInputHandler for a new Input System
    /// version, an AI driver, or a replay system without touching any gameplay code.
    /// </summary>
    public interface IPlayerInputSource
    {
        /// <summary>-1 (full left) to 1 (full right). 0 when idle.</summary>
        float LateralInput { get; }

        event Action MeleeAttackPressed;
        event Action RangedAttackPressed;
    }
}
