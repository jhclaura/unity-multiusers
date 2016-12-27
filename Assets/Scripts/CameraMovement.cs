using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	GvrHead gvrHead;
	public GameObject body;

	// Use this for initialization
	void Start () {
		StartCoroutine (LateStart());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	protected IEnumerator LateStart()
	{
		yield return new WaitWhile (()=> GetComponent<GvrHead>() == null);
		Debug.Log ("Get gvr head!");
		gvrHead = GetComponent<GvrHead> ();
		gvrHead.trackPosition = true;
		gvrHead.target = body.transform;
	}
}
