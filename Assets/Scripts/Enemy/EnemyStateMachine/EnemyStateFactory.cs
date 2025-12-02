public class EnemyStateFactory
{
    private EnemyAI _ec;

    public EnemyStateFactory(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
    public EnemyBaseState Chase()
    {
        return new EnemyChaseState(_ec, this);
    }
    public EnemyBaseState Attack()
    {
        return new EnemyAttackState(_ec, this);
    }
    public EnemyBaseState Ragdoll()
    {
        return new EnemyRagdollState(_ec, this);
    }
    public EnemyBaseState Crouch()
    {
        return new EnemyCrouchState(_ec, this);
    }
}
