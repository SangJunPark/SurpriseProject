using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MirrorBasics
{

    public class UILobby : MonoBehaviour
    {
        public static UILobby instance;
        [Header("Host Join")]
        [SerializeField] InputField joinMatchInput;
        [SerializeField] Button joinButton;
        [SerializeField] Button hostButton;
        [SerializeField] Canvas lobbyCanvas;

        [Header("Lobby")]
        [SerializeField] Canvas LobbyUI;
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject startGame;

        void Start()
        {
            instance = this;   
        }
        public void Host()
        {
            joinMatchInput.interactable = false;
            joinButton.interactable = false;
            hostButton.interactable = false;

            Player.localPlayer.HostGame();
        }

        public void HostSuccess(bool success, string _matchID)
        {
            if(success)
            {
                lobbyCanvas.enabled = true;
                SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = _matchID;
                startGame.SetActive(true);
            } else
            {
                joinMatchInput.interactable = true;
                joinButton.interactable = true;
                hostButton.interactable = true;
            }
        }

        public void Join()
        {
            joinMatchInput.interactable = false;
            joinButton.interactable = false;
            hostButton.interactable = false;

            Player.localPlayer.JoinGame(joinMatchInput.text.ToUpper());

        }

        public void JoinSuccess(bool success, string _matchID)
        {
            if (success)
            {
                lobbyCanvas.enabled = true;
                SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = _matchID;
                startGame.SetActive(false);

            }
            else
            {
                joinMatchInput.interactable = true;
                joinButton.interactable = true;
                hostButton.interactable = true;
            }
        }

        public void SpawnPlayerUIPrefab(Player player)
        {
            GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
            newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
            newUIPlayer.transform.SetSiblingIndex(player.playerIndex-1);
        }

        public void StartGame()
        {
            Player.localPlayer.StartGame();
        }

        public void HideUILobby()
        {
            LobbyUI.enabled = false;
        }
    }
}