using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Vector3 spawnLocation;
    public GameObject[] prefabs;
    public int maxSpawns;
    public int currentSpawns;
    public int currentSpawnIndex;

    int frameCount;

    // Start is called before the first frame update
    void Start()
    {
        currentSpawns = 0;
        currentSpawnIndex = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if(frameCount % 60 == 0 && currentSpawns < maxSpawns){
            currentSpawnIndex += 1;
            if(currentSpawnIndex >= prefabs.Length){
                currentSpawnIndex = 0;
            }

            Instantiate(prefabs[currentSpawnIndex],
                        transform.position + spawnLocation,
                        Quaternion.identity);

            currentSpawns += 1;
        }
        frameCount += 1;
    }
}
