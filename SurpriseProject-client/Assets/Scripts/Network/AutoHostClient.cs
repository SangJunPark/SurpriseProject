using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MirrorBasics
{

    public class AutoHostClient : MonoBehaviour
    {
        [SerializeField] NetworkManager networkManager;
        //Awake starts before network init
        void Start()
        {
            if (!Application.isBatchMode)
            {
                Debug.Log($"=== Client build ===");
                networkManager.StartClient();
            }
            else
            {
                Debug.Log($"=== Server build ===");
            }
        }
        public void JoinLocal()
        {
            networkManager.networkAddress = "localhost";
            networkManager.StartClient();
        }
    }
}