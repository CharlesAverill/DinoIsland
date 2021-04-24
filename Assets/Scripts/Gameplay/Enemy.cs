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
    public bool isAttacking;
    public float distanceToTarget;
    Transform target;
    NavMeshAgent agent;
    [Space(5)]

    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        target = GlobalsController.Instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isAttacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            isAttacking = false;
        }

        MoveAnimate();
    }

    void MoveAnimate(){
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        if(distanceToTarget <= lookRadius){
            agent.SetDestination(target.position);

            FaceTarget();

            if(distanceToTarget > agent.stoppingDistance){ // Walk towards target
                anim.SetBool("isWalking", true);
            }
            else if(!isAttacking && distanceToTarget <= agent.stoppingDistance + .2f){ // Attack if close enough
                agent.SetDestination(transform.position);

                if(launchTarget){
                    launchVector = (target.position - transform.position).normalized * launchSpeed;
                    launchVector.y = Mathf.Max(launchVector.x, launchVector.z);
                    GlobalsController.Instance.player.Launch(launchVector);
                }

                anim.SetTrigger("Attack");
                isAttacking = true;
            }
        } else { // Idle
            agent.SetDestination(transform.position);
            anim.SetBool("isWalking", false);
        }
    }

    void FaceTarget ()
	{
		Vector3 direction = (target.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
	}

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + launchVector * launchSpeed);
    }
}
