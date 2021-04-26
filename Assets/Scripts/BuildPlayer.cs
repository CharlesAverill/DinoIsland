using UnityEngine;
using UnityEditor;
#if (UNITY_EDITOR)
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

public class BuildPlayer : EditorWindow {

    enum BuildVersion{
        Windows,
        OSX,
        Linux
    }

    int nScenes;
    static string[] sceneNames;

    bool showScenes;

    bool buildWin;
    bool buildMac;
    bool buildLin;

    static string buildPath;

    [MenuItem("Tools/BuildPlayer")]
    private static void OpenBuilder() {
        buildPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "/";
        new BuildPlayer().Show();
    }

    BuildPlayerOptions getBuildOptions(BuildVersion bv){
        BuildPlayerOptions options = new BuildPlayerOptions();

        string[] formattedSceneNames = new string[nScenes];
        for(int i = 0; i < nScenes; i++){
            formattedSceneNames[i] = "Assets/Scenes/" + sceneNames[i] + ".unity";
        }

        options.scenes = formattedSceneNames;
        options.options = BuildOptions.None;
        switch(bv){
            case BuildVersion.Windows:
                options.locationPathName = buildPath + "Builds/Windows/DinosaurIsland.exe";
                options.target = BuildTarget.StandaloneWindows64;
                break;
            case BuildVersion.OSX:
                options.locationPathName = buildPath + "Builds/Mac/DinosaurIsland.app";
                options.target = BuildTarget.StandaloneOSX;
                break;
            case BuildVersion.Linux:
                options.locationPathName = buildPath + "Builds/Linux/DinosaurIsland.x86_64";
                options.target = BuildTarget.StandaloneLinux64;
                break;
            default:
                Debug.Log("BuildVersion " + bv + " not recognized");
                break;
        }

        return options;
    }

    void OnGUI() {
        LoadPrefs();

        EditorGUILayout.BeginHorizontal();

        nScenes = EditorGUILayout.DelayedIntField("Number of scenes in build:", nScenes);
        showScenes = EditorGUILayout.Toggle("Show scenes:", showScenes);

        EditorGUILayout.EndHorizontal();

        if(sceneNames != null){
            List<string> sceneNamesList = new List<string>(sceneNames);
            int difference = nScenes - sceneNames.Length;

            if(difference > 0){
                sceneNamesList.AddRange(new string[difference]);
            } else if(difference < 0){
                sceneNamesList.RemoveRange(sceneNamesList.Count + difference, -difference);
            }

            sceneNames = sceneNamesList.ToArray();
        }

        for(int i = 0; showScenes && i < nScenes; i++){
            sceneNames[i] = EditorGUILayout.TextField("Scene " + i + ":", sceneNames[i]);
        }

        buildWin = EditorGUILayout.Toggle("Build Windows", buildWin);
        buildMac = EditorGUILayout.Toggle("Build OSX", buildMac);
        buildLin = EditorGUILayout.Toggle("Build Linux", buildLin);

        if (GUILayout.Button("Build")) {
            if(buildWin){
                BuildReport winReport = BuildPipeline.BuildPlayer(getBuildOptions(BuildVersion.Windows));
                BuildSummary winSummary = winReport.summary;
            }
            if(buildMac){
                BuildReport macReport = BuildPipeline.BuildPlayer(getBuildOptions(BuildVersion.OSX));
                BuildSummary macSummary = macReport.summary;
            }
            if(buildLin){
                BuildReport linReport = BuildPipeline.BuildPlayer(getBuildOptions(BuildVersion.Linux));
                BuildSummary linSummary = linReport.summary;
            }
        }

        SavePrefs();
    }

    void LoadPrefs(){
        nScenes = EditorPrefs.GetInt("BuildPlayer_nScenes");
        sceneNames = new string[nScenes];

        for(int i = 0; i < nScenes; i++){
            sceneNames[i] = EditorPrefs.GetString("BuildPlayer_Scene" + (i + 1));
        }

        showScenes = EditorPrefs.GetBool("BuildPlayer_showScenes");

        buildWin = EditorPrefs.GetBool("BuildPlayer_buildWin");
        buildMac = EditorPrefs.GetBool("BuildPlayer_buildMac");
        buildLin = EditorPrefs.GetBool("BuildPlayer_buildLin");
    }

    void SavePrefs(){
        EditorPrefs.SetInt("BuildPlayer_nScenes", nScenes);

        for(int i = 0; i < nScenes; i++){
            EditorPrefs.SetString("BuildPlayer_Scene" + (i + 1), sceneNames[i]);
        }

        EditorPrefs.SetBool("BuildPlayer_showScenes", showScenes);

        EditorPrefs.SetBool("BuildPlayer_buildWin", buildWin);
        EditorPrefs.SetBool("BuildPlayer_buildMac", buildMac);
        EditorPrefs.SetBool("BuildPlayer_buildLin", buildLin);
    }
}
#endif
