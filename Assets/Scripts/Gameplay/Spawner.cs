using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Vector3 spawnLocation;
    public GameObject[] prefabs;

    public int maxSpawns;
    public int currentSpawnIndex;

    List<GameObject> spawns;
    float spawnTimer;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        currentSpawnIndex = -1;

        spawns = new List<GameObject>();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(spawnTimer > .1f){
            spawns.RemoveAll(item => item == null);
        }

        if(spawnTimer > 4f && spawns.Count < maxSpawns){
            currentSpawnIndex += 1;
            if(currentSpawnIndex >= prefabs.Length){
                currentSpawnIndex = 0;
            }

            spawns.Add(Instantiate(prefabs[currentSpawnIndex],
                                   transform.position + spawnLocation,
                                   Quaternion.identity));

            spawnTimer = 0f;

            audioSource.Play();
        }

        spawnTimer += Time.deltaTime;
    }
}
