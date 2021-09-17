using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public TMP_Dropdown textSpeed;

    GlobalsController gc;

    void Start()
    {
        gc = GlobalsController.Instance;

        mainMenu.SetActive(true);
        settingsMenu.SetActive(true);

        volumeSlider.value = gc.saveData["SETTINGS_master-volume"];
        sensitivityX.value = gc.saveData["SETTINGS_sensitivity-x"];
        sensitivityY.value = gc.saveData["SETTINGS_sensitivity-y"];
        invertX.isOn = gc.saveData["SETTINGS_invert-x"];
        invertY.isOn = gc.saveData["SETTINGS_invert-y"];
        textSpeed.value = gc.saveData["SETTINGS_text-speed"];

        settingsMenu.SetActive(false);
    }

    public void switchToSettings(){
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void switchToMain(){
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void setMasterVolume(float newVolume){
        gc.setMasterVolume(newVolume);
    }

    public void setSensitivityX(float newValue){
        gc.setSensitivityX(newValue);
    }

    public void setSensitivityY(float newValue){
        gc.setSensitivityY(newValue);
    }

    public void setInvertX(bool newValue){
        gc.setInvertX(newValue);
    }

    public void setInvertY(bool newValue){
        gc.setInvertY(newValue);
    }

    public void setTextSpeed(int newSpeed){
        gc.setTextSpeed(newSpeed);
    }

    public void Quit(){
        gc.Quit();
    }
}
