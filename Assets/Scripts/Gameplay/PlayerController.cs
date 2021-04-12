using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInput controls;

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

    public CharacterController cc { get; private set; }

    Vector2 stickInput;
    public Vector3 verticalVelocity;
    public Vector3 horizontalVelocity;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isJumping;

    public float walkSpeed;
    public float turnSmoothTime;
    public float rotateSpeed;

    public float groundSnapForce;
    public float groundSnapDistance = 1f;

    public float pushPower;

    public float jumpHeight;
    public float highJumpMultiplier;
    public float fallSpeed;
    public float jumpTimer;
    bool triedJump;

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

    public bool isInteracting;
    private float interactDelaySeconds;
    public NPC interactingWith;

    private GlobalsController gc;

    private float jumpElapsedTime;
    private ButtonState previousJumpButtonState;

    void Awake(){
        controls = new PlayerInput();

        controls.Player.Pause.performed += _ => Pause();
        controls.Player.Interact.performed += _ => Interact();
        controls.Player.CameraMode_Increment.performed += _ => IncrementCameraMode();
        controls.Player.CameraMode_FreeLook.performed += _ => SetCameraMode(0);
        controls.Player.CameraMode_FromBehind.performed += _ => SetCameraMode(1);

        controls.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();

        tempStepOffset = cc.stepOffset;
        verticalVelocity = Vector3.zero;

        interactDelaySeconds = 0f;

        camModeIndex = camModes.IndexOf(camMode);

        gc = GlobalsController.Instance;
        gc.player = this;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void OnEnable(){
        controls.Enable();
    }

    void OnDisable(){
        controls.Disable();
    }

    void Update()
    {
        interactDelaySeconds += Time.deltaTime;

        stickInput = controls.Player.Move.ReadValue<Vector2>();
        triedJump = controls.Player.Jump.ReadValueAsObject() != null;

        if(stickInput != Vector2.zero){
            if(isGrounded){
                anim.SetBool("isWalking", true);
            }
            HorizontalMovement();
        } else {
            anim.SetBool("isWalking", false);
            horizontalVelocity = Vector2.zero;
        }

        VerticalMovement();

        CameraMovement();
    }

    void Interact(){
        if(isInteracting && interactingWith != null && interactingWith.isTalking){
            interactingWith.Next();
        }
        else if(!lockMovement && interactDelaySeconds > CONSTANTS.DIALOGUE_INPUT_DELAY)
        {
            interactDelaySeconds = 0f;

            RaycastHit objectHit;
            if(isGrounded && Physics.Raycast(transform.position,
                                             transform.forward,
                                             out objectHit,
                                             7f))
            {
                if(objectHit.transform.gameObject.layer == CONSTANTS.NPC_LAYER)
                {
                    isInteracting = true;

                    interactingWith = objectHit.transform.parent.gameObject.GetComponent<NPC>();
                    interactingWith.Activate();

                    anim.SetBool("isWalking", false);
                }
            }
        }
    }

    void SetCameraMode(int index){
        camModeIndex = index;
        camMode = camModes[camModeIndex];
    }

    void IncrementCameraMode(){
        camModeIndex += 1;
        if(camModeIndex >= camModes.Count)
        {
            camModeIndex = 0;
        }
        camMode = camModes[camModeIndex];
    }

    void Pause(){
        Debug.Log("Quitting application");
        Application.Quit();
    }

    void HorizontalMovement(){
        if(lockMovement){
            return;
        }

        horizontalVelocity = Translate();

        // Move character controller
        cc.Move(horizontalVelocity);
    }

    void VerticalMovement(){
        isGrounded = checkGround();

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

            verticalVelocity.y = 0f;
            jumpElapsedTime = 0f;

            isJumping = false;
        }
        
        Jump(triedJump);

        if(verticalVelocity.magnitude > maxFallSpeed)
        {
            verticalVelocity = verticalVelocity.normalized * maxFallSpeed;
        }

        // Ground snap stuff
        if((horizontalVelocity + verticalVelocity).magnitude != 0 && onSlope()){
            verticalVelocity.y += cc.height / 2 * groundSnapForce * Time.deltaTime;
        }

        cc.Move(verticalVelocity * Time.deltaTime);
    }

    void CameraMovement(){
        // Camera stuff
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

    bool onSlope(){
        if(isJumping){
            return false;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position,
                           Vector3.down,
                           out hit,
                           cc.height / 2 * groundSnapDistance,
                           groundMask)){
            if(hit.normal != Vector3.up){
                return true;
            }
        }

        return false;
    }

    void Fall(){
        verticalVelocity.y += Physics.gravity.y * Time.deltaTime * fallSpeed;
    }

    Vector3 Translate(){
        float hInput, vInput;
        Vector3 horizontal, vertical;

        // These values should be either -1f, 0f, or 1f
        hInput = 1f * Mathf.Abs(stickInput.x) / stickInput.x;
        vInput = 1f * Mathf.Abs(stickInput.y) / stickInput.y;

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

    void Jump(bool jumpButtonPressed){
        if (jumpButtonPressed)
        {
            Debug.Log(CanJump);
            if(CanJump)
            {
                verticalVelocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                isJumping = true;
            }
            else if(CanContinueJump)
            {
                verticalVelocity.y += jumpHeight * highJumpMultiplier;
                isJumping = true;
            }
            jumpElapsedTime += Time.deltaTime;
        }
        previousJumpButtonState = jumpButtonPressed ? ButtonState.Held : ButtonState.Released;
    }
}
