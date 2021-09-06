using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Health and Damage")]
    int _health;
    public int health {
        get {
            return _health;
        }
        set {
            _health = Mathf.Min(value, maxHealth);
        }
    }
    public int maxHealth;
    public float healthPercentage {
        get {
            return (float)(health) / (float)maxHealth;
        }
        set {
            return;
        }
    }
    public float damage;
    [Space(5)]

    [Header("Jump, Falling Physics")]
    public bool canJump;

    public float slideFriction;

    public float jumpHeight;
    public float highJumpMultiplier;
    public float fallSpeed;
    public float jumpTimer;
    [Space(5)]

    [Header("Translation Physics")]
    public float walkSpeed;
    public float turnSmoothTime;
    public float rotateSpeed;

    public float pushPower;
    [Space(5)]

    [Header("Gravitational Physics")]
    public float groundSnapForce;
    public float groundSnapDistance = 1f;

    public float maxFallSpeed = Single.NegativeInfinity;
    [Space(5)]

    [Header("Audio")]
    public AudioClip footstepClip;
    public AudioClip defaultFootstepClip;
    [Space(5)]

    [Header("Animation")]
    public Animator anim;

    public void Start(){
        health = maxHealth;
    }

    public void SetFootstepClip(Transform groundTransform){
        try {
            footstepClip = groundTransform.parent.GetComponent<Ground>().footstepClip;
        } catch {
            footstepClip = defaultFootstepClip;
        }
    }
}
