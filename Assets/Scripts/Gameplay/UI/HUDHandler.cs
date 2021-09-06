using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HUDHandler : MonoBehaviour
{
    [Header("General")]
    public float timeBeforeHide;
    public bool showingHUD;

    public Color flashColor;
    public Color goodHealthColor;
    public Color badHealthColor;

    bool finishedLoad;

    float fillTime = 0f;
    [Space(5)]

    [Header("Health")]
    public Image healthBar;
    float lastHealthFillAmount;
    float healthFillAmount;

    public Vector3 healthHiddenPosition;
    public Vector3 healthShownPosition;
    Vector3 healthTargetPosition;

    public bool showingHealth;
    [Space(5)]

    [Header("Pickups")]
    public GameObject pickupsContainer;
    public Slideshow pickupsSlideshow;
    public TMP_Text pickupsNumber;

    public Vector3 pickupsHiddenPosition;
    public Vector3 pickupsShownPosition;
    Vector3 pickupsTargetPosition;
    public bool showingPickups;

    float hideTimer;
    [Space(5)]

    // Globals
    GlobalsController gc;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
    }

    // Executes once after Start
    void Load(){
        healthFillAmount = gc.player.currentStats.healthPercentage;
        healthBar.color = Color.Lerp(badHealthColor, goodHealthColor, healthFillAmount);

        pickupsNumber.text = "" + gc.currentPickups;

        showingHUD = true;

        finishedLoad = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!finishedLoad){
            Load();
        }

        if(showingHUD && !(showingHealth && showingPickups)){
            showingPickups = true;
            pickupsTargetPosition = pickupsShownPosition;

            showingHealth = true;
            healthTargetPosition = healthShownPosition;
        }

        hideTimer += Time.deltaTime;

        if(hideTimer > timeBeforeHide){
            hideTimer = 0f;

            pickupsTargetPosition = pickupsHiddenPosition;
            healthTargetPosition = healthHiddenPosition;

            showingHUD = false;
            showingHealth = false;
            showingPickups = false;
        }

        // Health
        if(Mathf.Abs(healthFillAmount - healthBar.fillAmount) != 0){
            fillTime += 3 * Time.deltaTime;
            healthBar.fillAmount = Mathf.SmoothStep(lastHealthFillAmount, healthFillAmount, fillTime);
            healthBar.color = Color.Lerp(flashColor, Color.Lerp(badHealthColor, goodHealthColor, healthFillAmount), fillTime);
        } else {
            fillTime = 0f;
        }

        if(Vector3.Distance(healthBar.transform.localPosition, healthTargetPosition) > 0.1f){
            healthBar.transform.localPosition = Vector3.Lerp(healthBar.transform.localPosition, healthTargetPosition, 10f * Time.deltaTime);
        }

        // Pickups
        if(Vector3.Distance(pickupsContainer.transform.localPosition, pickupsTargetPosition) > 0.1f){
            pickupsContainer.transform.localPosition = Vector3.Lerp(pickupsContainer.transform.localPosition, pickupsTargetPosition, 10f * Time.deltaTime);
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
            pickupsTargetPosition = pickupsShownPosition;
        }
    }

    public void updateHealth(float newHealthFillAmount){
        lastHealthFillAmount = healthFillAmount;
        healthFillAmount = newHealthFillAmount;

        hideTimer = 0f;

        if(!showingHealth){
            showingHealth = true;
            healthTargetPosition = healthShownPosition;
        }
    }
}
