using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyManagement : MonoBehaviour {

	[HideInInspector]
	public SocketManagement socketManagement;
	[HideInInspector]
	public GameObject body;
	[HideInInspector]
	public GameObject eyeCam;
	[HideInInspector]
	public GameObject viveCam;
	[HideInInspector]
	public GameObject nameTag;

	private Dictionary<string, object> trans;
	private float updateInterval;

	void Start ()
	{
		trans = new Dictionary<string, object>();
		trans.Add ("index", socketManagement.whoIamInLife);
		trans.Add ("type", "unity");

		if (socketManagement.isViveVR)
		{
			trans ["type"] = "vive";
			trans.Add ("posX", viveCam.transform.position.x);
			trans.Add ("posY", viveCam.transform.position.y);
			trans.Add ("posZ", viveCam.transform.position.z);
			trans.Add ("rotY", viveCam.transform.eulerAngles.y);

			trans.Add ("quaX", viveCam.transform.rotation.x);
			trans.Add ("quaY", viveCam.transform.rotation.y);
			trans.Add ("quaZ", viveCam.transform.rotation.z);
			trans.Add ("quaW", viveCam.transform.rotation.w);
		} else {
			trans.Add ("posX", transform.position.x);
			trans.Add ("posY", transform.position.y);
			trans.Add ("posZ", transform.position.z);
			trans.Add ("rotY", transform.eulerAngles.y);

			trans.Add ("quaX", eyeCam.transform.rotation.x);
			trans.Add ("quaY", eyeCam.transform.rotation.y);
			trans.Add ("quaZ", eyeCam.transform.rotation.z);
			trans.Add ("quaW", eyeCam.transform.rotation.w);
		}
			
	}
	
	void Update () {
		if (!socketManagement.isConnected) // || !viveCam.activeSelf
			return;

		// UPDATE: local self object transform
		if (socketManagement.isViveVR)
		{
			body.transform.position = new Vector3 (
				viveCam.transform.position.x, 
				viveCam.transform.position.y-1.5f,
				viveCam.transform.position.z
			);
			body.transform.localEulerAngles = new Vector3 (0f, viveCam.transform.eulerAngles.y, 0f);
			UpdateNameTag (viveCam.transform.position, body.transform.localEulerAngles);
		}
		else 
		{
			// MOVE BY PRESSING SCREEN OR MOUSE
			if (Input.touchCount > 0 || Input.GetMouseButton(0))
			{
				//Debug.Log ("touch!");
				transform.Translate(body.transform.forward * Time.deltaTime);
			}
			body.transform.localEulerAngles = new Vector3 (0f, eyeCam.transform.eulerAngles.y, 0f);
			UpdateNameTag (transform.position, body.transform.localEulerAngles);
		}

		// SEND: transform data to Server
		// Network send rate ideal 9 fps
		// ref: https://forum.unity3d.com/threads/unet-multiplayer-interpolation-and-latency-compensation.398102/
		updateInterval += Time.deltaTime;
		if(updateInterval > 0.11f) // 9 times per second (?)
		{
			updateInterval = 0f;

			if (socketManagement.isViveVR)
			{
				trans ["posX"] = viveCam.transform.position.x;
				trans ["posY"] = viveCam.transform.position.y;
				trans ["posZ"] = viveCam.transform.position.z;
				trans ["rotY"] = viveCam.transform.eulerAngles.y;

				trans ["quaX"] = viveCam.transform.rotation.x;
				trans ["quaY"] = viveCam.transform.rotation.y;
				trans ["quaZ"] = viveCam.transform.rotation.z;
				trans ["quaW"] = viveCam.transform.rotation.w;
			}
			else
			{
				trans ["posX"] = transform.position.x;
				trans ["posY"] = transform.position.y;
				trans ["posZ"] = transform.position.z;
				trans ["rotY"] = transform.eulerAngles.y;

				trans ["quaX"] = eyeCam.transform.rotation.x;
				trans ["quaY"] = eyeCam.transform.rotation.y;
				trans ["quaZ"] = eyeCam.transform.rotation.z;
				trans ["quaW"] = eyeCam.transform.rotation.w;
			}
			socketManagement.Manager.Socket.Emit ("update position", trans);
		}
	}

	void UpdateNameTag(Vector3 pos, Vector3 eulerRot){
		
		if (nameTag) {
			if (socketManagement.isViveVR)
				pos.y += 0.5f;
			else
				pos.y += 1.8f;
			nameTag.transform.position = pos;
			nameTag.transform.localEulerAngles = eulerRot;
		}
	}
}
