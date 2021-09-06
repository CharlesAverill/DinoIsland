using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    public enum PickupType {
        Fruit,
        Health
    }

    [Header("Stats")]
    public PickupType type;
    public int value = 1;

    bool canInteract = true;
    [Space(5)]

    [Header("Visual")]
    public float rotateSpeed = 1f;

    public float hoverMagnitude = 0.5f;
    public float hoverFrequency = 1f;
    float initialHeight;

    public MeshRenderer meshRenderer;
    public Projector shadowProjector;

    public bool useParticles;
#if UNITY_EDITOR
    [ConditionalHide("useParticles", true)]
#endif
    public ParticleSystem pickupParticles;
    [Space(5)]

    [Header("Audio")]
    public bool playSoundOnInteract;
#if UNITY_EDITOR
    [ConditionalHide("playSoundOnInteract", true)]
#endif
    public AudioSource audioSource;
    [Space(5)]

    GlobalsController gc;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;

        initialHeight = transform.position.y;

        if(useParticles){
            pickupParticles.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Time.deltaTime * 100f * rotateSpeed, 0);

        transform.position = new Vector3(transform.position.x,
                                         initialHeight +
                                            (Mathf.Sin(Time.time * hoverFrequency) * hoverMagnitude),
                                         transform.position.z);
    }

    public void Interact(){
        if(canInteract){
            switch(type){
                case PickupType.Fruit:
                    gc.addPickups(value);
                    break;
                case PickupType.Health:
                    if(gc.player.currentStats.health > 0){
                        gc.player.Heal(value);
                    }
                    break;
            }
            StartCoroutine(interactHelper());
            canInteract = false;
        }
    }

    IEnumerator interactHelper(){
        meshRenderer.enabled = false;
        shadowProjector.enabled = false;

        if(playSoundOnInteract){
            Debug.Log("Playing");
            audioSource.Play();
        }
        if(useParticles){
            pickupParticles.Play();
        }

        while((useParticles && pickupParticles.isEmitting) || (playSoundOnInteract && audioSource.isPlaying)){
            yield return null;
        }

        Destroy(gameObject);
    }
}
