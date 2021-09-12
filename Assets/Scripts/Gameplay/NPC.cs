using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using QuantumTek.QuantumDialogue;

public class NPC : MonoBehaviour
{

    public string npcName;
    public string conversationName;
    public bool isSimple; // A simple NPC always says the same dialogue
    public bool rotateOnTalk;

    QD_DialogueHandler handler;

    public bool isActivated;
    public bool isTalking;
    bool ended;

    bool rotateToPlayer;

    GlobalsController gc;
    UIController uic;

    void Start(){
        gc = GlobalsController.Instance;
        uic = UIController.Instance;

        handler = uic.getDialogueHandler();
    }

    // Update is called once per frame
    void Update()
    {
        if(ended){
            return;
        }

        if(rotateToPlayer){
            Vector3 destination = new Vector3(
                gc.player.transform.position.x,
                transform.position.y,
                gc.player.transform.position.z
            );

            Quaternion prevRotation = transform.rotation;
            transform.LookAt(destination);
            Quaternion newRotation = transform.rotation;

            transform.rotation = Quaternion.Lerp(prevRotation,
                                                 newRotation,
                                                 CONSTANTS.NPC_ROTATE_SPEED);
        }
    }

    public void Activate(){
        if(isActivated){
            return;
        }
        isActivated = true;

        if(isSimple){
            Talk();
            return;
        }

        Debug.Log("This NPC is trying to do something else");
    }

    private void Talk(){
        handler.SetConversation(conversationName);

        isTalking = true;

        uic.dialogueObject.SetActive(true);
        gc.player.lockMovement = true;

        if(rotateOnTalk){
            rotateToPlayer = true;
        }

        SetText();
    }

    private void SetText()
    {
        // Clear everything
        uic.speakerName.SetText("");
        uic.dialogueText.gameObject.SetActive(false);
        uic.dialogueText.SetText("");

        // If at the end, don't do anything
        if (ended)
            return;

        // Generate choices if a choice, otherwise display the message
        if (handler.currentMessageInfo.Type == QD_NodeType.Message)
        {
            QD_Message message = handler.GetMessage();

            uic.speakerName.SetText(message.SpeakerName);
            uic.dialogueText.SetText(message.MessageText);
            uic.dialogueText.gameObject.SetActive(true);

            if(message.Clip != null){
                gc.audioSource.Pause();
                gc.audioSource.clip = message.Clip;
                gc.audioSource.Play();
            } else {
                gc.audioSource.Pause();
                gc.audioSource.clip = uic.nextMessageSound;
                gc.audioSource.Play();
            }

            Sprite speakerSprite = handler.dialogue.GetSpeaker(npcName).Icon;
            uic.speakerImage.sprite = speakerSprite;
        }
    }

    public void Next(int choice = -1)
    {

        if (ended){
            return;
        }
        // Go to the next message
        handler.NextMessage(choice);
        // Set the new text
        SetText();
        // End if there is no next message
        if (handler.currentMessageInfo.ID < 0){
            ended = true;
            rotateToPlayer = false;

            isTalking = false;
            if(isSimple){
                isActivated = false;
                ended = false;

                gc.audioSource.Pause();
                gc.audioSource.clip = uic.endConversationSound;
                gc.audioSource.Play();

                gc.player.isInteracting = false;

                handler.SetConversation(conversationName); // Simple NPCs repeat dialogue
            }

            uic.dialogueObject.SetActive(false);
            gc.player.lockMovement = false;
        }
    }
}
