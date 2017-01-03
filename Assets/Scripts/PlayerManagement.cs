using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerManagement : MonoBehaviour {

	[Header("Player Attritubes")]
	public Color color;
	public int whoIam;
	public string username = "Anonymous";
	[Header("Object Assignment")]
	public GameObject player;
	public GameObject playerHead;
	public GameObject playerBody;
	public GameObject eyeCamera; 	// GVR
	public GameObject ViveRig;	// Vive
	public GameObject ViveCam;	// Vive
	//public BodyManagement bodyMgmt;
	[HideInInspector]
	public SocketManagement socketManagement;
	[HideInInspector]
	public GameObject nameTag;

	private BodyManagement bodyMgmt;

	void Start()
	{
		
	}

	public void InitPlayer(int _index, string _name)
	{
		color = new Color ();

		whoIam = _index;
		username = _name;
		nameTag.name = username + " name tag";
		nameTag.GetComponent<Text> ().text = username;
	}
		
	public void OnStartLocalPlayer()
	{
		playerHead.SetActive (false);	// no need for hear cuz can't see self
		bodyMgmt = player.GetComponent<BodyManagement> ();
		bodyMgmt.enabled = true;		// so to send transformation to Server
		bodyMgmt.socketManagement = socketManagement;
		bodyMgmt.body = playerBody;
		bodyMgmt.eyeCam = eyeCamera;
		bodyMgmt.viveCam = ViveCam;
		bodyMgmt.nameTag = nameTag;

		if (socketManagement.isViveVR)
		{
			ViveRig.SetActive(true);
		} 
		else
		{
			Vector3 startPosition = new Vector3 (Random.value*10f, 0f, Random.value*10f);
			player.transform.position = startPosition;
			//eyeCamera.tag = "MainCamera";
			eyeCamera.SetActive (true);
			GvrViewer.Create ();
		}
	}

	public void UpdateTrans(
		string type, float posX, float posY, float posZ,
		float rotX, float rotY, float rotZ, float rotW
	) {
		Vector3 position = new Vector3 (posX, posY, posZ);
		Quaternion rotation = new Quaternion (rotX, rotY, rotZ, rotW);

		if (type == "three")
		{
			position.z *= -1;
			player.transform.position = position;

			rotation = Quaternion.Euler (Vector3.up * -180f);
			rotation *= new Quaternion (rotX, -rotY, -rotZ, rotW);

			// Head
			// if(playerHead.activeSelf) // doesn't need to check cuz socket.io's broadcast doesn't send to self
				playerHead.transform.rotation = rotation;

			// Body
			rotation.x = 0;
			rotation.z = 0;
			// need to normalize quaternion?
			playerBody.transform.rotation = rotation;
		} 
		else
		{
			if (type == "vive")
				position.y -= 2f;
			player.transform.position = position;

			//rotation = new Quaternion (rotX, rotY, rotZ, rotW);

			// Head
			// if(playerHead.activeSelf)
				playerHead.transform.rotation = rotation;

			// Body
			rotation.x = 0;
			rotation.z = 0;
			playerBody.transform.rotation = rotation;
		}

		if (nameTag)
		{
			position.y += 1.8f;
			nameTag.transform.position = position;
			nameTag.transform.localRotation = rotation;
		}
	}
}
