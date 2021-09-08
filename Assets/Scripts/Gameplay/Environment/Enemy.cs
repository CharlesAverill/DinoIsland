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

    public bool wander;
    public float wanderDistance;
    public float wanderTimer;

    public int damage;

    public bool launchTarget;
    public float launchSpeed;
    public Vector3 launchVector;

    public GameObject hurtBox;

    public float deathRotateSpeed = 5f;
    bool isDying;
    [Space(5)]

    [Header("Enemy AI")]
    public float attackWait;
    public float attackWaitTimer;

    public bool isAttacking;

    public float distanceToTarget;

    public bool playerInRange;

    Transform target;
    NavMeshAgent agent;
    [Space(5)]

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip attackClip;
    public AudioClip deathClip;
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
        hurtBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        attackWaitTimer += Time.deltaTime;
        wanderTimer += Time.deltaTime;

        if(isAttacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            isAttacking = false;
        }

        if(agent.enabled){
            MoveAnimate();
        }
    }

    public void Kill(){
        if(!isDying){
            agent.enabled = false;
            StartCoroutine(killHelper());
        }
        isDying = true;
    }

    IEnumerator killHelper(){
        audioSource.Stop();
        audioSource.clip = deathClip;
        audioSource.Play();

        hurtBox.SetActive(false);

        try{
            GetComponent<Collider>().enabled = false;
        } catch {
            Debug.Log("Killed enemy without root-level collider");
        }

        while(audioSource.isPlaying){
            transform.Rotate(0, Time.deltaTime * 100f * deathRotateSpeed, 0);
            transform.localScale -= transform.localScale / 30f;
            yield return null;
        }

        Destroy(gameObject);
    }

    void MoveAnimate(){
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        if(distanceToTarget <= lookRadius){
            if(attackWaitTimer > attackWait){
                agent.SetDestination(target.position);

                FaceTarget();

                if(distanceToTarget > agent.stoppingDistance + 2f){
                    Walk();
                }
                else if(!isAttacking && playerInRange){
                    Attack();
                }
            }
        } else {
            if(wander){
                if(wanderTimer > 3f && agent.remainingDistance < 4f){
                    agent.SetDestination(getRandomNavMeshPosition());
                    Walk();
                } else if(agent.remainingDistance >= 4.2f){
                    wanderTimer = 0f;
                    Walk();
                } else {
                    Idle();
                }
            } else {
                Idle();
            }
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
            GlobalsController.Instance.player.Hurt(damage);
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
        if(!agent.enabled && GlobalsController.Instance.layerInMask(other.gameObject.layer, CONSTANTS.GROUND_MASK)){
            rb.isKinematic = true;
            rb.useGravity = false;

            agent.enabled = true;
            hurtBox.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player"){
            playerInRange = false;
        }
    }

    Vector3 getRandomNavMeshPosition(){
        Vector3 randomDirection = Random.insideUnitSphere * wanderDistance;
        randomDirection += transform.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, wanderDistance, NavMesh.AllAreas);

        return navHit.position;
    }
}
