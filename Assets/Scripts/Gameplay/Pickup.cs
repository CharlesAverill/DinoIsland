using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Pickup : MonoBehaviour
{

    public int value = 1;
    public float rotateSpeed = 1f;

    public float hoverMagnitude = 0.5f;
    public float hoverFrequency = 1f;

    public MeshRenderer meshRenderer;
    public Projector shadowProjector;
    public ParticleSystem pickupParticles;

    AudioSource audioSource;
    float initialHeight;

    bool canInteract = true;

    // Start is called before the first frame update
    void Start()
    {
        initialHeight = transform.position.y;
        audioSource = GetComponent<AudioSource>();

        if(meshRenderer == null){
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        if(shadowProjector == null){
            shadowProjector = GetComponentInChildren<Projector>();
        }

        pickupParticles.Stop();
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
            StartCoroutine(interactHelper());
            canInteract = false;
        }
    }

    IEnumerator interactHelper(){
        meshRenderer.enabled = false;
        shadowProjector.enabled = false;

        pickupParticles.Play();
        audioSource.Play();

        while(pickupParticles.isEmitting){
            yield return null;
        }

        Destroy(gameObject);
    }
}
