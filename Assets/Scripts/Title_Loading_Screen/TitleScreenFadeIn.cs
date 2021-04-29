using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleScreenFadeIn : MonoBehaviour
{

    public Image fadeScreen;
    public TMP_Text fadeText;

    public GameObject IslandObject;
    public GameObject MainMenuObject;

    public float startFadeSeconds;
    public float endFadeSeconds;

    float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        fadeText.color = new Color32(255, 255, 255, 0);

        IslandObject.SetActive(false);
        MainMenuObject.SetActive(false);
        fadeScreen.gameObject.SetActive(true);
        fadeText.gameObject.SetActive(true);
        
        StartCoroutine(FadeTextIn());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(endFadeSeconds > timer && timer > startFadeSeconds){
            IslandObject.SetActive(true);
            MainMenuObject.SetActive(true);
            StartCoroutine(FadeAllOut());
        }
    }

    IEnumerator FadeTextIn(){
        for(float i = startFadeSeconds; i >= 0; i -= Time.deltaTime){
            fadeText.color = new Color32(255, 255, 255, (byte)(255 - (255 * i / 6)));
            yield return null;
        }
    }

    IEnumerator FadeAllOut(){
        for (float i = endFadeSeconds - startFadeSeconds; i >= 0; i -= Time.deltaTime)
        {
            fadeScreen.color = new Color(255, 255, 255, i);
            fadeText.color = new Color32(255, 255, 255, (byte)(255 * i / 2));
            yield return null;
        }
        fadeScreen.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
