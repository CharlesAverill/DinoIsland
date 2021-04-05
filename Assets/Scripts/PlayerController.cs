using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform mainCamera;
    public List<Transform> groundChecks;
    public Animator anim;

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
    bool isGrounded;
    Vector3 verticalVelocity;

    CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        tempStepOffset = cc.stepOffset;
        verticalVelocity = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        isGrounded = checkGround();

        if (isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = 0f;
        }

        Vector3 translate = Translate();
        cc.Move(Translate());
        if(translate.magnitude != 0f && isGrounded){
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }

        if(!isGrounded){
            anim.SetBool("isFalling", true);
            cc.stepOffset = 0f;
            Fall();
        } else {
            anim.SetBool("isFalling", false);
            cc.stepOffset = tempStepOffset;
        }
        Jump();

        if(verticalVelocity.magnitude > maxFallSpeed){
            verticalVelocity = verticalVelocity.normalized * maxFallSpeed;
        }
        Debug.Log(verticalVelocity.magnitude);

        cc.Move(verticalVelocity * Time.deltaTime);
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
        Vector3 horizontal = mainCamera.right *
                             Input.GetAxisRaw("Horizontal");
        Vector3 vertical = mainCamera.forward *
                           Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = Vector3.zero;

        if(vertical.magnitude > 0.1f && horizontal.magnitude > 0.1f){
            moveDirection = Vector3.Slerp(vertical, horizontal, 0.5f);
        }
        else if(vertical.magnitude > 0.1f){
            moveDirection = vertical;
        }
        else if(horizontal.magnitude > 0.1f){
            moveDirection = horizontal;
        }
        moveDirection.y = 0f;

        if(horizontal.magnitude > 0.1f && moveDirection.magnitude < 0.1f){
            transform.RotateAround(transform.position, transform.up, rotateSpeed);
        }

        if(moveDirection.magnitude > 0.1f){
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) *
                                            Mathf.Rad2Deg;
            targetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                                targetAngle,
                                                ref turnSmoothVelocity,
                                                turnSmoothTime);
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
