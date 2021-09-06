using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Slideshow : MonoBehaviour
{

    public bool isAnimating;
    public bool loop;
    public bool playBackwards;

    public float timeBetweenFrames;

    public Sprite[] images;

    Image target;
    int imageIndex;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        target = GetComponent<Image>();
        timer = timeBetweenFrames;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(isAnimating && timer >= timeBetweenFrames){
            imageIndex += playBackwards ? -1 : 1;

            if(!loop){
                isAnimating = false;
            }
            if(imageIndex >= images.Length){
                imageIndex = 0;
            } else if(imageIndex < 0){
                imageIndex = images.Length - 1;
            } else {
                isAnimating = true;
            }

            target.sprite = images[imageIndex];

            timer = 0f;
        }
    }

    public void Begin(bool fromBeginning=false){
        isAnimating = true;
        if(fromBeginning){
            imageIndex = playBackwards ? images.Length - 1 : 0;
        }
    }

    public void End(){
        isAnimating = false;
    }
}
