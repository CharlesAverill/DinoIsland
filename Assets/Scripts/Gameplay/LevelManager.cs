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

    [Header("Level Management")]
    public GameObject playerPrefab;

    public string currentSceneName;

    public Dictionary<string, AsyncOperation> additiveScenes;
    public List<string> toUnloadScenes;

    public bool loadingScene;

    public string singleSceneLoading;

    public string spawnPointName;
    [Space(5)]

    [Header("Screen Wipe")]
    public ScreenTransitionImageEffect screenWipeController;

    public Color maskColor = Color.black;

    public Texture2D[] maskTextures;
    public int maskTextureOverrideIndex = -1;
    public bool maskInvert;

    // Start is called before the first frame update
    void Start()
    {
        gc = GlobalsController.Instance;

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
        if(additive){
            if(additiveScenes.ContainsKey(sceneName)){
                return;
            }
            // Start loading and prevent activation
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); //, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            // Add to dictionary
            additiveScenes.Add(sceneName, op);

            return;
        }

        loadingScene = true;

        if(unloadCurrentScene){
            toUnloadScenes.Add(SceneManager.GetActiveScene().name);
        }

        spawnPointName = _spawnPointName;
        maskInvert = invertScreenWipe;

        StartCoroutine(loadSceneEnumerator(sceneName, additive, screenWipe));
    }

    public void ActivateAdditiveScene(string sceneName, string _spawnPointName, bool doScreenWipe=true){
        loadingScene = true;
        spawnPointName = _spawnPointName;
        StartCoroutine(loadSceneEnumerator(sceneName, true, doScreenWipe));
    }

    IEnumerator loadSceneEnumerator(string sceneName, bool additive, bool doScreenWipe){
        AsyncOperation asyncLoadOperation;
        if(additive){
            asyncLoadOperation = additiveScenes[sceneName];
            additiveScenes.Remove(sceneName);

            asyncLoadOperation.allowSceneActivation = true;
            while(!asyncLoadOperation.isDone){
                yield return null;
            }
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        } else {
            // Load scene, prevent from immediately activating
            asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncLoadOperation.allowSceneActivation = false;
        }

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

        loadingScene = false;
        yield return null;
    }

    IEnumerator screenWipeEnumerator(){
        screenWipeController.enabled = true;
        // Apply screen wipe settings
        screenWipeController.maskColor = maskColor;
        screenWipeController.maskInvert = maskInvert;

        // Pick random screen wipe mask
        Texture2D maskTexture;
        if(maskTextureOverrideIndex == -1){
            maskTexture = maskTextures[UnityEngine.Random.Range(0, maskTextures.Length - 1)];
        } else {
            maskTexture = maskTextures[maskTextureOverrideIndex];
        }
        screenWipeController.maskTexture = maskTexture;

        // Screen Wipe
        float step = 1f / 64f;
        for(int i = 0; i < 64; i++){
            screenWipeController.maskValue = step * i;
            yield return new WaitForSeconds(step / 2f);
        }
        screenWipeController.maskValue = 1f;

        maskInvert = false;
        screenWipeController.enabled = false;
    }

    void OnEnable(){
        //OnSceneLoaded(default(Scene), default(LoadSceneMode));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        if(loadingScene){
            return;
        }
        Debug.Log("Onsceneloaded");
        // Check for double loading
        if(gc != null && gc.player != null){
            return;
        }

        // Find GlobalsController in case of catastrophic failure
        gc = GlobalsController.Instance;

        if(mode == LoadSceneMode.Single){
            singleSceneLoading = "";
        }

        // Destroy any existing players
        if(gc.player != null){
            Destroy(gc.player.gameObject);
        }

        // Spawn player
        Transform spawnPoint;
        if(spawnPointName != ""){
            spawnPoint = GameObject.Find(spawnPointName).transform;
        } else {
            spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;
        }
        // Check for no spawn point
        if(spawnPoint == null){
            throw new Exception("Could not find a spawn point. Check if one exists and that the name is spelled correctly in the warp.");
        } else {
            GameObject player = (GameObject)Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            player.transform.rotation = spawnPoint.rotation;
        }

        // Tell GlobalsController to find player, dialogue, camera, etc.
        gc.FindAll();

        // Reset screen wipe stuff
        if(Camera.main != null){
            maskTextureOverrideIndex = -1;
            maskInvert = true;

            screenWipeController = Camera.main.GetComponent<ScreenTransitionImageEffect>();
            StartCoroutine(screenWipeEnumerator());
        }

        spawnPointName = null;
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
