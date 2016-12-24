using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyManagement : MonoBehaviour {

	public SocketManagement socketManagement;
	public GameObject eyeCam;
	public GameObject body;

	Dictionary<string, object> trans;

	// Use this for initialization
	void Start () {
		trans = new Dictionary<string, object>();
		trans.Add ("index", socketManagement.whoIamInLife);
		trans.Add ("type", "unity");
		trans.Add ("posX", transform.position.x);
		trans.Add ("posY", transform.position.y);
		trans.Add ("posZ", transform.position.z);
		trans.Add ("rotY", transform.eulerAngles.y);

		trans.Add ("quaX", eyeCam.transform.rotation.x);
		trans.Add ("quaY", eyeCam.transform.rotation.y);
		trans.Add ("quaZ", eyeCam.transform.rotation.z);
		trans.Add ("quaW", eyeCam.transform.rotation.w);

	}
	
	// Update is called once per frame
	void Update () {
		if (!socketManagement.connected)
			return;

		trans["posX"] = transform.position.x;
		trans["posY"] = transform.position.y;
		trans["posZ"] = transform.position.z;
		trans["rotY"] = transform.eulerAngles.y;

		trans["quaX"] = eyeCam.transform.rotation.x;
		trans["quaY"] = eyeCam.transform.rotation.y;
		trans["quaZ"] = eyeCam.transform.rotation.z;
		trans["quaW"] = eyeCam.transform.rotation.w;

		socketManagement.Manager.Socket.Emit ("update position", trans);

		body.transform.localEulerAngles = new Vector3 (0f, eyeCam.transform.eulerAngles.y, 0f);
	}
}
