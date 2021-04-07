using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuantumTek.QuantumDialogue;
using QuantumTek.QuantumUI;
using TMPro;
using RotaryHeart.Lib.SerializableDictionary;

public class GlobalsController : MonoBehaviour {

    private static GlobalsController _instance;

    public static GlobalsController Instance { get { return _instance; } }

    public PlayerController player;

    public GameObject dialogueObject;
    public QUI_Bar dialogueBar;
    public TMP_Text dialogueText;
    public TMP_Text speakerName;
    public Image speakerImage;

    QD_DialogueHandler dialogueHandler;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public void Start(){
        dialogueObject.SetActive(false);
        dialogueHandler = null;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        Screen.SetResolution(640, 480, false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public QuantumTek.QuantumDialogue.QD_DialogueHandler getDialogueHandler(){
        if(dialogueHandler == null){
            dialogueHandler = GameObject.FindWithTag("DialogueHandler").GetComponent<QD_DialogueHandler>();
        }
        return dialogueHandler;
    }

}
