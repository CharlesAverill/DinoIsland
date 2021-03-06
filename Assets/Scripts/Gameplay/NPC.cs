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

        gc.player.lockMovement = true;
        gc.enemyFreeze = true;

        if(rotateOnTalk){
            rotateToPlayer = true;
        }

        // Clear everything
        uic.ClearDialogue();

        SetText();
    }

    private void SetText()
    {
        // If at the end, don't do anything
        if (ended)
            return;

        // Generate choices if a choice, otherwise display the message
        if (handler.currentMessageInfo.Type == QD_NodeType.Message)
        {
            QD_Message message = handler.GetMessage();

            uic.SetDialogue(message.SpeakerName, handler.dialogue.GetSpeaker(npcName).Icon, message.MessageText);
        }
    }

    public void Next(int choice = -1)
    {
        if (ended){
            return;
        }

        if(uic.dialogueTextWriter.isWriting){
            uic.dialogueTextWriter.SkipWriting();
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
                gc.enemyFreeze = false;

                handler.SetConversation(conversationName); // Simple NPCs repeat dialogue
            }

            uic.HideDialogue();
            gc.player.lockMovement = false;
        }
    }
}
