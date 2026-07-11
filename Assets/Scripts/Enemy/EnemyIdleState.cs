public class EnemyIdleState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        // Play idle animation here if needed
    }

    public void Tick(EnemyController enemy)
    {
        if (enemy.Detector.TryDetectPlayer(out var player))
        {
            enemy.PlayerTarget = player;
            enemy.ChangeState(new EnemyDetectState());
        }
    }

    public void Exit(EnemyController enemy) { }
}