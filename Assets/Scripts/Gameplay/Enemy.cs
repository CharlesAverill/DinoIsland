using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Information")]
    public int health;
    public float lookRadius;

    public bool launchTarget;
    public float launchSpeed;
    public Vector3 launchVector;
    [Space(5)]

    [Header("Enemy AI")]
    public float attackWait;
    public float attackWaitTimer;
    public bool isAttacking;
    public float distanceToTarget;
    Transform target;
    NavMeshAgent agent;
    [Space(5)]

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip attackClip;
    [Space(5)]

    Animator anim;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        target = GlobalsController.Instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        attackWaitTimer = attackWait;

        agent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        attackWaitTimer += Time.deltaTime;

        if(isAttacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            isAttacking = false;
        }

        if(agent.enabled){
            MoveAnimate();
        }
    }

    void MoveAnimate(){
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        if(distanceToTarget <= lookRadius && attackWaitTimer > attackWait){
            agent.SetDestination(target.position);

            FaceTarget();

            if(distanceToTarget > agent.stoppingDistance + 2f){
                Walk();
            }
            else if(!isAttacking && distanceToTarget <= agent.stoppingDistance + 2f && attackWaitTimer > attackWait){
                Attack();
            }
        } else {
            Idle();
        }
    }

    void Walk(){
        anim.SetBool("isWalking", true);

        if(audioSource.clip != moveClip && !audioSource.isPlaying){
            audioSource.Stop();
            audioSource.clip = moveClip;
            audioSource.Play();
        } else if(!audioSource.isPlaying){
            audioSource.Play();
        }
    }

    void Attack(){
        if(launchTarget){
            launchVector = transform.TransformVector(Vector3.forward * launchSpeed);
            launchVector.y = Mathf.Max(Mathf.Abs(launchVector.x),
                                       Mathf.Abs(launchVector.z)) * 2f;

            GlobalsController.Instance.player.Launch(launchVector);
        }

        audioSource.Stop();
        audioSource.clip = attackClip;
        audioSource.Play();

        anim.SetTrigger("Attack");
        isAttacking = true;
        attackWaitTimer = 0f;
    }

    void Idle(){
        agent.SetDestination(transform.position);
        anim.SetBool("isWalking", false);
    }

    void FaceTarget ()
	{
		Vector3 direction = (target.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
	}

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + launchVector);
        if(target != null){
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, target.position + target.InverseTransformVector(launchVector));
        }
    }

    void OnCollisionEnter(Collision other){
        Debug.Log(GlobalsController.Instance.layerInMask(other.gameObject.layer, CONSTANTS.GROUND_LAYER));
        if(agent.enabled == false && GlobalsController.Instance.layerInMask(other.gameObject.layer, CONSTANTS.GROUND_MASK)){
            rb.isKinematic = true;
            rb.useGravity = false;
            agent.enabled = true;
        }
    }
}
