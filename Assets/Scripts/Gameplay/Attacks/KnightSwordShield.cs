using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KnightSwordShield : MonoBehaviour
{
    public Transform shieldParent;
    public Transform swordParent;

    public GameObject[] shieldModels;
    public int shieldIndex = -1;

    public GameObject[] swordModels;
    public int swordIndex = -1;

    GameObject spawnedShield;
    GameObject spawnedSword;

    // Start is called before the first frame update
    void Start()
    {
        if(shieldIndex == -1 && shieldModels.Length > -1){
            shieldIndex = 0;
        }
        if(swordIndex == -1 && swordModels.Length > -1){
            swordIndex = 0;
        }

        SceneManager.sceneLoaded += OnSceneLoad;

        spawnSwordShield();
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode){
        spawnSwordShield();
    }

    void spawnSwordShield(){
        if(shieldParent != null){
            Destroy(spawnedShield);
            spawnedShield = (GameObject)Instantiate(shieldModels[shieldIndex], shieldParent.position, Quaternion.identity);
            spawnedShield.transform.rotation = shieldParent.rotation;
            spawnedShield.transform.parent = shieldParent;
        }
        if(swordParent != null){
            Destroy(spawnedSword);
            spawnedSword = (GameObject)Instantiate(swordModels[swordIndex], swordParent.position, Quaternion.identity);
            spawnedSword.transform.rotation = swordParent.rotation;
            spawnedSword.transform.localScale = swordParent.localScale;
            spawnedSword.transform.parent = swordParent;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetShield(int index){
        shieldIndex = index;
        spawnSwordShield();
    }

    public void SetSword(int index){
        swordIndex = index;
        spawnSwordShield();
    }
}
