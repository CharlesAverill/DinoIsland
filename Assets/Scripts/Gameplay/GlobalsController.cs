using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuantumTek.EncryptedSave;
using RotaryHeart.Lib.SerializableDictionary;

public class GlobalsController : MonoBehaviour {

    private static GlobalsController _instance;

    public static GlobalsController Instance { get { return _instance; } }

    public PlayerController player;
    public Camera mainCamera;
    public AudioListener listener;
    public AudioSource audioSource;

    public int currentPickups;

    public string sceneToLoad;

    public Dictionary<string, dynamic> saveData = new Dictionary<string, dynamic>(){
       {"SAVEDATA_initialized-save", true},
       {"SETTINGS_master-volume", 1f},
       {"SETTINGS_sensitivity-x", 220f},
       {"SETTINGS_sensitivity-y", 2f},
       {"SETTINGS_invert-x", true},
       {"SETTINGS_invert-y", false},
       {"PLAYER_total-pickups", 0}
    };

    UIController uic;

    private void Awake()
    {
        // Only want 1 GlobalsController instance per scene
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        // Persist between scenes
        DontDestroyOnLoad (transform.gameObject);

        if(!ES_Save.Exists("SAVEDATA_initialized-save")){
            SaveData();
        } else {
            LoadSaveData();
        }
    }

    void Start(){
        uic = UIController.Instance;
    }

    void SaveData(){
        foreach(KeyValuePair<string, dynamic> entry in saveData)
        {
            Debug.Log("Saving " + entry.Key + " as " + entry.Value);
            ES_Save.Save(entry.Value, entry.Key);
        }
    }

    void LoadSaveData(){
        saveData["SETTINGS_master-volume"] = ES_Save.Load<float>("SETTINGS_master-volume");
        saveData["SETTINGS_sensitivity-x"] = ES_Save.Load<float>("SETTINGS_sensitivity-x");
        saveData["SETTINGS_sensitivity-y"] = ES_Save.Load<float>("SETTINGS_sensitivity-y");
        saveData["SETTINGS_invert-x"] = ES_Save.Load<bool>("SETTINGS_invert-x");
        saveData["SETTINGS_invert-y"] = ES_Save.Load<bool>("SETTINGS_invert-y");
        saveData["PLAYER_total-pickups"] = ES_Save.Load<int>("PLAYER_total-pickups");
    }

    void DeleteSaveData(){
        foreach(KeyValuePair<string, dynamic> entry in saveData)
        {
            ES_Save.DeleteData(entry.Key);
        }
    }

    public void Quit(){
        SaveData();
#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void Pause(){
        if(Time.timeScale != 1f){
            Unpause();
            return;
        }

        AudioListener.pause = true;
        uic.isPaused = true;
        Time.timeScale = 0f;
        uic.pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Unpause(){
        AudioListener.pause = false;
        uic.isPaused = false;
        Time.timeScale = 1f;
        uic.pauseMenu.SetActive(false);

        player.justUnpausedGroundCheck = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void FindAll(){
        uic = UIController.Instance;

        mainCamera = Camera.main;

        listener = GameObject.FindObjectOfType<AudioListener>();
        AudioListener.volume = saveData["SETTINGS_master-volume"];

        try{
            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            player.invertX = saveData["SETTINGS_invert-x"];
            player.invertY = saveData["SETTINGS_invert-y"];
            player.sensitivityX = saveData["SETTINGS_sensitivity-x"];
            player.sensitivityY = saveData["SETTINGS_sensitivity-y"];
        } catch {
            Debug.Log("There is no Player object in this Scene");
        }
    }

    public void LoadingScreenToScene(string sceneName){
        LevelManager.Instance.LoadScene(sceneName, showLoadingScreen: true);
    }

    public bool layerInMask(int layer, int mask){
        return mask == (mask | (1 << layer));
    }

    public IEnumerator PlayReverseAudio(){
        audioSource.time = audioSource.clip.length - 0.01f;
        audioSource.pitch = -1;

        audioSource.Play();

        while(audioSource.isPlaying){
            yield return null;
        }

        audioSource.time = 0f;
        audioSource.pitch = 1;
    }

    public void addPickups(int deltaPickups){
        currentPickups += deltaPickups;
        saveData["PLAYER_total-pickups"] = saveData["PLAYER_total-pickups"] + deltaPickups;

        if(uic.hudHandler != null){
            uic.hudHandler.updatePickups(currentPickups);
        }
    }

    public void updateHealth(float newPercentage){
        if(uic.hudHandler != null){
            uic.hudHandler.updateHealth(newPercentage);
        }
    }
}
