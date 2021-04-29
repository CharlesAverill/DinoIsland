using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject settingsMenu;

    public Slider volumeSlider;

    GlobalsController gc;

    void Start()
    {
        gc = GlobalsController.Instance;

        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        volumeSlider.value = gc.saveData["SETTINGS_master-volume"];
    }

    public void setMasterVolume(float newVolume){
        gc.saveData["SETTINGS_master-volume"] = newVolume;
        AudioListener.volume = newVolume;
    }

    public void switchToSettings(){
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void switchToMain(){
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }
}
