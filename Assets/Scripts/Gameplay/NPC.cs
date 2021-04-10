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

    bool isActivated;
    bool isTalking;
    bool ended;

    bool rotateToPlayer;

    GlobalsController gc;

    void Start(){
        gc = GlobalsController.Instance;
        handler = gc.getDialogueHandler();
    }

    // Update is called once per frame
    void Update()
    {
        if(ended){
            return;
        }

        if(rotateToPlayer){
            Debug.Log("Rotating");
            Vector3 destination = new Vector3(
                gc.player.transform.position.x,
                transform.position.y,
                gc.player.transform.position.z
            );

            Quaternion prevRotation = transform.rotation;
            transform.LookAt(destination);
            Quaternion newRotation = transform.rotation;

            Debug.Log(prevRotation + " " + newRotation);

            transform.rotation = Quaternion.Lerp(prevRotation,
                                                 newRotation,
                                                 CONSTANTS.NPC_ROTATE_SPEED);
        }

        if(isTalking && gc.player.isInteracting()){
            Next();
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

        gc.dialogueObject.SetActive(true);
        gc.player.lockMovement = true;

        if(rotateOnTalk){
            rotateToPlayer = true;
        }

        SetText();
    }

    private void SetText()
    {
        // Clear everything
        gc.speakerName.SetText("");
        gc.dialogueText.gameObject.SetActive(false);
        gc.dialogueText.SetText("");

        // If at the end, don't do anything
        if (ended)
            return;

        // Generate choices if a choice, otherwise display the message
        if (handler.currentMessageInfo.Type == QD_NodeType.Message)
        {
            QD_Message message = handler.GetMessage();

            gc.speakerName.SetText(message.SpeakerName);
            gc.dialogueText.SetText(message.MessageText);
            gc.dialogueText.gameObject.SetActive(true);

            Sprite speakerSprite = handler.dialogue.GetSpeaker(npcName).Icon;
            gc.speakerImage.sprite = speakerSprite;
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
                handler.SetConversation(conversationName); // Simple NPCs repeat dialogue
            }

            gc.dialogueObject.SetActive(false);
            gc.player.lockMovement = false;
        }
    }
}
