using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterHandler : MonoBehaviour
{
    GlobalsController gc;
    LevelManager lm;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
        lm = LevelManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEnterWater(){
        lm.bgMusic.volume /= 2f;
    }

    public void OnExitWater(){
        lm.bgMusic.volume *= 2f;
    }
}
