using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveColorBackground : MonoBehaviour
{

    public RenderTexture outputRenderTexture;

    public Color fromColor;
    public Color toColor;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Texture2D currentTexture = getTexture();

        var pixels = currentTexture.GetPixels();

        for(int i = 0; i < pixels.Length; i++){
            if(pixels[i].maxColorComponent < fromColor.maxColorComponent){
                pixels[i] = toColor;
            }
            /*
            if(pixels[i] == fromColor){
                pixels[i] = toColor;
            } else {
                Debug.Log(pixels[i]);
            }
            */
        }

        currentTexture.SetPixels(pixels);

        Graphics.Blit(currentTexture, outputRenderTexture);
    }

    Texture2D getTexture()
    {
        Texture2D tex = new Texture2D(outputRenderTexture.width, outputRenderTexture.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = outputRenderTexture;
        tex.ReadPixels(new Rect(0, 0, outputRenderTexture.width, outputRenderTexture.height), 0, 0);
        tex.Apply();
        return tex;
    }
}
