using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public GameObject settingsPanel = null;
    public Text sensitivityLabel;
    public float volume = 0.5f;
    float sensitivity = 0.5f;
	// Use this for initialization
	void Start () {
        AudioListener.volume = volume;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleSettingsPanel() {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ChangeVolume(Slider slider) {
        volume = slider.value;
        AudioListener.volume = volume;
    }

    public void ChangeSensitivity(Scrollbar scroll) {
        sensitivity = scroll.value;
        sensitivityLabel.text = ""+Math.Round(sensitivity,2);
    }

    public void StartGame() {
        SceneManager.LoadScene(1);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
