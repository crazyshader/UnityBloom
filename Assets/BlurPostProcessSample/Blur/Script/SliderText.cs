using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour {
    Slider slider;
    Text text;

    string content;
    void Start () {
        slider = GetComponent<Slider>();
        text = GetComponentInChildren<Text>();
        content = text.text;
    }
	
	void Update () {
        text.text = string.Format("{0}:{1}", content, slider.value.ToString());
    }
}
