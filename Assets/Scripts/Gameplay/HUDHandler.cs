using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class HUDHandler : MonoBehaviour
{

    [Header("Pickups")]
    public Slideshow pickupsSlideshow;
    public TMP_Text pickupsNumber;
    public Vector3 hiddenPosition;
    public Vector3 shownPosition;
    Vector3 targetPosition;

    public float timeBeforeHide;
    public bool showingPickups;

    float hideTimer;
    [Space(5)]

    GlobalsController gc;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
        pickupsNumber.text = "" + gc.currentPickups;
        showingPickups = true;
    }

    // Update is called once per frame
    void Update()
    {
        hideTimer += Time.deltaTime;

        if(hideTimer > timeBeforeHide){
            hideTimer = 0f;
            targetPosition = hiddenPosition;
            showingPickups = false;
        }

        if(Vector3.Distance(transform.localPosition, targetPosition) > 0.1f){
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 10f * Time.deltaTime);
        } else if(!showingPickups){
            pickupsSlideshow.End();
        }
    }

    public void updatePickups(int pickups){
        pickupsNumber.text = "" + gc.currentPickups;

        pickupsSlideshow.Begin(!showingPickups);

        hideTimer = 0f;

        if(!showingPickups){
            showingPickups = true;
            targetPosition = shownPosition;
        }
    }
}
