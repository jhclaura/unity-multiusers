using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.SocketIO;
using UnityEngine.UI;
using UnityEngine.VR;

public class SocketManagement : MonoBehaviour {

	[Header("Socket Settings")]
	public string networkAddress;
	public string portNumber;
	public string myName = string.Empty;
	public bool isConnected = false;
	public int whoIamInLife;
	[HideInInspector]
	public bool isViveVR;
	public SocketManager Manager;

	[Header("UI Settings")]
	public GameObject menuCanvas;
	public Button sendButton;
	public InputField msgInput;

	[Header("Object Assignment")]
	public GameObject worldCanvas;
	public GameObject simplePlayerPrefab;
	public GameObject nameTagPrefab;

	private enum ChatStates
	{
		Login,
		Chat
	}
	private ChatStates State;
	private string message = string.Empty;
	private string chatLog = string.Empty;
	private GameObject mainCamera;

	private Dictionary<int, GameObject> playerDict = new Dictionary<int, GameObject>();
	private bool updatePreviousPlayer = false;


	void Awake() {
		if (VRSettings.enabled)
		{
			isViveVR = true;
		}
		else
		{
			isViveVR = false;
		}
	}

	void Start () {
		if (Camera.main.gameObject != null)
		{
			mainCamera = Camera.main.gameObject;
		}
		else {
			mainCamera = null;
		}

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
				menuCanvas.SetActive (false);
				break;

			case ChatStates.Chat:
				SendNewMessage ();
				break;
		}
	}

	void SetUserName()
	{
		string u_n = msgInput.textComponent.text;
		if (string.IsNullOrEmpty (u_n))
			u_n = "Anonymous";
		myName = u_n;
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
		isConnected = true;

		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		whoIamInLife = GetInt (data["index"]);
		int n_p = GetInt (data["numPlayers"]);
		Debug.Log("Connected to Socket Server! I'm #" + whoIamInLife + " in total " + n_p + " players");

		// disable main camera, if any
		if(mainCamera)
			mainCamera.SetActive (false);

		// create player SELF
		GameObject selfPlayer = Instantiate(simplePlayerPrefab);
		selfPlayer.name = "ME #" + whoIamInLife + " " + myName;
		PlayerManagement selfPlayerMgmt = selfPlayer.GetComponent<PlayerManagement> ();
		selfPlayerMgmt.socketManagement = this;
		selfPlayerMgmt.nameTag = Instantiate(nameTagPrefab);
		selfPlayerMgmt.nameTag.transform.SetParent (worldCanvas.transform);
		selfPlayerMgmt.InitPlayer(whoIamInLife, myName);
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
		var p_index = GetInt(data["index"]);
		Dictionary<string, object> transform = data["transform"] as Dictionary<string, object>;

		// create incoming player
		Vector3 newPlayerStartPos = new Vector3(
			GetFloat(transform["startX"]), GetFloat(transform["startY"]), GetFloat(transform["startZ"])
		);
		GameObject newGuy = Instantiate (simplePlayerPrefab);
		newGuy.name = "Player #"+ p_index + " " + username;
		newGuy.transform.position = newPlayerStartPos;
		PlayerManagement newGuyMgmt = newGuy.GetComponent<PlayerManagement> ();

		newGuyMgmt.nameTag = Instantiate(nameTagPrefab);
		newGuyMgmt.nameTag.transform.SetParent (worldCanvas.transform);
		newGuyMgmt.nameTag.transform.position = newPlayerStartPos;
		newGuyMgmt.InitPlayer(p_index, username);

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
		//string username = data ["username"] as string;
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

			for (int i=0; i<allplayers.Count; i++)
			{
				Dictionary<string, object> a_p = allplayers[i] as Dictionary<string, object>;
				int p_index = GetInt (a_p ["index"]);
				string p_name = a_p ["username"] as string;
				Vector3 oldPlayerStartPos = new Vector3(
					GetFloat(a_p["startX"]), GetFloat(a_p["startY"]), GetFloat(a_p["startZ"])
				);
				GameObject oldGuy = Instantiate (simplePlayerPrefab);
				oldGuy.transform.position = oldPlayerStartPos;
				oldGuy.name = "Player #"+ p_index + " " + p_name;
				PlayerManagement oldGuyMgmt = oldGuy.GetComponent<PlayerManagement> ();
				oldGuyMgmt.nameTag = Instantiate(nameTagPrefab);
				oldGuyMgmt.nameTag.transform.SetParent (worldCanvas.transform);
				oldGuyMgmt.nameTag.transform.position = oldPlayerStartPos;
				oldGuyMgmt.InitPlayer( p_index, p_name);

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
