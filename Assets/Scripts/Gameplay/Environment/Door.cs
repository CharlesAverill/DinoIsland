using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    public enum DoorType {
        KeepRunning,
        Stop,
        TurnKnob
    }

    GlobalsController gc;

    public DoorType doorType;

    public string destinationScene;
    public string spawnPointName;

    [Header("Smart Loading")]
    public bool useSmartLoading;
    public float distanceRequiredForLoad;
    public bool startedLoading;
    [Space(5)]

    float timeCount;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;
        if(useSmartLoading && timeCount > 0.2f){
            if(!startedLoading && Vector3.Distance(transform.position, gc.player.transform.position) < distanceRequiredForLoad){
                startedLoading = true;
                LevelManager.Instance.LoadScene(destinationScene, additive: true);
            }
            timeCount = 0f;
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            switch(doorType){
                case DoorType.KeepRunning:
                    other.gameObject.GetComponent<PlayerController>().forceContinueSameDirection = true;
                    break;
                case DoorType.Stop:
                    other.gameObject.GetComponent<PlayerController>().lockMovement = true;
                    break;
                case DoorType.TurnKnob:
                    break;
            }

            if(useSmartLoading){
                LevelManager.Instance.ActivateAdditiveScene(destinationScene, spawnPointName);
            } else {
                LevelManager.Instance.LoadScene(destinationScene, spawnPointName);
            }
        }
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player" && useSmartLoading){
            LevelManager.Instance.RemoveAdditiveScene(destinationScene);
            startedLoading = false;
        }
    }
}
