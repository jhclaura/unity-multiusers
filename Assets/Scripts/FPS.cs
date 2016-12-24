using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPS : MonoBehaviour {
  private Text textField;
  private float fps = 60;

  void Awake() {
    textField = GetComponent<Text>();
  }

  void LateUpdate() {
    string text = "";

    float interp = Time.deltaTime / (0.5f + Time.deltaTime);
    float currentFPS = 1.0f / Time.deltaTime;
    fps = Mathf.Lerp(fps, currentFPS, interp);
    text += Mathf.RoundToInt(fps) + "fps";
    textField.text = text;
  }
}
