using UnityEngine;

public class SPPlayerSpawnPoint : MonoBehaviour
{
    private void Awake() => SPPlayerSpawnSystem.AddSpawnPoint(transform);
    private void OnDestroy() => SPPlayerSpawnSystem.RemoveSpawnPoint(transform);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
    }
}
