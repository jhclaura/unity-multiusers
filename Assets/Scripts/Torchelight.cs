using UnityEngine;
using System.Collections;

public class Torchelight : MonoBehaviour {
	
	Light FireLight;
	public float MaxLightIntensity;
	public float IntensityLight;
	

	void Start () {
		FireLight = GetComponent<Light> ();
		IntensityLight = FireLight.intensity;
	}
	

	void Update () {
		if (IntensityLight<0) IntensityLight=0;
		if (IntensityLight>MaxLightIntensity) IntensityLight=MaxLightIntensity;		
		FireLight.intensity=IntensityLight/2f+Mathf.Lerp(IntensityLight-0.1f,IntensityLight+0.1f,Mathf.Cos(Time.time*30));	
	}
}
