using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

public class PlatformerUtils : EditorWindow
{

    int layerOffsetLayer;
    Vector3 layerOffsetVector;
    bool layerOffsetGroundCheck;

    string tagOffsetTag;
    Vector3 tagOffsetVector;
    bool tagOffsetGroundCheck;

    [MenuItem("Tools/PlatformerUtils")]
    private static void OpenUtils() {
        new PlatformerUtils().Show();
    }

    void OnGUI() {
        LoadPrefs();

        EditorGUILayout.LabelField("Offset Objects in Layer", EditorStyles.boldLabel);
        layerOffsetLayer = EditorGUILayout.LayerField("Layer for Offsetting:", layerOffsetLayer);
        layerOffsetVector = EditorGUILayout.Vector3Field("Offset Vector", layerOffsetVector);
        layerOffsetGroundCheck = EditorGUILayout.Toggle("Check for Ground", layerOffsetGroundCheck);

        if (GUILayout.Button("Offset by Layer")) {
            GameObject[] toOffset = FindObjectsByLayer(layerOffsetLayer);
            foreach(GameObject obj in toOffset){
                OffsetObject(obj, layerOffsetVector, layerOffsetGroundCheck);
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Offset Objects with Tag", EditorStyles.boldLabel);
        tagOffsetTag = EditorGUILayout.TagField("Tag for Offsetting:", tagOffsetTag);
        tagOffsetVector = EditorGUILayout.Vector3Field("Offset Vector", tagOffsetVector);
        tagOffsetGroundCheck = EditorGUILayout.Toggle("Check for Ground", tagOffsetGroundCheck);

        if (GUILayout.Button("Offset by Tag")) {
            GameObject[] toOffset = GameObject.FindGameObjectsWithTag(tagOffsetTag);
            foreach(GameObject obj in toOffset){
                OffsetObject(obj, tagOffsetVector, tagOffsetGroundCheck);
            }
        }

        SavePrefs();
    }

    GameObject[] FindObjectsByLayer(int layer)
    {
        List<GameObject> validTransforms = new List<GameObject>();
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].gameObject.layer == layer)
                {
                    validTransforms.Add(objs[i].gameObject);
                }
            }
        }
        return validTransforms.ToArray();
    }

    void OffsetObject(GameObject obj, Vector3 offsetVector, bool checkGround){
        if(checkGround){
            RaycastHit hit;
            if(Physics.Raycast(obj.transform.position,
                               -obj.transform.up,
                               out hit,
                               Mathf.Infinity,
                               CONSTANTS.GROUND_MASK)){
                obj.transform.position = obj.transform.position + offsetVector;
                obj.transform.position = new Vector3(obj.transform.position.x,
                                                     obj.transform.position.y - hit.distance,
                                                     obj.transform.position.z);
                Debug.Log("Moved object " + obj.name);
            } else {
                Debug.Log("No ground detected underneath " + obj.name);
            }
        } else {
            obj.transform.position += offsetVector;
            Debug.Log("Moved object " + obj.name + " without checking for ground");
        }
    }

    void LoadPrefs(){
        layerOffsetLayer = EditorPrefs.GetInt("PlatformerUtils_layerOffsetLayer");
        layerOffsetVector = new Vector3(EditorPrefs.GetFloat("PlatformerUtils_layerOffsetVectorX"),
                                        EditorPrefs.GetFloat("PlatformerUtils_layerOffsetVectorY"),
                                        EditorPrefs.GetFloat("PlatformerUtils_layerOffsetVectorZ"));
    }

    void SavePrefs(){
        EditorPrefs.SetInt("PlatformerUtils_layerOffsetLayer", layerOffsetLayer);

        EditorPrefs.SetFloat("PlatformerUtils_layerOffsetVectorX", layerOffsetVector.x);
        EditorPrefs.SetFloat("PlatformerUtils_layerOffsetVectorY", layerOffsetVector.y);
        EditorPrefs.SetFloat("PlatformerUtils_layerOffsetVectorZ", layerOffsetVector.z);
    }
}
#endif
