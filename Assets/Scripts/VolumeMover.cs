using UnityEngine;
using System.Collections;

public class VolumeMover : MonoBehaviour {

	public MicInputManager micInput;
	[Range(0.001f,10f)]
	public float moveScale = 1f;
	public GameObject cubePrefab;

	Vector3 direction = new Vector3(0,1,0);

	GameObject[] cubes;
	Transform[] cubeTransforms;
	Vector3[] cubeOriPos;

	static int bandsCount;

	// Use this for initialization
	void Start () {
		if (micInput.useBands) {
			bandsCount = MicInputManager.BANDS;
		} else {
			bandsCount = MicInputManager.sampleCount;
		}

		cubes = new GameObject[bandsCount];
		cubeTransforms = new Transform[bandsCount];
		cubeOriPos = new Vector3[bandsCount];

		for (int i = 0; i < bandsCount; i++) {
			Vector3 cubePos = transform.position;
			cubePos.x -= i * 2f;
			GameObject cube = (GameObject) Instantiate (cubePrefab, cubePos, Quaternion.identity);
			cube.transform.parent = transform;

			cubes [i] = cube;
			cubeTransforms[i] = cubes [i].transform;
			cubeOriPos [i] = cubeTransforms[i].position;
		}
		Debug.Log (cubes.Length);
			
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < cubes.Length; i++) {
			cubeTransforms[i].position = cubeOriPos[i] + direction * micInput.freqData[i] * moveScale;
		}
	}
}
