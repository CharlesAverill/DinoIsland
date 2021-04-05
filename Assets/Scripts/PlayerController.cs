using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform mainCamera;
    public List<Transform> groundChecks;
    public Animator anim;

    public float walkSpeed;
    public float jumpHeight;
    public float fallSpeed;
    public float turnSmoothTime;
    public float rotateSpeed;
    public float groundDistance;
    public LayerMask groundMask;

    float turnSmoothVelocity;
    bool isGrounded;
    bool isWalking;
    Vector3 verticalVelocity;

    CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        verticalVelocity = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = checkGround();

        if (isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = 0f;
        }

        Vector3 translate = Translate();
        cc.Move(Translate());
        if(translate.magnitude != 0f && isGrounded){
            anim.SetBool("isWalking", true);
            isWalking = true;
        } else {
            anim.SetBool("isWalking", false);
            isWalking = false;
        }

        if(!isGrounded){
            Debug.Log("Falling");
            Fall();
        }
        Jump();

        cc.Move(verticalVelocity * Time.deltaTime);
    }

    bool checkGround(){
        bool output = false;
        foreach(Transform groundCheck in groundChecks){
            output = output || Physics.CheckSphere(groundCheck.position,
                                                   groundDistance,
                                                   groundMask);
        }
        return output;
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
            isWalking = false;
        }
    }
}
