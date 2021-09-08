using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuantumTek.EncryptedSave;
using QuantumTek.QuantumDialogue;
using QuantumTek.QuantumUI;
using TMPro;
using RotaryHeart.Lib.SerializableDictionary;

public class GlobalsController : MonoBehaviour {

    private static GlobalsController _instance;

    public static GlobalsController Instance { get { return _instance; } }

    public PlayerController player;
    public Camera mainCamera;
    public AudioListener listener;
    public HUDHandler hudHandler;

    public AudioSource audioSource;

    public GameObject dialogueObject;
    public QUI_Bar dialogueBar;
    public TMP_Text dialogueText;
    public TMP_Text speakerName;
    public Image speakerImage;

    QD_DialogueHandler dialogueHandler;

    public GameObject pauseMenu;
    public bool isPaused;

    public bool loadingTheLoadingScreen;
    public bool loadingNextScene;

    public int currentPickups;

    public string sceneToLoad;

    public Dictionary<string, dynamic> saveData = new Dictionary<string, dynamic>(){
       {"SAVEDATA_initialized-save", true},
       {"SETTINGS_master-volume", 1f},
       {"PLAYER_total-pickups", 0}
    };

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

        // Call OnSceneLoaded when Scenes are loaded
        SceneManager.sceneLoaded += OnSceneLoaded;

        if(!ES_Save.Exists("SAVEDATA_initialized-save")){
            SaveData();
        } else {
            LoadSaveData();
        }
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
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Unpause(){
        AudioListener.pause = false;
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);

        player.justUnpausedGroundCheck = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        if(loadingNextScene){ // Successfully loaded new Scene
            loadingTheLoadingScreen = false;
            loadingNextScene = false;
            sceneToLoad = null;
        }
        else if(loadingTheLoadingScreen){ // Currently in Loading Scene
            loadingTheLoadingScreen = false;
            loadingNextScene = true;
            StartCoroutine(LoadSceneHelper());
        }

        // Collect and deactivate Dialogue objects if they exist
        try{
            dialogueObject = GameObject.FindWithTag("DialogueObject");
            dialogueBar = GameObject.FindWithTag("DialogueBar").GetComponent<QUI_Bar>();
            dialogueText = GameObject.FindWithTag("DialogueText").GetComponent<TMP_Text>();
            speakerName = GameObject.FindWithTag("SpeakerName").GetComponent<TMP_Text>();
            speakerImage = GameObject.FindWithTag("SpeakerIcon").GetComponent<Image>();

            dialogueObject.SetActive(false);
        } catch {
            Debug.Log("There are no Dialogue objects in this Scene");
        }

        dialogueHandler = null;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        listener = GameObject.FindObjectOfType<AudioListener>();
        AudioListener.volume = saveData["SETTINGS_master-volume"];

        try{
            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        } catch {
            Debug.Log("There is no Player object in this Scene");
        }

        try{
            hudHandler = GameObject.FindWithTag("HUD").GetComponent<HUDHandler>();
        } catch {
            Debug.Log("There is no HUD object in this Scene");
        }
    }

    public void LoadScene(string SceneName){
        loadingTheLoadingScreen = true;
        loadingNextScene = false;
        sceneToLoad = SceneName;
        SceneManager.LoadScene("Loading");
    }

    private IEnumerator LoadSceneHelper(){
        yield return new WaitForSeconds(1f); // We always want to see the cute Dino
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

    // Cache the dialogueHandler object
    public QuantumTek.QuantumDialogue.QD_DialogueHandler getDialogueHandler(){
        if(dialogueHandler == null){
            dialogueHandler = GameObject.FindWithTag("DialogueHandler").GetComponent<QD_DialogueHandler>();
        }
        return dialogueHandler;
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

        if(hudHandler != null){
            hudHandler.updatePickups(currentPickups);
        }
    }

    public void updateHealth(float newPercentage){
        if(hudHandler != null){
            hudHandler.updateHealth(newPercentage);
        }
    }
}
