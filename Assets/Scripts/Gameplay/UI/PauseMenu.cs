using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{

    public GameObject background;
    public GameObject title;
    public GameObject settingsContainer;

    public Button resume;
    public Button settings;
    public Button quit;

    public Button backButton;

    public Slider volumeSlider;

    public Slider sensitivityX;
    public Slider sensitivityY;

    public Toggle invertX;
    public Toggle invertY;

    public TMP_Dropdown textSpeed;

    GlobalsController gc;
    UIController uic;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
        uic = UIController.Instance;
        //uic.pauseMenu = gameObject;

        resume.onClick.AddListener(gc.Unpause);
        settings.onClick.AddListener(openSettingsMenu);

        settingsContainer.SetActive(false);
        gameObject.SetActive(false);
    }

    void OnEnable(){
        if(gc == null){
            gc = GlobalsController.Instance;
            uic = UIController.Instance;
            //uic.pauseMenu = gameObject;
        }

        exitSettingsMenu();
        resume.Select();
        resume.OnSelect(null);
    }

    public void openSettingsMenu(){
        title.SetActive(false);
        background.SetActive(false);
        resume.gameObject.SetActive(false);
        settings.gameObject.SetActive(false);
        quit.gameObject.SetActive(false);

        settingsContainer.SetActive(true);

        volumeSlider.value = gc.saveData["SETTINGS_master-volume"];
        sensitivityX.value = gc.saveData["SETTINGS_sensitivity-x"];
        sensitivityY.value = gc.saveData["SETTINGS_sensitivity-y"];
        invertX.isOn = gc.saveData["SETTINGS_invert-x"];
        invertY.isOn = gc.saveData["SETTINGS_invert-y"];
        textSpeed.value = gc.saveData["SETTINGS_text-speed"];

        uic.inSettings = true;
    }

    public void exitSettingsMenu(){
        settingsContainer.SetActive(false);

        title.SetActive(true);
        background.SetActive(true);
        resume.gameObject.SetActive(true);
        settings.gameObject.SetActive(true);
        quit.gameObject.SetActive(true);

        uic.inSettings = false;
    }

    public void quitToMainMenu(){
        uic.HideAll();
        gc.LoadingScreenToScene("Title");
    }
}
