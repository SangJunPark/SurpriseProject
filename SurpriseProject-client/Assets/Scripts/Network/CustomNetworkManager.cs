using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
namespace MirrorBasics
{
    public class CustomNetworkManager : NetworkManager
    {

        public static event Action<NetworkConnection> OnServerReadied;
        [SerializeField] GameObject playerSpawnSystem;
        public override void OnStartServer()
        {
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
            // base.OnStartServer();
        }
        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            OnServerReadied?.Invoke(conn);
        }
    }
}