
public abstract class EnemyBaseState
{
    protected EnemyAI _ec;
    protected EnemyStateFactory _factory;
    public EnemyBaseState(EnemyAI enemyAI, EnemyStateFactory enemyStateFactory)
    {
        _ec = enemyAI;
        _factory = enemyStateFactory;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    void UpdateStates(){}
    protected void SwitchState(EnemyBaseState newState)
    {
        ExitState();
        newState.EnterState();
        _ec.currentState = newState;
    }
    protected void SetSuperState(){}
    protected void SetSubState(){}
}
