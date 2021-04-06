using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public PlayerController player;

    public Text cameraMode;
    public Text position;
    public Text velocity;
    public Text isGrounded;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch(player.camMode){
            case PlayerController.CameraMode.FreeLook:
                cameraMode.text = "Camera Mode: Free Look";
                break;
            case PlayerController.CameraMode.FromBehind:
                cameraMode.text = "Camera Mode: Strafe Look";
                break;
            default:
                cameraMode.text = "Unknown Camera Mode";
                break;
        }
        position.text = "Position: " + player.transform.position.ToString();
        velocity.text = "Velocity: " +
                        (player.horizontalVelocity +
                            player.verticalVelocity).magnitude.ToString("F2") +
                        " "  +
                        player.cc.velocity.normalized.ToString();
        isGrounded.text = "Is Grounded: " + player.isGrounded;
    }
}
