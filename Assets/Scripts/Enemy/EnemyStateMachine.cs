using UnityEngine;

public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }

    private readonly EnemyController _enemy;

    public EnemyStateMachine(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public void ChangeState(IEnemyState newState)
    {
        CurrentState?.Exit(_enemy);
        CurrentState = newState;
        CurrentState?.Enter(_enemy);
    }

    public void Tick()
    {
        CurrentState?.Tick(_enemy);
    }
}