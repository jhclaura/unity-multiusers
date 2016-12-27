using UnityEngine;
using System.Collections;

public class PlayerManagement : MonoBehaviour {

	public Color color;
	public int whoIam;
	public string username = "Anonymous";

	public GameObject player;
	public GameObject playerHead;
	public GameObject playerBody;
	public GameObject eyeCamera;
	public BodyManagement bodyMgmt;
	public SocketManagement socketManagement;
	public GameObject ViveCamRig;

	public GameObject nameTag;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnStartLocalPlayer() {
		playerHead.SetActive (false);
		bodyMgmt.enabled = true;
		bodyMgmt.socketManagement = socketManagement;

		bodyMgmt.nameTag = nameTag;

		if (socketManagement.viveVR) {
			ViveCamRig.SetActive(true);
		} else {
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

		if (nameTag) {
			position.y += 1.8f;
			nameTag.transform.position = position;
			nameTag.transform.localRotation = rotation;
		}
	}
}
