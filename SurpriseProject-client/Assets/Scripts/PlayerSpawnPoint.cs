using UnityEngine;

namespace MirrorBasics
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        void Awake()
        {
            PlayerSpawner.AddSpawnPoint(transform);
        }
        void OnDestroy()
        {
            PlayerSpawner.RemoveSpawnPoint(transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
        }
    }
}