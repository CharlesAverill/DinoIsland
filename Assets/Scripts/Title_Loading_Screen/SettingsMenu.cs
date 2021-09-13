using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject settingsMenu;

    public Button playButton;
    public Button backButton;

    public Slider volumeSlider;

    public Slider sensitivityX;
    public Slider sensitivityY;

    public Toggle invertX;
    public Toggle invertY;

    GlobalsController gc;

    void Start()
    {
        gc = GlobalsController.Instance;

        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        volumeSlider.value = gc.saveData["SETTINGS_master-volume"];
        sensitivityX.value = gc.saveData["SETTINGS_sensitivity-x"];
        sensitivityY.value = gc.saveData["SETTINGS_sensitivity-y"];
        invertX.isOn = gc.saveData["SETTINGS_invert-x"];
        invertY.isOn = gc.saveData["SETTINGS_invert-y"];
    }

    public void setMasterVolume(float newVolume){
        gc.saveData["SETTINGS_master-volume"] = newVolume;
        AudioListener.volume = newVolume;
    }

    public void setSensitivityX(float newValue){
        gc.saveData["SETTINGS_sensitivity-x"] = newValue;
    }

    public void setSensitivityY(float newValue){
        gc.saveData["SETTINGS_sensitivity-y"] = newValue;
    }

    public void setInvertX(bool newValue){
        gc.saveData["SETTINGS_invert-x"] = newValue;
    }

    public void setInvertY(bool newValue){
        gc.saveData["SETTINGS_invert-y"] = newValue;
    }

    public void switchToSettings(){
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);

        Debug.Log(EventSystem.current.currentSelectedGameObject);
    }

    public void switchToMain(){
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }
}
