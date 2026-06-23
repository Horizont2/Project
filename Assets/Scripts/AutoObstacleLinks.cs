using UnityEngine;
using Unity.AI.Navigation;

public class AutoObstacleLinks : MonoBehaviour
{
    public float linkWidth = 3f;

    [Tooltip("Distance from the wall start to end")]
    public float distanceFromWall = 0.5f;

    void Start()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3 extents = transform.lossyScale * 0.5f;

        foreach (Vector3 dir in directions)
        {
            GameObject linkObj = new GameObject("ClimbLink");
            linkObj.transform.position = transform.position;
            linkObj.transform.rotation = transform.rotation;

            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();
            link.width = linkWidth;

            Vector3 edgePos = new Vector3(dir.x * extents.x, 0f, dir.z * extents.z);

            link.startPoint = edgePos + (dir * distanceFromWall) + (Vector3.down * extents.y);

            link.endPoint = edgePos - (dir * distanceFromWall) + (Vector3.up * extents.y);
        }
    }
}