using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextWriter : MonoBehaviour
{

    [Header("Properties")]
    public TMP_Text textObject;
    [Range(10, 40)]
    public int writeSpeed = 30;
    [Space(5)]

    [Header("Progress")]
    public string writing;
    public int charIndex;

    public bool isWriting { get{ return writing != null; } }
    [Space(5)]

    float timePerCharacter;
    float timer;

    GlobalsController gc;
    UIController uic;

    void Awake()
    {
        if(textObject == null){
            textObject = gameObject.GetComponent<TMP_Text>();
        }

        timePerCharacter = 1f / writeSpeed;

        writing = null;
        textObject.text = "";
        charIndex = 0;
    }

    void Load(){
        if(gc == null || uic == null){
            gc = GlobalsController.Instance;
            uic = UIController.Instance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(writing != null){
            timer -= Time.deltaTime;
            if(timer <= 0){
                // Display next character
                timer = timePerCharacter;
                charIndex++;

                // Make rest of string invisible so no resize on line break
                textObject.text = writing.Substring(0, charIndex);
                textObject.text += "<color=#00000000>" + writing.Substring(charIndex) + "</color>";

                gc.audioSource.time = 0;
                gc.audioSource.Play();

                if(charIndex >= writing.Length){
                    SkipWriting();
                }
            }
        }
    }

    public void Write(string _toWrite){
        Load();

        textObject.text = "";
        writing = _toWrite;
        gc.audioSource.clip = uic.scrollTextSound;

        charIndex = 0;
    }

    public void SkipWriting(){
        textObject.text = writing;
        writing = null;

        gc.audioSource.Pause();
        gc.audioSource.clip = uic.nextMessageSound;
        gc.audioSource.Play();
    }
}
