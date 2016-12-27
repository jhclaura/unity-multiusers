using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyManagement : MonoBehaviour {

	public SocketManagement socketManagement;
	public GameObject eyeCam;
	public GameObject body;
	public GameObject viveCam;

	Dictionary<string, object> trans;

	public GameObject nameTag;

	// Use this for initialization
	void Start () {
		trans = new Dictionary<string, object>();
		trans.Add ("index", socketManagement.whoIamInLife);
		trans.Add ("type", "unity");

		if (socketManagement.viveVR)
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
	
	// Update is called once per frame
	void Update () {
		if (!socketManagement.connected) // || !viveCam.activeSelf
			return;

		if (socketManagement.viveVR)
		{
			//if (viveCam.activeSelf) {
			trans ["posX"] = viveCam.transform.position.x;
			trans ["posY"] = viveCam.transform.position.y;
			trans ["posZ"] = viveCam.transform.position.z;
			trans ["rotY"] = viveCam.transform.eulerAngles.y;

			trans ["quaX"] = viveCam.transform.rotation.x;
			trans ["quaY"] = viveCam.transform.rotation.y;
			trans ["quaZ"] = viveCam.transform.rotation.z;
			trans ["quaW"] = viveCam.transform.rotation.w;

			body.transform.position = new Vector3 (
				viveCam.transform.position.x, 
				viveCam.transform.position.y-1.5f,
				viveCam.transform.position.z
			);
			body.transform.localEulerAngles = new Vector3 (0f, viveCam.transform.eulerAngles.y, 0f);

			UpdateNameTag (viveCam.transform.position, body.transform.localEulerAngles);
			//}
		} else {
			trans ["posX"] = transform.position.x;
			trans ["posY"] = transform.position.y;
			trans ["posZ"] = transform.position.z;
			trans ["rotY"] = transform.eulerAngles.y;

			trans ["quaX"] = eyeCam.transform.rotation.x;
			trans ["quaY"] = eyeCam.transform.rotation.y;
			trans ["quaZ"] = eyeCam.transform.rotation.z;
			trans ["quaW"] = eyeCam.transform.rotation.w;

			body.transform.localEulerAngles = new Vector3 (0f, eyeCam.transform.eulerAngles.y, 0f);

			UpdateNameTag (transform.position, body.transform.localEulerAngles);
		}

		socketManagement.Manager.Socket.Emit ("update position", trans);

	}

	void UpdateNameTag(Vector3 pos, Vector3 eulerRot){
		if (nameTag) {
			pos.y += 1.8f;
			nameTag.transform.position = pos;
			nameTag.transform.localEulerAngles = eulerRot;
		}
	}
}
