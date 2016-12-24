﻿using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.SocketIO;
using UnityEngine.UI;

public class SocketManagement : MonoBehaviour {

	private enum ChatStates
	{
		Login,
		Chat
	}

	public SocketManager Manager;
	ChatStates State;
	public string networkAddress;
	public string portNumber;
	public string myName = string.Empty;
	string message = string.Empty;
	string chatLog = string.Empty;

	public Button sendButton;
	public InputField msgInput;

	public bool connected = false;

	public int whoIamInLife;

	public GameObject simplePlayerPrefab;
	GameObject selfPlayer;
	PlayerManagement selfPlayerMgmt;
	GameObject mainCamera;

	Dictionary<int, GameObject> playerDict = new Dictionary<int, GameObject>();
	bool updatePreviousPlayer = false;


	// Use this for initialization
	void Start () {
		mainCamera = Camera.main.gameObject;

		State = ChatStates.Login;

		// Socket
		SocketOptions options = new SocketOptions ();
		options.AutoConnect = false;

		Manager = new SocketManager (new Uri("http://"+networkAddress+":"+portNumber+"/socket.io/"), options);

		// Setup Events
		Manager.Socket.On ("login", OnLogin);
		Manager.Socket.On ("new message", OnNewMessage);
		Manager.Socket.On ("player joined", OnPlayerJoined);
		Manager.Socket.On ("player left", OnPlayerLeft);
		Manager.Socket.On ("update position", OnUpdatePosition);
		Manager.Socket.On ("update history", OnUpdateHistory);

		// On Error
		Manager.Socket.On (SocketIOEventTypes.Error, (socket, packet, args) 
			=> Debug.Log(string.Format("Error: {0}", args[0].ToString()))
		);
			
		Manager.Open ();
	}

	void OnDestroy()
	{
		Manager.Close ();
	}

	public void PushSendButton()
	{
		switch (State)
		{
			case ChatStates.Login:
				SetUserName ();
				break;

			case ChatStates.Chat:
				SendNewMessage ();
				break;
		}
	}

	void SetUserName()
	{
		string u_n = msgInput.textComponent.text;
		myName = u_n;
		if (string.IsNullOrEmpty (u_n))
			u_n = "Anonymous";
		
		State = ChatStates.Chat;
		Manager.Socket.Emit ("new player", u_n);
	}

	void SendNewMessage()
	{
		string msg = msgInput.textComponent.text;
		if (string.IsNullOrEmpty (msg))
			return;

		Manager.Socket.Emit ("new message", msg);
	}

	#region SocketIO Evenets
	void OnLogin(Socket socket, Packet packet, params object[] args)
	{
		connected = true;
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		whoIamInLife = GetInt (data["index"]);
		int n_p = GetInt (data["numPlayers"]);
		Debug.Log("Connected to IO server! player total num: " + n_p);

		// create player SELF
		selfPlayer = Instantiate(simplePlayerPrefab);
		selfPlayer.name = "ME #"+whoIamInLife + " " + myName;
		selfPlayerMgmt = selfPlayer.GetComponent<PlayerManagement> ();
		mainCamera.SetActive (false);
		selfPlayerMgmt.color = new Color ();
		selfPlayerMgmt.whoIam = whoIamInLife;
		selfPlayerMgmt.username = myName;
		selfPlayerMgmt.socketManagement = this;
		selfPlayerMgmt.OnStartLocalPlayer ();

		// add to dict
		playerDict.Add(whoIamInLife, selfPlayer);

		// know transformation
		// -> created in GameObject selfPlayer

		// send new player info back to server
		Dictionary<string, object> msg = new Dictionary<string, object>();
		msg.Add ("index", whoIamInLife);
		msg.Add ("startX", selfPlayer.transform.position.x);
		msg.Add ("startY", selfPlayer.transform.position.y);
		msg.Add ("startZ", selfPlayer.transform.position.z);
		//msg.Add ("color", selfPlayerMgmt.color);
		msg.Add ("username", myName);
		Manager.Socket.Emit("create new player", msg);

		/*
		Dictionary<string, object>[] allplayers = data ["allPlayers"] as Dictionary<string, object>[];
		Debug.Log(allplayers.Length);
		for (int i = 0; i < allplayers.Length; i++) {
//			Dictionary<string, string> a_p = allplayers[i] as Dictionary<string, string>;
			Debug.Log("Add history player: " + allplayers[i]["username"]);
		}
		*/
	}

