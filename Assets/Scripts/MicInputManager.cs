using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class MicInputManager : MonoBehaviour {

	AudioSource source;
	string selectedDevice;
	int minFreq, maxFreq;

	// larger sample sizes will yield more "accurate" analysis at the cost of slower analysis
	// must be a power of 2. Min = 64. Max = 8192
	public static int sampleCount = 512;

	public const int BANDS = 8; //4
//	public float[] freqData = new float[BANDS]; // averaged amplitude of freqData per band
	public float[] freqData;
	public float[] rawData = new float[sampleCount]; // raw output data of every analyzed sample: values between -1.0 to 1.0

	public float masterGain = 1f;
	public bool useBands = false;

	[SerializeField]
	protected AudioClip clip;
	protected float[] band = new float[BANDS]; // used for accumulating freqData

	// Split the spectrum into bands at each crossover point
	[SerializeField]
	int[] crossovers = new int[BANDS]{ 40, 80, 150, 300, 600, 1200, 2400, 6000 }; //10, 80, 150, 300

	// multiply the amplitude of each band. higher frequencies require more boosting
	[SerializeField]
	float[] bandGain = new float[BANDS]{ 250f, 2500f, 8000f, 15000f, 15000f, 30000f, 30000f, 30000f }; //250f, 2500f, 8000f, 15000f

	[SerializeField]
	bool easeAmplitude; // smooth amplitude changes vs. immediate change
	[SerializeField]
	[Range(0.001f, 0.1f)]
	float climbRate;
	[SerializeField]
	[Range(0.001f, 0.1f)]
	float fallRate;

	// Use this for initialization
	void Start () {
		if (useBands) {
			freqData = new float[BANDS];
		} else {
			freqData = new float[sampleCount];
		}
		Debug.Log(freqData.Length);
		/*
		foreach (string device in Microphone.devices) {
			Debug.Log("Name: " + device);
		}
		*/

		// Select microphone as the device
		selectedDevice = Microphone.devices [0].ToString ();
		Debug.Log("SelectedDevice: " + selectedDevice);

		// Get the frequency capabilities of a device.
		Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);

		if ((minFreq + maxFreq) == 0)
			maxFreq = 48000;
		Debug.Log("minFreq: " + minFreq + ", maxFreq: " + maxFreq);

		// Set mic input as an AudioClip on a looping AudioSource
		source = GetComponent<AudioSource>();
		source.loop = true; 
		source.clip = Microphone.Start(selectedDevice, true, 5, maxFreq); //deviceName, loop, lengthSec, frequency

		// choose your desired latency sample rate
		// If you want no latency, set this to “0” samples before the audio starts to play
		while (!(Microphone.GetPosition (selectedDevice) > 0)) {
		}

		source.Play();
	}
	
	// Update is called once per frame
	void Update () {
//		float amp = GetAverageAmplitude ();
//		Debug.Log (amp);

		GetMultibandAmplitude (useBands);
	}

	void GetMultibandAmplitude(bool _useBands) {
		source.GetSpectrumData (rawData, 0, FFTWindow.Hamming);

		if (_useBands) {

			// ====== Setup Threshold ======
			float[] bandThresholds = new float[BANDS];
			for (int i = 0; i < BANDS; i++) {

				// Set threshold for each band
				float min = (i > 0 ? crossovers [i - 1] : 0);
				bandThresholds [i] = crossovers [i] - min;

				// Reset accumulated/summed data
				band [i] = 0f;
			}

			// ====== Assing freq to Bands ======
			int k = 0;
			for (int i = 0; i < rawData.Length; i++) {

				if (k > BANDS - 1)
					break;

				// Sum each frequency data per band
				band [k] += rawData [i];

				// If reach another band zone
				if (i > crossovers [k]) {

					// Divide total amplitude by total number of datapoints = average amplitude for band
					float bandAmp = Mathf.Abs (band [k] / bandThresholds [k]) * bandGain [k];

					if (easeAmplitude) {
						// if analyzed amplitude is larger than previous amplitude
						// ease to new amplitude at climbRate, otherwise ease at fallRate
						freqData [k] = Mathf.Lerp (freqData [k], bandAmp * masterGain, bandAmp * (bandAmp > freqData [k] ? climbRate : fallRate));
					} else {
						freqData [k] = bandAmp * masterGain;
					}

					k++;
				}
			}
		} else {

			for (int i = 0; i < rawData.Length; i++) {

				float freqAmp = Mathf.Abs (rawData [i] );// * bandGain [k];

				if (easeAmplitude) {
					// if analyzed amplitude is larger than previous amplitude
					// ease to new amplitude at climbRate, otherwise ease at fallRate
					freqData [i] = Mathf.Lerp (freqData [i], freqAmp * masterGain, freqAmp * (freqAmp > freqData [i] ? climbRate : fallRate));
				} else {
					freqData [i] = freqAmp * masterGain;
				}

			}
		}


	}

	float GetAverageAmplitude() {
		float[] spectrum = new float[sampleCount];
		source.GetSpectrumData (spectrum, 0, FFTWindow.Hamming);	// samples array, channel, FFTWindow type
		// FFTWindow: Spectrum analysis windowing types - to reduce leakage of signals

		float a = 0f;
		foreach(float s in spectrum){
			a += Mathf.Abs (s);
		}
		return a / sampleCount;
	}
}
