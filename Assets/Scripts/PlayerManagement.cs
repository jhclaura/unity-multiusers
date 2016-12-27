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
		if (type == "three")
		{
			Vector3 position = new Vector3 (posX, posY, -posZ);
			player.transform.position = position;

			Quaternion rotation = Quaternion.Euler (Vector3.up * -180f);
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
			Vector3 position = new Vector3 (posX, posY, posZ);
			if (type == "vive")
				position.y -= 2f;
			player.transform.position = position;

			Quaternion rotation = new Quaternion (rotX, rotY, rotZ, rotW);
			// Head
			// if(playerHead.activeSelf)
				playerHead.transform.rotation = rotation;

			// Body
			rotation.x = 0;
			rotation.z = 0;
			playerBody.transform.rotation = rotation;
		}
	}
}
