using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{   
    public Transform[] spawnPoint;
    public GameObject[] people;
    int randomSpawnPoint, randomPeople;
    public static bool spawnAllowed;
    public float timer;

    // Start is called before the first frame update
    void Start()
    {
        spawnAllowed = true;
        InvokeRepeating("SpawnNow", 0f, timer);
    }

    // Update is called once per frame
    void SpawnNow()
    {
        if (spawnAllowed)
        {
        randomSpawnPoint = Random.Range(0, spawnPoint.Length);
        randomPeople = Random.Range(0, people.Length);
        Instantiate (people[randomPeople], spawnPoint[randomSpawnPoint].position, Quaternion.identity);   
        }
    }
}
