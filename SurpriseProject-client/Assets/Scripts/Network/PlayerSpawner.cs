using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

namespace MirrorBasics
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab = null;
        [SerializeField] private static List<Transform> spawnPoints = new List<Transform>();
        private int nextIndex = 0;
        
        public static void AddSpawnPoint(Transform transform) {
            spawnPoints.Add(transform);
            spawnPoints = spawnPoints.OrderBy(x=>x.GetSiblingIndex()).ToList();
        }
        public static void RemoveSpawnPoint(Transform transform) {
            spawnPoints.Remove(transform);
        }

        public override void OnStartClient()
        {
            Debug.Log("player spawner start client");
        }

        public override void OnStartServer() => CustomNetworkManager.OnServerReadied += SpawnPlayer;

        [ServerCallback]
        private void OnDestroy() => CustomNetworkManager.OnServerReadied -= SpawnPlayer;

        [Server]
        public void SpawnPlayer(NetworkConnection conn) {
            Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);
            if(spawnPoint == null) {
                Debug.LogError($"Missing spawn index {nextIndex}");
                return;
            }
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
            NetworkServer.Spawn(playerInstance, conn);

            nextIndex++;
        }

    }
}