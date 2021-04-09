using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum CameraMode {
        FreeLook,
        FromBehind
    }

    private enum ButtonState {
        Released,
        Held
    };

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
    public float highJumpMultiplier;
    public float fallSpeed;
    public float jumpTimer;

    public bool CanJump {
        get
        {
            return isGrounded && previousJumpButtonState == ButtonState.Released;
        }
    }

    public bool CanContinueJump
    {
        get
        {
            return jumpElapsedTime <= jumpTimer && !isGrounded;
        }
    }

    public float maxFallSpeed;
    public float groundDistance;
    public LayerMask groundMask;

    public bool lockMovement;

    private float turnSmoothVelocity;
    private float tempStepOffset;

    private float interactDelaySeconds;

    private GlobalsController gc;

    private float jumpElapsedTime;
    private ButtonState previousJumpButtonState;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();

        tempStepOffset = cc.stepOffset;
        verticalVelocity = Vector3.zero;

        interactDelaySeconds = 0f;

        camModeIndex = camModes.IndexOf(camMode);
    }

    void OnEnable(){
        gc = GlobalsController.Instance;
        gc.player = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        interactDelaySeconds += Time.deltaTime;

        MiscInput();
        if(!lockMovement){
            Movement();
        }
    }

    void MiscInput(){
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown("c"))
        {
            camModeIndex += 1;
            if(camModeIndex >= camModes.Count)
            {
                camModeIndex = 0;
            }
            camMode = camModes[camModeIndex];
        }
        else if (!lockMovement && isInteracting())
        {
            RaycastHit objectHit;
            if(isGrounded && Physics.Raycast(transform.position,
                                             transform.forward,
                                             out objectHit,
                                             7f))
            {
                if(objectHit.transform.gameObject.layer == CONSTANTS.NPC_LAYER)
                {
                    NPC other = objectHit.transform.parent.gameObject.GetComponent<NPC>();
                    anim.SetBool("isWalking", false);
                    other.Activate();
                }
            }
        }
    }

    public bool isInteracting(){
        if(interactDelaySeconds > CONSTANTS.DIALOGUE_INPUT_DELAY)
        {
            if(Input.GetButtonDown("Interact"))
            {
                interactDelaySeconds = 0f;
                return true;
            }
            return false;
        }
        return false;
    }

    void Movement(){
        isGrounded = checkGround();

        if (isGrounded)
        {
            verticalVelocity.y = 0f;
            jumpElapsedTime = 0f;
        }

        horizontalVelocity = Translate();

        if(horizontalVelocity.magnitude != 0f && isGrounded)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        if(!isGrounded)
        {
            anim.SetBool("isFalling", true);
            cc.stepOffset = 0f;
            Fall();
        }
        else
        {
            anim.SetBool("isFalling", false);
            cc.stepOffset = tempStepOffset;
        }
        Jump();

        if(verticalVelocity.magnitude > maxFallSpeed)
        {
            verticalVelocity = verticalVelocity.normalized * maxFallSpeed;
        }

        cc.Move(horizontalVelocity + (verticalVelocity * Time.deltaTime));

        if(camMode == CameraMode.FromBehind)
        {
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

    bool checkGround(){
        int numGrounded = 0;
        foreach(Transform groundCheck in groundChecks)
        {
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

        if(vertical.magnitude > 0.1f && horizontal.magnitude > 0.1f)
        {
            moveDirection = Vector3.Slerp(vertical, horizontal, 0.5f);
        }
        else if(vertical.magnitude > 0.1f)
        {
            moveDirection = vertical;
        }
        else if(horizontal.magnitude > 0.1f)
        {
            moveDirection = horizontal;
        }
        moveDirection.y = 0f;

        if(camMode == CameraMode.FreeLook && horizontal.magnitude > 0.1f && moveDirection.magnitude < 0.1f)
        {
            transform.RotateAround(transform.position, transform.up, rotateSpeed);
        }

        if(moveDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) *
                                            Mathf.Rad2Deg;
            targetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                                targetAngle,
                                                ref turnSmoothVelocity,
                                                turnSmoothTime);

            if(camMode == CameraMode.FromBehind)
            {
                // Strafe Mode
                targetAngle = transform.eulerAngles.y;
            }

            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        return moveDirection.normalized * Time.deltaTime * walkSpeed;
    }

    void Jump(){
        bool jumpButtonPressed = Input.GetButton("Jump");
        if (jumpButtonPressed)
        {
            if(CanJump)
            {
                verticalVelocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
            else if(CanContinueJump)
            {
                verticalVelocity.y += jumpHeight * highJumpMultiplier;
            }
            jumpElapsedTime += Time.deltaTime;
        }
        previousJumpButtonState = jumpButtonPressed ? ButtonState.Held : ButtonState.Released;
    }
}
