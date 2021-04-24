using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugUIHandler : MonoBehaviour
{

    public PlayerController player;

    public TMP_Text cameraMode;
    public TMP_Text position;
    public TMP_Text velocity;
    public TMP_Text rotation;
    public TMP_Text isGrounded;
    public TMP_Text movementLocked;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch(player.camMode){
            case PlayerController.CameraMode.FreeLook:
                cameraMode.SetText("Camera Mode: Free Look");
                break;
            case PlayerController.CameraMode.FromBehind:
                cameraMode.SetText("Camera Mode: Strafe Look");
                break;
            default:
                cameraMode.SetText("Unknown Camera Mode");
                break;
        }
        position.SetText("Position: " + player.transform.position);
        velocity.SetText("Velocity: {0:2} " + player.cc.velocity.normalized,
                         player.horizontalVelocity.magnitude + player.verticalVelocity);
        rotation.SetText("Rotation: " + player.transform.eulerAngles);
        isGrounded.SetText("Is Grounded: " + player.isGrounded);
        movementLocked.SetText("Movement Locked: " + player.lockMovement);
    }
}
