using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;

public class SocketIOScript : MonoBehaviour {
	public string serverURL = "http://sgpain.net:3000";

	public InputField uiInput = null;
	public Button uiSend = null;
	public Text uiChatLog = null;

	protected Socket socket = null;

	void Destroy() {
		DoClose ();
	}

	// Use this for initialization
	void Start () {
		DoOpen ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void DoOpen() {
		if (socket == null) {
			socket = IO.Socket (serverURL);
			socket.On (Socket.EVENT_CONNECT, () => {
                //console.log('socket connected to '  + serverURL);
			});
			
		}
	}

	void DoClose() {
		if (socket != null) {
			socket.Disconnect ();
			socket = null;
		}
	}

}