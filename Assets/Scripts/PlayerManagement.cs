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
	private Vector3 calPosition;
	private Quaternion calRotation;

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
	)
	{
		calPosition.Set (posX, posY, posZ);
		calRotation.Set (rotX, rotY, rotZ, rotW);

		if (type == "three")
		{
			calPosition.z *= -1;

			calRotation = Quaternion.Euler (Vector3.up * -180f);
			calRotation *= new Quaternion (rotX, -rotY, -rotZ, rotW);

			/*
			// Head
			// if(playerHead.activeSelf) // doesn't need to check cuz socket.io's broadcast doesn't send to self
			playerHead.transform.rotation = calRotation;

			// Body
			calRotation.x = 0;
			calRotation.z = 0;
			// need to normalize quaternion?
			playerBody.transform.rotation = calRotation;
			*/
		} 
		else
		{
			if (type == "vive")
				calPosition.y -= 2f;
			/*
			// Head
			// if(playerHead.activeSelf)
			playerHead.transform.rotation = calRotation;

			// Body
			calRotation.x = 0;
			calRotation.z = 0;
			playerBody.transform.rotation = calRotation;
			*/
		}

		//v.1 no interpolation
		/*
		player.transform.position = calPosition;
		playerHead.transform.rotation = calRotation;
		calRotation.x = 0;
		calRotation.z = 0;
		playerBody.transform.rotation = calRotation;
		*/
		player.transform.position = Vector3.Lerp (player.transform.position, calPosition, 0.1f);
		playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, calRotation, 0.1f);
		calRotation.x = 0;
		calRotation.z = 0;
		playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, calRotation, 0.1f);

		if (nameTag)
		{
			calPosition.y += 1.8f;
			//v.1 no interpolation
			/*
			nameTag.transform.position = calPosition;
			nameTag.transform.localRotation = calRotation;
			*/
			nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, calPosition, 0.1f);
			nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, calRotation, 0.1f);
		}
	}
}
