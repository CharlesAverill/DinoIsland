using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance { get { return _instance; } }

    GlobalsController gc;
    UIController uic;

    [Header("Level Management")]
    public GameObject playerPrefab;

    public string currentSceneName;

    public Dictionary<string, AsyncOperation> additiveScenes;
    public List<string> toUnloadScenes;

    public bool loadingScene;

    public string singleSceneLoading;

    public string spawnPointName;
    [Space(5)]

    [Header("Loading Screen")]
    public bool usingLoadingScreen;
    public bool loadingTheLoadingScreen;
    public bool loadingNextScene;
    [Space(5)]

    [Header("Screen Wipe")]
    public ScreenTransitionImageEffect screenWipeController;

    public Color maskColor = Color.black;

    public Texture2D[] maskTextures;
    public int maskTextureOverrideIndex = -1;
    public bool maskInvert;

    AudioSource bgMusic;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;
        uic = UIController.Instance;

        additiveScenes = new Dictionary<string, AsyncOperation>();
    }

    void Awake(){
        // Only want 1 LevelManager instance per scene
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
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(!loadingScene && toUnloadScenes.Count > 0){
            unloadScenes();
        }
        */
    }

    public void LoadScene(string sceneName, string _spawnPointName="", bool showLoadingScreen=false, bool additive=false, bool unloadCurrentScene=true, bool screenWipe=true, bool invertScreenWipe=false){
        if(showLoadingScreen){
            loadingTheLoadingScreen = true;
            loadingNextScene = false;
            usingLoadingScreen = true;

            singleSceneLoading = sceneName;
            SceneManager.LoadScene("Loading");

            return;
        }

        if(additive){
            if(singleSceneLoading != ""){
                SceneManager.UnloadSceneAsync(singleSceneLoading);
            }
            if(SceneManager.sceneCount > 1 && SceneManager.GetSceneByName(sceneName) != null){
                return;
            }
            if(additiveScenes.ContainsKey(sceneName)){
                return;
            }
            // Start loading and prevent activation
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); //, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            // Add to dictionary
            additiveScenes.Add(sceneName, op);

            singleSceneLoading = sceneName;

            return;
        }

        //loadingScene = true;

        if(unloadCurrentScene){
            toUnloadScenes.Add(SceneManager.GetActiveScene().name);
        }

        spawnPointName = _spawnPointName;
        maskInvert = invertScreenWipe;

        StartCoroutine(loadSceneEnumerator(sceneName, additive, screenWipe));
    }

    public void ActivateAdditiveScene(string sceneName, string _spawnPointName, bool doScreenWipe=true){
        spawnPointName = _spawnPointName;
        singleSceneLoading = "";
        StartCoroutine(loadSceneEnumerator(sceneName, true, doScreenWipe));
    }

    IEnumerator loadSceneEnumerator(string sceneName, bool additive, bool doScreenWipe){
        AsyncOperation asyncLoadOperation;
        if(additive){
            asyncLoadOperation = additiveScenes[sceneName];
            additiveScenes.Remove(sceneName);
            //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        } else {
            // Load scene, prevent from immediately activating
            asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncLoadOperation.allowSceneActivation = false;
        }

        gc.enemyFreeze = true;

        // Check for screen wipe
        if(doScreenWipe && maskTextures.Length > 0){
            screenWipeController = Camera.main.GetComponent<ScreenTransitionImageEffect>();
            screenWipeController.maskValue = 0f;
            StartCoroutine(screenWipeEnumerator());
        }

        // Wait until scene is loaded
        while(screenWipeController.maskValue != 1f || asyncLoadOperation.progress < 0.9f){
            yield return null;
        }

        // Allow activation
        asyncLoadOperation.allowSceneActivation = true;
        yield return null;
    }

    IEnumerator screenWipeEnumerator(){
        screenWipeController.enabled = true;
        // Apply screen wipe settings
        screenWipeController.maskColor = maskColor;

        // Pick random screen wipe mask
        Texture2D maskTexture;
        if(maskTextureOverrideIndex == -1){
            maskTexture = maskTextures[UnityEngine.Random.Range(0, maskTextures.Length - 1)];
        } else {
            maskTexture = maskTextures[maskTextureOverrideIndex];
        }
        screenWipeController.maskTexture = maskTexture;

        // Screen Wipe
        screenWipeController.maskValue = maskInvert ? 1f : 0f;
        float maxVolume = bgMusic.volume;

        float step = 1f / 64f;
        if(maskInvert){
            bgMusic.volume /= 2f;
            yield return new WaitForSeconds(.25f);
            for(int i = 0; i < 64; i++){
                screenWipeController.maskValue = Mathf.Lerp(1, 0, step * i);
                if(bgMusic != null){
                    bgMusic.volume = Mathf.Lerp(maxVolume / 2f, maxVolume, step * i);
                }
                yield return new WaitForSeconds(step / 2f);
            }
        } else {
            for(int i = 0; i < 64; i++){
                screenWipeController.maskValue = Mathf.Lerp(0, 1, step * i);
                bgMusic.volume = Mathf.Lerp(maxVolume, maxVolume / 2f, step * i);
                yield return new WaitForSeconds(step / 2f);
            }
        }
        screenWipeController.maskValue = maskInvert ? 0f : 1f;
        //bgMusic.volume = maskInvert ? maxVolume : maxVolume / 2f;

        maskInvert = false;
        screenWipeController.enabled = false;
        gc.player.lockMovement = false;
        gc.player.OnControlChange();

        gc.enemyFreeze = false;
    }

    IEnumerator LoadSceneHelper(){
        yield return new WaitForSeconds(1f); // We always want to see the cute Dino
        SceneManager.LoadSceneAsync(singleSceneLoading);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        if(_instance != this){
            //_instance.OnSceneLoaded(scene, mode);
            return;
        }

        if(usingLoadingScreen){
            if(loadingNextScene){ // Successfully loaded new Scene
                loadingTheLoadingScreen = false;
                loadingNextScene = false;
                singleSceneLoading = "";
            }
            else if(loadingTheLoadingScreen){ // Currently in Loading Scene
                loadingTheLoadingScreen = false;
                loadingNextScene = true;

                StartCoroutine(LoadSceneHelper());
                return;
            }
        }

        // Check for double loading
        if(loadingScene){
            return;
        } else {
            loadingScene = true;
        }

        // Find GlobalsController in case of catastrophic failure
        gc = GlobalsController.Instance;
        uic = UIController.Instance;

        // Destroy any existing players
        if(gc.player != null){
            Destroy(gc.player.gameObject);
        }

        // Spawn player
        Transform spawnPoint = default(Transform);
        if(spawnPointName != null && spawnPointName.Length > 0){
            spawnPoint = GameObject.Find(spawnPointName).transform;
        } else try {
            spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;
        } catch(Exception) {
            Debug.Log("No spawn point in this scene");
        }

        // Check for no spawn point
        if(spawnPoint != null) {
            GameObject player = (GameObject)Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            player.transform.rotation = spawnPoint.rotation;
        }

        // Tell GlobalsController to find player, camera, etc.
        gc.FindAll();

        if(uic != null){
            // Reset Dialogue UI
            uic.ResetDialogue();
        }

        try {
            bgMusic = GameObject.FindWithTag("Music").GetComponent<AudioSource>();
        } catch {
            Debug.Log("There is no background music in this scene");
        }

        // Reset screen wipe stuff
        if(Camera.main != null){
            maskTextureOverrideIndex = -1;
            maskInvert = true;

            gc.enemyFreeze = true;
            if(gc.player != null){
                gc.player.lockMovement = true;

                screenWipeController = Camera.main.GetComponent<ScreenTransitionImageEffect>();
                StartCoroutine(screenWipeEnumerator());
            }
        }

        spawnPointName = null;

        loadingScene = false;
    }

    public void RemoveAdditiveScene(string sceneName){
        toUnloadScenes.Add(sceneName);
        additiveScenes.Remove(sceneName);
        unloadScenes();
    }

    void unloadScenes(){
        foreach(string sceneName in toUnloadScenes){
            try {
                SceneManager.UnloadSceneAsync(sceneName);
            } catch {
                continue;
            }
        }
        toUnloadScenes = new List<string>();
    }
}
