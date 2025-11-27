using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    EnemyAI enemyAI;
    public float damage;
    private void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        damage = enemyAI.attackDamage;
    }
}
