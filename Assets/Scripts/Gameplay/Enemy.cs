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
    [Space(5)]

    [Header("Enemy AI")]
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
        MoveTowardsTarget();
        Animate();
    }

    void MoveTowardsTarget(){
        float distanceToTarget = Vector3.Distance(target.position, transform.position);

        if(distanceToTarget <= lookRadius){
            agent.SetDestination(target.position);
        } else {
            agent.SetDestination(transform.position);
        }
    }

    void Animate(){
        if(agent.velocity.magnitude > agent.speed * .1f){
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
