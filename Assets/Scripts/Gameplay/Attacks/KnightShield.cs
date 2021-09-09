using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KnightShield : MonoBehaviour
{
    public Transform spawnParent;

    public GameObject[] shieldModels;
    public int prefabIndex = -1;

    GameObject spawnedShield;

    // Start is called before the first frame update
    void Start()
    {
        if(prefabIndex == -1 && shieldModels.Length > -1){
            prefabIndex = 0;
        }

        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        spawnShield();
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode){
        spawnShield();
    }

    void OnSceneUnloaded(Scene current){
        spawnedShield = null;
    }

    void spawnShield(){
        Destroy(spawnedShield);
        if(spawnParent != null){
            spawnedShield = (GameObject)Instantiate(shieldModels[prefabIndex], spawnParent.position, Quaternion.identity);
            spawnedShield.transform.rotation = spawnParent.rotation;
            spawnedShield.transform.parent = spawnParent;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetShield(int index){
        prefabIndex = index;
    }
}
