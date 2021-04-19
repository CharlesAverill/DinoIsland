using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuantumTek.QuantumDialogue;
using QuantumTek.QuantumUI;
using TMPro;
using RotaryHeart.Lib.SerializableDictionary;

public class GlobalsController : MonoBehaviour {

    private static GlobalsController _instance;

    public static GlobalsController Instance { get { return _instance; } }

    public PlayerController player;
    public Camera mainCamera;

    public AudioSource audioSource;

    public GameObject dialogueObject;
    public QUI_Bar dialogueBar;
    public TMP_Text dialogueText;
    public TMP_Text speakerName;
    public Image speakerImage;

    QD_DialogueHandler dialogueHandler;

    public bool loadingTheLoadingScreen;
    public bool loadingNextScene;

    public string sceneToLoad;

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

        //Screen.SetResolution(640, 480, false);
    }

    public void Quit(){
        Application.Quit();
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

}
