using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public Button resume;
    public Button quit;

    // Start is called before the first frame update
    void Start()
    {
        GlobalsController gc = GlobalsController.Instance;

        resume.onClick.AddListener(gc.Unpause);
        quit.onClick.AddListener(gc.Quit);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
