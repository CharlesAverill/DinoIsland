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
    public TMP_Text speakerName;
    public Image speakerImage;

    public AudioClip nextMessageSound;
    public AudioClip scrollTextSound;
    public AudioClip startConversationSound;
    public AudioClip endConversationSound;

    QD_DialogueHandler dialogueHandler;

    public GameObject pauseMenu;
    public bool isPaused;

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
        dialogueObject.SetActive(false);
    }

    // Cache the dialogueHandler object
    public QuantumTek.QuantumDialogue.QD_DialogueHandler getDialogueHandler(){
        if(dialogueHandler == null){
            dialogueHandler = GameObject.FindWithTag("DialogueHandler").GetComponent<QD_DialogueHandler>();
        }
        return dialogueHandler;
    }
}
