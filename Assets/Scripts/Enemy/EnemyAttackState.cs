using UnityEngine;
using EndlessRunner.Player.Combat;

public class EnemyAttackState : IEnemyState
{
    private float _cooldownTimer;

    public void Enter(EnemyController enemy)
    {
        _cooldownTimer = 0f;
        Attack(enemy);
    }

    public void Tick(EnemyController enemy)
    {
        if (!enemy.Detector.TryDetectPlayer(out var player))
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }

        enemy.PlayerTarget = player;

        float distance = Vector3.Distance(enemy.transform.position, player.position);
        if (distance > enemy.AttackRange)
        {
            enemy.ChangeState(new EnemyDetectState());
            return;
        }

        _cooldownTimer += Time.deltaTime;
        if (_cooldownTimer >= enemy.AttackCooldown)
        {
            Attack(enemy);
            _cooldownTimer = 0f;
        }
    }

    public void Exit(EnemyController enemy) { }

    private void Attack(EnemyController enemy)
    {
        if (enemy.ProjectilePrefab == null || enemy.FirePoint == null || enemy.PlayerTarget == null)
            return;

        Vector3 targetVelocity = enemy.PlayerVelocityTracker ? enemy.PlayerVelocityTracker.Velocity : Vector3.zero;

        Vector3 aimPoint = ProjectileAimSolver.PredictInterceptPosition(
            enemy.FirePoint.position,
            enemy.PlayerTarget.position,
            targetVelocity,
            enemy.ProjectileSpeed);

        Vector3 direction = (aimPoint - enemy.FirePoint.position).normalized;

        GameObject projectileGO = Object.Instantiate(
            enemy.ProjectilePrefab,
            enemy.FirePoint.position,
            Quaternion.LookRotation(direction));

        if (projectileGO.TryGetComponent<ProjectileDeflector>(out var projectile))
        {
            projectile.Launch(
                direction,
                enemy.ProjectileSpeed,
                enemy.ProjectileDamage,
                enemy.ProjectileLifetime,
                enemy.gameObject,
                enemy.PlayerTarget);
        }
    }
}