	void OnNewMessage(Socket socket, Packet packet, params object[] args)
	{
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		var username = data ["username"] as string;
		var msg = data["message"] as string;
		Debug.Log (username + " sends msg: " + msg);
	}

	void OnPlayerJoined(Socket socket, Packet packet, params object[] args)
	{
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		int numP = GetInt(data["numPlayers"]);
		var username = data ["username"] as string;
		Dictionary<string, object> transform = data["transform"] as Dictionary<string, object>;

		// create incoming player
		Vector3 newPlayerStartPos = new Vector3(
			GetFloat(transform["startX"]), GetFloat(transform["startY"]), GetFloat(transform["startZ"])
		);
		GameObject newGuy = Instantiate (simplePlayerPrefab);
		newGuy.transform.position = newPlayerStartPos;
		PlayerManagement newGuyMgmt = newGuy.GetComponent<PlayerManagement> ();
		//newGuyMgmt.color = (Color) data ["color"];
		newGuyMgmt.whoIam = GetInt(data["index"]);
		newGuyMgmt.username = data ["username"] as string;
		newGuy.name = "Player #"+ newGuyMgmt.whoIam + " " + newGuyMgmt.username;

		playerDict.Add (newGuyMgmt.whoIam, newGuy);

		Debug.Log("New player: " + newGuyMgmt.username + " joined! Now total player num: " + numP);
	}
		
	void OnPlayerLeft(Socket socket, Packet packet, params object[] args)
	{
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		var username = data ["username"] as string;
		int numP = GetInt(data["numPlayers"]);
		int leftIndex = GetInt(data["index"]);

		// remove player
		if(playerDict.ContainsKey(leftIndex)){
			Destroy (playerDict[leftIndex]);
			playerDict.Remove (leftIndex);
		}
		Debug.Log("Player: " + username + " left! Now total player num: " + numP);
	}

	void OnUpdatePosition(Socket socket, Packet packet, params object[] args)
	{
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		string username = data ["username"] as string;
		string transType = data ["type"] as string;
		int index = GetInt(data ["index"]);

		if( playerDict.ContainsKey(index) ){
			playerDict [index].GetComponent<PlayerManagement> ().UpdateTrans (
				transType, GetFloat(data["posX"]), GetFloat(data["posY"]), GetFloat(data["posZ"]),
				GetFloat(data["quaX"]), GetFloat(data["quaY"]), GetFloat(data["quaZ"]), GetFloat(data["quaW"])
			);
		}

		/*
		Debug.Log("Player: " + username + " position x: " + data ["posX"]
			+ ", y: " + data ["posY"]
			+ ", z: " + data ["posZ"]);
		*/
	}

	void OnUpdateHistory(Socket socket, Packet packet, params object[] args)
	{
		if (!updatePreviousPlayer) {
			Dictionary<string, object> data = args [0] as Dictionary<string, object>;
			List<object> allplayers = data ["allPlayers"] as List<object>;
			//Debug.Log(allplayers.Count);

			for (int i=0; i<allplayers.Count; i++) {
				Dictionary<string, object> a_p = allplayers[i] as Dictionary<string, object>;
				Vector3 oldPlayerStartPos = new Vector3(
					GetFloat(a_p["startX"]), GetFloat(a_p["startY"]), GetFloat(a_p["startZ"])
				);
				GameObject oldGuy = Instantiate (simplePlayerPrefab);
				oldGuy.transform.position = oldPlayerStartPos;
				PlayerManagement oldGuyMgmt = oldGuy.GetComponent<PlayerManagement> ();
				//newGuyMgmt.color = (Color) data ["color"];
				oldGuyMgmt.whoIam = GetInt(a_p["index"]);
				oldGuyMgmt.username = a_p ["username"] as string;
				oldGuy.name = "Player #"+ oldGuyMgmt.whoIam + " " + oldGuyMgmt.username;
				playerDict.Add (oldGuyMgmt.whoIam, oldGuy);

				Debug.Log("Add history player: " + a_p["username"]);
			}
			updatePreviousPlayer = true;
		}
	}
	#endregion

	int GetInt(object num)
	{
		int numm = Convert.ToInt32 (num);
		return numm;
	}

	float GetFloat(object num)
	{
		float numm = Convert.ToSingle (num);
		return numm;
	}
}
