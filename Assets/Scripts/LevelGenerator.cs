using UnityEngine;
using Unity.AI;
using Unity.AI.Navigation;

public class LevelGenerator : MonoBehaviour
{
    public NavMeshSurface surface;
    void Awake()
    {
        //GetComponent<NavMeshSurface>().BuildNavMesh();
        surface.BuildNavMesh();
    }

}
