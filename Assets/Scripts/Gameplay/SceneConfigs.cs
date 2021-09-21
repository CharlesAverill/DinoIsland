using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneConfigs : MonoBehaviour
{
    static SceneConfigs _instance;
    public static SceneConfigs Instance { get { return _instance; } }

    [Header("Skybox")]
    public bool useCustomSkybox;
#if UNITY_EDITOR
    [ConditionalHide("useCustomSkybox", true)]
#endif
    public Material skyboxMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadConfigs(){
        Debug.Log("Loading");
        UpdateSkybox();
    }

    void UpdateSkybox(){
        GameObject cameraObj = Camera.main.gameObject;
        Skybox box = cameraObj.GetComponent<Skybox>();
        if(useCustomSkybox){
            if(box != null){
                box.material = skyboxMaterial;
            }
        } else {
            box.enabled = false;
        }
    }
}
