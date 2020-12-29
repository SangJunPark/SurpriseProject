using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
namespace MirrorBasics
{
    public class Player : NetworkBehaviour
    {

        public static Player localPlayer;
        [SyncVar] public string matchID;
        [SyncVar] public int playerIndex;
        [SerializeField] GameObject playerSpawnSystem;
        NetworkMatchChecker networkMatchChecker;
        void Start()
        {

            networkMatchChecker = GetComponent<NetworkMatchChecker>();
            if (isLocalPlayer)
            {
                localPlayer = this;
            }
            else
            {
                UILobby.instance.SpawnPlayerUIPrefab(this);
            }
        }


        /// ////////////////////////////////// HOST GAME //////////////////////////////////////////////////////////
        public void HostGame()
        {
            string matchID = MatchMaker.GetNewMatchID();
            CmdHostGame(matchID);
        }

        [Command]
        void CmdHostGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame(_matchID, gameObject, out playerIndex))
            {
                Debug.Log("<color=green>Game Host Success</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                TargetHostGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log("<color=red>Game Host fail</color>");
                TargetHostGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetHostGame(bool success, string _matchID, int _playerIndex)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            UILobby.instance.HostSuccess(success, _matchID);
        }

        /// ////////////////////////////////// JOIN GAME //////////////////////////////////////////////////////////

        public void JoinGame(string _matchID)
        {
            CmdJoinGame(_matchID);
        }

        [Command]
        void CmdJoinGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
            {
                Debug.Log("<color=green>Game Join Success</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                TargetJoinGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log("<color=red>Game Join fail</color>");
                TargetJoinGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetJoinGame(bool success, string _matchID, int _playerIndex)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            UILobby.instance.JoinSuccess(success, _matchID);
        }

        /// ////////////////////////////////// START GAME //////////////////////////////////////////////////////////
        public void StartGame()
        {
            CmdStartGame();
        }

        [Command]
        void CmdStartGame()
        {
            MatchMaker.instance.StartGame(matchID);
            Debug.Log("<color=red>Game Starting!</color>");
        }

        public void BeginGame()
        {
            TargetStartGame();
        }

        [TargetRpc]
        void TargetStartGame()
        {
            Debug.Log($"Game {matchID} Starting!");
            //load game scene?
            StartCoroutine(StartGameScene(2));
        }

        IEnumerator StartGameScene(int sceneID)
        {
            Debug.Log($"Start load scene {sceneID}");

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneID, LoadSceneMode.Additive);


            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            UILobby.instance.HideUILobby();

            SceneManager.SetActiveScene(SceneManager.GetSceneByName("ingame_prototype"));
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }
}