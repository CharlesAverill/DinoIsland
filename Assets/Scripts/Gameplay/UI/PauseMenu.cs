using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{

    public Button resume;
    public Button quit;

    // Start is called before the first frame update
    void Start()
    {
        GlobalsController gc = GlobalsController.Instance;
        UIController uic = UIController.Instance;
        uic.pauseMenu = gameObject;

        resume.onClick.AddListener(gc.Unpause);
        quit.onClick.AddListener(gc.Quit);

        gameObject.SetActive(false);
    }

    void OnEnable(){
        //EventSystem.current.SetSelectedGameObject(resume.gameObject);
        resume.Select();
        resume.OnSelect(null);
    }
}
