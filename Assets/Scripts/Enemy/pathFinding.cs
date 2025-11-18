using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathFinding
{
    private EnemyAI _ec;

    public PathFinding(EnemyAI enemyAI)
    {
        _ec = enemyAI;
    }
    public void HandleMovement()
    {
        if (_ec.enemyAgent.isOnNavMesh && !_ec.StartRagdoll)
        {
            _ec.enemyAgent.destination = _ec.target.position;
        }

    }

}
