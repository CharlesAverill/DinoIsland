using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum CameraMode {
        FreeLook,
        FromBehind
    }

    List<CameraMode> camModes = new List<CameraMode>(){
        CameraMode.FreeLook,
        CameraMode.FromBehind
    };
    int camModeIndex;

    public Transform mainCamera;
    public CameraMode camMode;
    public List<Transform> groundChecks;
    public Animator anim;

    [HideInInspector]
    public CharacterController cc{
        get; private set;
    }

    [HideInInspector]
    public Vector3 verticalVelocity;
    [HideInInspector]
    public Vector3 horizontalVelocity;

    [HideInInspector]
    public bool isGrounded;

    public float walkSpeed;
    public float turnSmoothTime;
    public float rotateSpeed;

    public float pushPower;

    public float jumpHeight;
    public float fallSpeed;
    public float maxFallSpeed;
    public float groundDistance;
    public LayerMask groundMask;

    float turnSmoothVelocity;
    float tempStepOffset;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        tempStepOffset = cc.stepOffset;
        verticalVelocity = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
        camModeIndex = camModes.IndexOf(camMode);
    }

    void Update()
    {

        KeyboardInput();

        isGrounded = checkGround();

        if (isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = 0f;
        }

        horizontalVelocity = Translate();

        if(horizontalVelocity.magnitude != 0f && isGrounded){
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }

        if(!isGrounded){
            anim.SetBool("isFalling", true);
            cc.stepOffset = 0f;
            //horizontalVelocity *= 0.5f;
            Fall();
        } else {
            anim.SetBool("isFalling", false);
            cc.stepOffset = tempStepOffset;
        }
        Jump();

        if(verticalVelocity.magnitude > maxFallSpeed){
            verticalVelocity = verticalVelocity.normalized * maxFallSpeed;
        }

        cc.Move(horizontalVelocity + (verticalVelocity * Time.deltaTime));

        if(camMode == CameraMode.FromBehind){
            Vector3 currentAngle = transform.eulerAngles;
            Vector3 targetAngle = mainCamera.transform.eulerAngles;

            Vector3 newAngle = new Vector3(
                currentAngle.x,
                Mathf.LerpAngle(currentAngle.y, targetAngle.y, Time.deltaTime * rotateSpeed),
                currentAngle.z
            );

            transform.eulerAngles = newAngle;
        }
    }

    void KeyboardInput(){
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown("c")){
            camModeIndex += 1;
            if(camModeIndex >= camModes.Count){
                camModeIndex = 0;
            }
            camMode = camModes[camModeIndex];
        }
    }

    bool checkGround(){
        int numGrounded = 0;
        foreach(Transform groundCheck in groundChecks){
            numGrounded += Physics.CheckSphere(groundCheck.position,
                                          groundDistance,
                                          groundMask) ? 1 : 0;
        }
        return numGrounded > 1;
    }

    void Fall(){
        verticalVelocity.y += Physics.gravity.y * Time.deltaTime * fallSpeed;
    }

    Vector3 Translate(){
        float hInput, vInput;
        Vector3 horizontal, vertical;

        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        horizontal = mainCamera.right * hInput;
        vertical = mainCamera.forward * vInput;

        Vector3 moveDirection = Vector3.zero;

        if(vertical.magnitude > 0.1f && horizontal.magnitude > 0.1f){
            moveDirection = Vector3.Slerp(vertical, horizontal, 0.5f);
        } else if(vertical.magnitude > 0.1f){
            moveDirection = vertical;
        } else if(horizontal.magnitude > 0.1f){
            moveDirection = horizontal;
        }
        moveDirection.y = 0f;

        if(camMode == CameraMode.FreeLook && horizontal.magnitude > 0.1f && moveDirection.magnitude < 0.1f){
            transform.RotateAround(transform.position, transform.up, rotateSpeed);
        }

        if(moveDirection.magnitude > 0.1f){
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) *
                                            Mathf.Rad2Deg;
            targetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                                targetAngle,
                                                ref turnSmoothVelocity,
                                                turnSmoothTime);

            if(camMode == CameraMode.FromBehind){
                // Strafe Mode
                targetAngle = transform.eulerAngles.y;
            }

            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        return moveDirection.normalized * Time.deltaTime * walkSpeed;
    }

    void Jump(){
        if (Input.GetButtonDown("Jump") && isGrounded){
            verticalVelocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        }
    }
}
