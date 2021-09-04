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

    [Header("Camera")]
    public Transform mainCamera;
    public CinemachineFreeLook freeLook;
    public CameraMode camMode;

    private float turnSmoothVelocity;

    private List<CameraMode> camModes = new List<CameraMode>(){
        CameraMode.FreeLook,
        CameraMode.FromBehind
    };
    private int camModeIndex;
    [Space(5)]

    [Header("Character Stats")]
    public CharacterStats currentStats;
    public CharacterStats[] characterStats;

    int currentStatIndex;
    public float characterSwapWaitTime = 1f;

    int numDead;

    float characterSwapTimer;
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
    float hitAngle;

    public bool isGrounded;
    public bool isJumping;
    public bool isFalling;
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
                   jumpElapsedTime <= currentStats.jumpTimer;
        }
    }
    [Space(5)]

    [Header("Gravitational Physics")]
    public bool ignoreCheckGround;
    public int ignoreCheckGroundFrames = 3;
    int ignoreCheckGroundTimer = 0;

    public bool isLaunching;

    public bool justUnpausedGroundCheck;
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
    [Space(5)]

    // Handles moving platforms
    private bool lastGroundInitialized;
    private Transform lastGroundTransform;
    private Vector3 lastGroundPosition;

    //Slowdown platforms
    float slowDownTime;

    private ButtonState previousPauseButtonState;

    void Awake(){
        controls = new InputActions();

        controls.Player.Interact.performed += _ => Interact();
        controls.Player.CameraMode_Increment.performed += _ => IncrementCameraMode();
        controls.Player.CameraMode_FreeLook.performed += _ => SetCameraMode(0);
        controls.Player.CameraMode_FromBehind.performed += _ => SetCameraMode(1);

        controls.Player.Character_Increment.performed += _ => SetCharacter(currentStatIndex + 1);
        controls.Player.Character_Decrement.performed += _ => SetCharacter(currentStatIndex - 1);

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

        currentStatIndex = 0;
        currentStats = characterStats[0];
        DisableCharactersExcept(0);
        numDead = 0;

        camModeIndex = camModes.IndexOf(camMode);

        gc = GlobalsController.Instance;
        gc.player = this;
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    void OnEnable(){
        controls.Enable();
    }

    void OnDisable(){
        controls.Disable();
    }

    void Update()
    {
        if(controls.Player.Pause.ReadValue<float>() != 0f){
            if(previousPauseButtonState == ButtonState.Released){
                previousPauseButtonState = ButtonState.Held;
                gc.Pause();

                justUnpausedGroundCheck = true;
            }
        } else {
            previousPauseButtonState = ButtonState.Released;
        }

        interactDelaySeconds += Time.deltaTime;
        characterSwapTimer += Time.deltaTime;

        stickInput = controls.Player.Move.ReadValue<Vector2>();
        triedJump = controls.Player.Jump.ReadValueAsObject() != null;

        if(!isGrounded || lockMovement || stickInput == Vector2.zero){
            currentStats.anim.SetBool("isWalking", false);
        }

        hitNormal = new Vector3(0, 0, 0);
        hitAngle = 0f;

        VerticalMovement();
        HorizontalMovement();
        GroundMovement();
        CameraMovement();

        if(!gc.isPaused){
            if(ignoreCheckGround){
                ignoreCheckGroundTimer += 1;
                isGrounded = false;
                if(ignoreCheckGroundTimer > ignoreCheckGroundFrames){
                    ignoreCheckGround = false;
                }
            } else {
                isGrounded = justUnpausedGroundCheck || hitNormal.sqrMagnitude != 0 && hitAngle <= cc.slopeLimit;
                justUnpausedGroundCheck = false;
            }
        }

        PlayerAudio();
    }

    void DisableCharactersExcept(int exclude){
        for(int i = 0; i < characterStats.Length; i++){
            if(i == exclude){
                characterStats[i].gameObject.SetActive(true);
            } else {
                characterStats[i].gameObject.SetActive(false);
            }
        }
    }

    void SetCharacter(int newIndex, bool ignoreSwapTime=false){
        if(numDead == characterStats.Length){
            Debug.Log("Game Over!");
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }

        if(!ignoreSwapTime && characterSwapTimer < characterSwapWaitTime){
            return;
        }

        characterSwapTimer = 0f;

        if(newIndex < 0){
            newIndex = characterStats.Length - 1;
        } else if(newIndex >= characterStats.Length){
            newIndex = 0;
        }

        while(characterStats[newIndex].health == 0){
            newIndex++;
            if(newIndex < 0){
                newIndex = characterStats.Length - 1;
            } else if(newIndex >= characterStats.Length){
                newIndex = 0;
            }
        }

        currentStatIndex = newIndex;
        currentStats = characterStats[currentStatIndex];
        DisableCharactersExcept(currentStatIndex);

        // Fix footstep not changing bug
        Transform groundTransform = getGround();

        if(groundTransform == null){
            return;
        }

        currentStats.SetFootstepClip(groundTransform);

        gc.hudHandler.updateHealth(currentStats.healthPercentage);
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

                currentStats.SetFootstepClip(groundTransform);
            } else if(currentStats.footstepClip == null){
                currentStats.SetFootstepClip(groundTransform);
            }

            Vector3 deltaGroundPosition = groundTransform.position - lastGroundPosition;
            lastGroundTransform = groundTransform;
            lastGroundPosition = groundTransform.position;

            transform.position = transform.position + deltaGroundPosition;
        } else {
            lastGroundInitialized = false;
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
                currentStats.anim.SetBool("isWalking", true);
            }
        }

        Vector3 moveHorizontal = new Vector3(horizontalVelocity.x,
                                             0f,
                                             horizontalVelocity.y);

        if(hitAngle >= cc.slopeLimit){
            moveHorizontal.x = (1f - hitNormal.y) * hitNormal.x * (1f - currentStats.slideFriction);
            moveHorizontal.z = (1f - hitNormal.y) * hitNormal.z * (1f - currentStats.slideFriction);

            moveHorizontal = moveHorizontal.normalized / 2f;

            /*
            RaycastHit hit;
            if(Physics.Raycast(transform.position + moveHorizontal / 10f,
                               Vector3.down,
                               out hit,
                               cc.height / 2 * currentStats.groundSnapDistance,
                               CONSTANTS.GROUND_MASK)){
                if(hit.point.y < transform.position.y){
                    moveHorizontal.x = horizontalVelocity.x;
                    moveHorizontal.z = horizontalVelocity.y;

                    Debug.Log(transform.position + " " + hit.point);
                }
            }
            */
        }

        // Move character controller
        cc.Move(moveHorizontal * launchSpeedModifier);
    }

    void VerticalMovement(){
        if(!isGrounded || isLaunching)
        {
            currentStats.anim.SetBool("isFalling", true);
            cc.stepOffset = 0f;
            currentStats.footstepClip = null;
        }
        else
        {
            currentStats.anim.SetBool("isFalling", false);
            cc.stepOffset = tempStepOffset;

            verticalVelocity = 0f;
            jumpElapsedTime = 0f;

            isJumping = false;
        }

        if(!ignoreCheckGround){
            Fall();
        }

        Jump(triedJump);

        isFalling = isJumping && verticalVelocity < 0;

        if(verticalVelocity < currentStats.maxFallSpeed)
        {
            verticalVelocity = currentStats.maxFallSpeed;
        }

        // Ground snap stuff
        if(!ignoreCheckGround && !isJumping && horizontalVelocity.magnitude + verticalVelocity != 0 && onSlope()){
            verticalVelocity += cc.height / 2 * currentStats.groundSnapForce * Time.deltaTime;
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
                Mathf.LerpAngle(currentAngle.y, targetAngle.y, Time.deltaTime * currentStats.rotateSpeed),
                currentAngle.z
            );

            transform.eulerAngles = newAngle;
        }
    }

    void PlayerAudio(){
        // Walking
        if(isGrounded && !isInteracting && horizontalVelocity.magnitude > 0f && !playerAudioSource.isPlaying){
            playerAudioSource.clip = currentStats.footstepClip;
            playerAudioSource.Play();
        }
    }

    Transform getGround(){
        RaycastHit hit;
        if(Physics.Raycast(transform.position,
                           Vector3.down,
                           out hit,
                           cc.height / 2 * currentStats.groundSnapDistance,
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
                           cc.height / 2 * currentStats.groundSnapDistance,
                           CONSTANTS.GROUND_MASK)){
            if(hit.normal != Vector3.up){
                return true;
            }
        }

        return false;
    }

    void Fall(){
        verticalVelocity += Physics.gravity.y * Time.deltaTime * currentStats.fallSpeed;
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
            transform.RotateAround(transform.position, transform.up, currentStats.rotateSpeed);
        }

        if(moveDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) *
                                            Mathf.Rad2Deg;
            targetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                                targetAngle,
                                                ref turnSmoothVelocity,
                                                currentStats.turnSmoothTime);

            if(camMode == CameraMode.FromBehind)
            {
                // Strafe Mode
                targetAngle = transform.eulerAngles.y;
            }

            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        Vector3 output = moveDirection.normalized * Time.deltaTime * currentStats.walkSpeed;
        return new Vector2(output.x, output.z);
    }

    void Jump(bool jumpButtonPressed){
        if(!currentStats.canJump){
            return;
        }
        if (jumpButtonPressed)
        {
            if(CanJump)
            {
                verticalVelocity = Mathf.Sqrt(currentStats.jumpHeight * -2f * Physics.gravity.y * currentStats.fallSpeed);

                isJumping = true;
                heldJumpInAir = true;
            }
            else if(CanContinueJump)
            {
                verticalVelocity += Mathf.Sqrt(currentStats.jumpHeight * -2f * Physics.gravity.y * currentStats.fallSpeed) * currentStats.highJumpMultiplier;
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

    public void Hurt(int damage){
        updateHealth(damage);
    }

    void updateHealth(int damage){
        currentStats.health -= damage;

        gc.hudHandler.updateHealth(currentStats.healthPercentage);

        if(currentStats.health <= 0){
            numDead++;
            SetCharacter(currentStatIndex + 1, true);
            Debug.Log("Dead!");
        }
    }

    void OnControllerColliderHit(ControllerColliderHit collision){
        if(gc.layerInMask(collision.gameObject.layer, CONSTANTS.GROUND_MASK)){
            hitNormal = collision.normal;
            hitAngle = Vector3.Angle(Vector3.up, hitNormal);

            if(!ignoreCheckGround){
                if(gc.layerInMask(collision.gameObject.layer, CONSTANTS.CEILING_LAYER) &&
                   verticalVelocity > 0f){
                    verticalVelocity = 0f;
                } else if(isLaunching && gc.layerInMask(collision.gameObject.layer, CONSTANTS.GROUND_MASK)){
                    isLaunching = false;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == CONSTANTS.PICKUPS_LAYER){ // Pickups
            Pickup pickup = other.gameObject.GetComponent<Pickup>();
            gc.addPickups(pickup.value);
            pickup.Interact();
        } else if((isGrounded || isFalling) && other.gameObject.layer == CONSTANTS.HURTBOX_LAYER){ // Jumping on enemies
            other.transform.root.gameObject.GetComponent<Enemy>().Kill();

            jumpElapsedTime = 0f;
            verticalVelocity = Mathf.Sqrt(currentStats.jumpHeight * -2f * Physics.gravity.y * currentStats.fallSpeed);

            isJumping = true;
            heldJumpInAir = true;
            isGrounded = false;
        }
    }

    void OnControlChange(){
        /*
        X Axis inverted on gamepad
        X Axis max speed is 2x on gamepad
        */
        bool usingGamepad = playerInput.currentControlScheme == "Gamepad";
        if(usingGamepad){
            freeLook.m_XAxis.m_InvertInput = true;
            freeLook.m_XAxis.m_MaxSpeed = 220f;
        } else {
            freeLook.m_XAxis.m_InvertInput = false;
            freeLook.m_XAxis.m_MaxSpeed = 170f;
        }
    }

    void Interact(){
        if(isInteracting && interactingWith != null && interactingWith.isTalking){
            interactingWith.Next();
        }
        else if(!lockMovement && interactDelaySeconds > CONSTANTS.DIALOGUE_INPUT_DELAY)
        {
            interactDelaySeconds = 0f;

            RaycastHit objectHit;
            if(isGrounded && Physics.SphereCast(transform.position,
                                             7f,
                                             transform.forward,
                                             out objectHit,
                                             10f))
            {
                if(objectHit.transform.gameObject.layer == CONSTANTS.NPC_LAYER
                   || objectHit.transform.gameObject.layer == CONSTANTS.INTERACT_LAYER)
                {
                    isInteracting = true;

                    interactingWith = objectHit.transform.root.gameObject.GetComponent<NPC>();
                    interactingWith.Activate();

                    currentStats.anim.SetBool("isWalking", false);
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
}
