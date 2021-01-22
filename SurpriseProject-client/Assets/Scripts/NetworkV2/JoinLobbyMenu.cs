using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private SPNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button closeButton = null;

    private void OnEnable()
    {
        SPNetworkManager.OnClientConnected += HandleClientConnected;
        SPNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        SPNetworkManager.OnClientConnected -= HandleClientConnected;
        SPNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;
        if (String.IsNullOrEmpty(ipAddress)) ipAddress = "localhost";
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
        closeButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        closeButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
        closeButton.interactable = true;
    }
}