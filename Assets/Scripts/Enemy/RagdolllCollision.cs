using UnityEngine;

public class RagdollCollisionHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private EnemyAI _ec;

    public void Init(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // check player on whatever hit the ragdoll bone
        var player = collision.transform.GetComponent<playerController>();
        if (player != null)
        {
            _ec.OnRagdollHit(collision, player);
        }
    }

}
