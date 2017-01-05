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

	private bool isLocalPlayer = false;
	private BodyManagement bodyMgmt;
	private Vector3 realPosition;
	private Vector3 realTagPosition;
	private Quaternion realRotation;
	private Quaternion realBillboardRotation;

	void Update()
	{
		if (isLocalPlayer)
			return;

		player.transform.position = Vector3.Lerp (player.transform.position, realPosition, 0.1f);
		playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, realRotation, 0.1f);
		playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, realBillboardRotation, 0.1f);

		nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, realTagPosition, 0.1f);
		nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, realBillboardRotation, 0.1f);
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
		isLocalPlayer = true;
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
		realPosition.Set (posX, posY, posZ);
		realRotation.Set (rotX, rotY, rotZ, rotW);

		if (type == "three")
		{
			realPosition.z *= -1;
			realRotation = Quaternion.Euler (Vector3.up * -180f);
			realRotation *= new Quaternion (rotX, -rotY, -rotZ, rotW);
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
				realPosition.y -= 1f;
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

		//v.2 interpolation --> needs to be in Update()
		/*
		player.transform.position = Vector3.Lerp (player.transform.position, realPosition, 0.1f);
		playerHead.transform.rotation = Quaternion.Lerp (playerHead.transform.rotation, realRotation, 0.1f);
		realRotation.x = 0;
		realRotation.z = 0;
		playerBody.transform.rotation = Quaternion.Lerp (playerBody.transform.rotation, realRotation, 0.1f);
		*/
		realBillboardRotation.Set(0f, realRotation.y, 0f, realRotation.w);

		if (nameTag)
		{
			//realPosition.y += 1.8f;
			// v.1 no interpolation
			/*
			nameTag.transform.position = calPosition;
			nameTag.transform.localRotation = calRotation;
			*/
			// v.2 --> needs to be in Update()
			/*
			nameTag.transform.position = Vector3.Lerp (nameTag.transform.position, realPosition, 0.1f);
			nameTag.transform.localRotation = Quaternion.Lerp (nameTag.transform.localRotation, realRotation, 0.1f);
			*/
			realTagPosition.Set (realPosition.x, realPosition.y+1.8f, realPosition.z);
		}
	}
}
