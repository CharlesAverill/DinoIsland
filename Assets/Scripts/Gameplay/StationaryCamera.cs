using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryCamera : MonoBehaviour
{

    public enum FollowMode {
        PointToPlayer
    }

    public FollowMode followMode;

    GlobalsController gc;
    Transform target;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
        target = gc.player.transform;

        gc.player.mainCamera.GetComponent<Camera>().enabled = false;
        gc.player.mainCamera = transform;
    }

    // Update is called once per frame
    void Update()
    {
        switch(followMode){
            case FollowMode.PointToPlayer:
                transform.LookAt(target);
                break;
            default:
                Debug.Log("Unrecognized followMode: " + followMode);
                break;
        }
    }
}
