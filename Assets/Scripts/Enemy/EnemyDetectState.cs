using UnityEngine;

public class EnemyDetectState : IEnemyState
{
    private float _timer;

    public void Enter(EnemyController enemy)
    {
        _timer = 0f;
        enemy.detectFeedback?.PlayFeedbacks();
    }

    public void Tick(EnemyController enemy)
    {
        if (!enemy.Detector.TryDetectPlayer(out var player))
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }

        enemy.PlayerTarget = player;
        _timer += Time.deltaTime;

        if (_timer >= enemy.DetectDelay)
            enemy.ChangeState(new EnemyAttackState());
    }

    public void Exit(EnemyController enemy) { }
}