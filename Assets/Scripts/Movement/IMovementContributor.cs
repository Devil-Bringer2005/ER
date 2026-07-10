namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Anything that wants a say in the player's per-frame displacement implements this.
    /// PlayerMotorDriver sums every contributor on the object and applies a single
    /// CharacterController.Move call, so new movement behaviours (dash, knockback, wind)
    /// can be added later as sibling components without touching existing motors.
    /// </summary>
    public interface IMovementContributor
    {
        Vector3 GetFrameMovement(float deltaTime);
    }
}
