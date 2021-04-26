using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // Stores various Camera modes
    public enum CameraMode {
        FreeLook,
        FromBehind
    }

    // Stores state of a given button
    private enum ButtonState {
        Released,
        Held
    };

    // Globals
    private GlobalsController gc;

    [Header("Camera and Animation")]
    public Transform mainCamera;
    public CinemachineFreeLook freeLook;
    public CameraMode camMode;

    private float turnSmoothVelocity;

    public Animator anim;

    private List<CameraMode> camModes = new List<CameraMode>(){
        CameraMode.FreeLook,
        CameraMode.FromBehind
    };
    private int camModeIndex;
    [Space(5)]

    [Header("Player Input")]
    public CharacterController cc;

    public InputActions controls;
    public PlayerInput playerInput;

    private Vector2 stickInput;
    public float verticalVelocity;
    public Vector2 horizontalVelocity;
    [Space(5)]

    [Header("Jump, Falling Physics")]
    public Vector3 hitNormal;
    public float slideFriction;

    public bool isGrounded;
    public bool isJumping;

    public float jumpHeight;
    public float highJumpMultiplier;
    public float fallSpeed;
    public float jumpTimer;
    private bool triedJump;

    private float jumpElapsedTime;
    private ButtonState previousJumpButtonState;

    private bool heldJumpInAir;

    private float tempStepOffset;

    public bool CanJump {
        get
        {
            return isGrounded &&
                   previousJumpButtonState == ButtonState.Released;
        }
    }

    public bool CanContinueJump
    {
        get
        {
            return !isGrounded &&
                   heldJumpInAir &&
                   jumpElapsedTime <= jumpTimer;
        }
    }
    [Space(5)]

    [Header("Translation Physics")]
    public float walkSpeed;
    public float turnSmoothTime;
    public float rotateSpeed;

    public float pushPower;
    private float slowDownTime;
    [Space(5)]

    [Header("Gravitational Physics")]
    public List<Transform> groundChecks;
    public float groundSnapForce;
    public float groundSnapDistance = 1f;

    public bool ignoreCheckGround;
    public int ignoreCheckGroundFrames = 3;
    int ignoreCheckGroundTimer = 0;

    public float maxFallSpeed;
    public float groundDistance;

    public bool isLaunching;
    [Space(5)]

    [Header("NPC Interaction")]
    public bool isInteracting;
    private float interactDelaySeconds;
    public NPC interactingWith;

    // For locking movement during NPC chatter, cutscenes, etc.
    public bool lockMovement;
    [Space(5)]

    [Header("Audio")]
    public AudioSource playerAudioSource;
    public AudioClip footstepClip;
    public AudioClip defaultFootstepClip;
    [Space(5)]

    // Handles moving platforms
    private bool lastGroundInitialized;
    private Transform lastGroundTransform;
    private Vector3 lastGroundPosition;

    void Awake(){
        controls = new InputActions();

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
        playerInput = GetComponent<PlayerInput>();

        tempStepOffset = cc.stepOffset;
        verticalVelocity = 0f;

        interactDelaySeconds = 0f;

        camModeIndex = camModes.IndexOf(camMode);

        gc = GlobalsController.Instance;
        gc.player = this;

        //Cursor.lockState = CursorLockMode.Locked;
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
        if(ignoreCheckGround){
            ignoreCheckGroundTimer += 1;
        }

        stickInput = controls.Player.Move.ReadValue<Vector2>();
        triedJump = controls.Player.Jump.ReadValueAsObject() != null;

        if(!isGrounded || lockMovement || stickInput == Vector2.zero){
            anim.SetBool("isWalking", false);
        }

        hitNormal = new Vector3(0, 0, 0);

        VerticalMovement();
        HorizontalMovement();
        GroundMovement();
        CameraMovement();

        if(ignoreCheckGround){
            isGrounded = false;
            if(ignoreCheckGroundTimer > ignoreCheckGroundFrames){
                ignoreCheckGround = false;
            }
        } else {
            isGrounded = hitNormal.sqrMagnitude != 0 && Vector3.Angle(Vector3.up, hitNormal) <= cc.slopeLimit;
        }

        PlayerAudio();
    }

    void GroundMovement(){
        if(isGrounded){
            Transform groundTransform = getGround();

            if(groundTransform == null){
                return;
            }

            if(!ignoreCheckGround){
                isLaunching = false;
            }

            if(!lastGroundInitialized || groundTransform != lastGroundTransform){
                lastGroundTransform = groundTransform;
                lastGroundPosition = groundTransform.position;
                lastGroundInitialized = true;
                SetFootstepClip(groundTransform);
            } else if(footstepClip == null){
                SetFootstepClip(groundTransform);
            }

            Vector3 deltaGroundPosition = groundTransform.position - lastGroundPosition;
            lastGroundTransform = groundTransform;
            lastGroundPosition = groundTransform.position;

            transform.position = transform.position + deltaGroundPosition;
        } else {
            lastGroundInitialized = false;
        }
    }

    void SetFootstepClip(Transform groundTransform){
        try {
            footstepClip = groundTransform.parent.GetComponent<Ground>().footstepClip;
        } catch {
            footstepClip = defaultFootstepClip;
        }
    }

    void HorizontalMovement(){
        if(lockMovement){
            return;
        }

        float launchSpeedModifier = isLaunching ? Time.deltaTime : 1;

        if(!isLaunching){
            horizontalVelocity = Translate();

            Transform ground = getGround();
            if(ground != null && ground.gameObject.layer == CONSTANTS.SLOW_DOWN_LAYER){
                slowDownTime += Time.deltaTime;
                horizontalVelocity = horizontalVelocity * (1f / slowDownTime);
            } else {
                slowDownTime = 1f;
            }

            if(horizontalVelocity.magnitude > 0f){
                anim.SetBool("isWalking", true);
            }
        }

        Vector3 moveHorizontal = new Vector3(horizontalVelocity.x,
                                             0f,
                                             horizontalVelocity.y);

        moveHorizontal.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
        moveHorizontal.z += (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction);

        // Move character controller
        cc.Move(moveHorizontal * launchSpeedModifier);
    }

    void VerticalMovement(){
        if(!isGrounded || isLaunching)
        {
            anim.SetBool("isFalling", true);
            cc.stepOffset = 0f;
            footstepClip = null;
        }
        else
        {
            anim.SetBool("isFalling", false);
            cc.stepOffset = tempStepOffset;

            verticalVelocity = 0f;
            jumpElapsedTime = 0f;

            isJumping = false;
        }

        if(!ignoreCheckGround){
            Fall();
        }

        Jump(triedJump);

        if(verticalVelocity < maxFallSpeed)
        {
            verticalVelocity = maxFallSpeed;
        }

        // Ground snap stuff
        if(!ignoreCheckGround && horizontalVelocity.magnitude + verticalVelocity != 0 && onSlope()){
            verticalVelocity += cc.height / 2 * groundSnapForce * Time.deltaTime;
        }

        cc.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
    }

    void CameraMovement(){
        // Camera stuff
        if(camMode == CameraMode.FromBehind && !lockMovement)
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

    void PlayerAudio(){
        // Walking
        if(isGrounded && horizontalVelocity.magnitude > 0f && !playerAudioSource.isPlaying){
            playerAudioSource.clip = footstepClip;
            playerAudioSource.Play();
        }
    }

    bool checkGround(){
        int numGrounded = 0;
        foreach(Transform groundCheck in groundChecks)
        {
            numGrounded += Physics.CheckSphere(groundCheck.position,
                                               groundDistance,
                                               CONSTANTS.GROUND_MASK) ? 1 : 0;
        }
        return numGrounded > 1;
    }

    Transform getGround(){
        RaycastHit hit;
        if(Physics.Raycast(transform.position,
                           Vector3.down,
                           out hit,
                           cc.height / 2 * groundSnapDistance,
                           CONSTANTS.GROUND_MASK)){
            return hit.transform;
        }
        return null;
    }

    bool onSlope(){
        if(isJumping || isLaunching){
            return false;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position,
                           Vector3.down,
                           out hit,
                           cc.height / 2 * groundSnapDistance,
                           CONSTANTS.GROUND_MASK)){
            if(hit.normal != Vector3.up){
                return true;
            }
        }

        return false;
    }

    void Fall(){
        verticalVelocity += Physics.gravity.y * Time.deltaTime * fallSpeed;
    }

    void LedgeGrab(){
        RaycastHit objectHit;
        if(Physics.Raycast(transform.position,
                           transform.forward,
                           out objectHit,
                           7f)){
            Debug.Log("Ledge grab hit");
        }
    }

    Vector2 Translate(){
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

        Vector3 output = moveDirection.normalized * Time.deltaTime * walkSpeed;
        return new Vector2(output.x, output.z);
    }

    void Jump(bool jumpButtonPressed){
        if (jumpButtonPressed)
        {
            if(CanJump)
            {
                verticalVelocity += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

                isJumping = true;
                heldJumpInAir = true;
            }
            else if(CanContinueJump)
            {
                verticalVelocity += jumpHeight * highJumpMultiplier;
                isJumping = true;
            }
            jumpElapsedTime += Time.deltaTime;
        } else {
            heldJumpInAir = false;
        }
        previousJumpButtonState = jumpButtonPressed ? ButtonState.Held : ButtonState.Released;
    }

    public void Launch(Vector3 velocity){
        ignoreCheckGround = true;
        ignoreCheckGroundTimer = 0;
        isLaunching = true;

        horizontalVelocity = new Vector2(velocity.x, velocity.z);
        verticalVelocity = velocity.y;
    }

    void OnControllerColliderHit(ControllerColliderHit collision){
        hitNormal = collision.normal;
        if(!ignoreCheckGround){
            if(gc.layerInMask(collision.gameObject.layer, CONSTANTS.CEILING_LAYER) &&
               verticalVelocity > 0f){
                verticalVelocity = 0f;
            } else if(isLaunching && gc.layerInMask(collision.gameObject.layer, CONSTANTS.GROUND_MASK)){
                isLaunching = false;
            }
        }
    }

    void InvertXAxisOnControlChange(){
        bool invert = playerInput.currentControlScheme == "Gamepad";
        freeLook.m_XAxis.m_InvertInput = invert;
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
                if(objectHit.transform.gameObject.layer == CONSTANTS.NPC_LAYER
                   || objectHit.transform.gameObject.layer == CONSTANTS.INTERACT_LAYER)
                {
                    isInteracting = true;

                    interactingWith = objectHit.transform.root.gameObject.GetComponent<NPC>();
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
        gc.Quit();
    }
}
