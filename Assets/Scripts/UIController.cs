using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuantumTek.QuantumDialogue;
using QuantumTek.QuantumUI;
using TMPro;

public class UIController : MonoBehaviour
{
    static UIController _instance;
    public static UIController Instance { get { return _instance; } }

    public HUDHandler hudHandler;

    public GameObject dialogueObject;
    public QUI_Bar dialogueBar;
    public TMP_Text dialogueText;
    public TextWriter dialogueTextWriter;
    public TMP_Text speakerName;
    public Image speakerImage;

    public AudioClip nextMessageSound;
    public AudioClip scrollTextSound;
    public AudioClip startConversationSound;
    public AudioClip endConversationSound;

    QD_DialogueHandler dialogueHandler;

    public GameObject pauseMenu;
    public bool isPaused;
    public bool inSettings;

    GlobalsController gc;

    void Awake()
    {
        // Only want 1 GlobalsController instance per scene
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        // Persist between scenes
        DontDestroyOnLoad (transform.gameObject);
    }

    void Start(){
        gc = GlobalsController.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetDialogue(){
        gc = GlobalsController.Instance;

        dialogueHandler = null;
        gc.setTextSpeed();
        dialogueObject.SetActive(false);
    }

    // Cache the dialogueHandler object
    public QuantumTek.QuantumDialogue.QD_DialogueHandler getDialogueHandler(){
        if(dialogueHandler == null){
            dialogueHandler = GameObject.FindWithTag("DialogueHandler").GetComponent<QD_DialogueHandler>();
        }
        return dialogueHandler;
    }

    public void ClearDialogue(){
        dialogueObject.SetActive(true);

        speakerName.text = "";
        dialogueText.text = "";
    }

    public void SetDialogue(string _speakerName, Sprite _speakerImage, string messageText){
        speakerName.text = _speakerName;
        speakerImage.sprite = _speakerImage;

        dialogueTextWriter.Write(messageText);
    }

    public void HideDialogue(){
        dialogueObject.SetActive(false);
    }

    public void HideAll(){
        dialogueObject.SetActive(false);
        pauseMenu.SetActive(false);
        hudHandler.gameObject.SetActive(false);
    }

    public void setMasterVolume(float newVolume){
        gc.setMasterVolume(newVolume);
    }

    public void setSensitivityX(float newValue){
        gc.setSensitivityX(newValue);
    }

    public void setSensitivityY(float newValue){
        gc.setSensitivityY(newValue);
    }

    public void setInvertX(bool newValue){
        gc.setInvertX(newValue);
    }

    public void setInvertY(bool newValue){
        gc.setInvertY(newValue);
    }

    public void setTextSpeed(int newSpeed){
        gc.setTextSpeed(newSpeed);
    }
}
