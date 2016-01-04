using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class Clouds : MonoBehaviour
{
    public GameObject[] clouds;

    public float minCloudTime = 4f;
    public float maxCloudTime = 5f;

    private float nextCloudTime;
    private Transform cloudStuff;

    void Awake()
    {
        cloudStuff = new GameObject("CloudsStuff").transform;
        nextCloudTime = Random.Range(minCloudTime, maxCloudTime);
    }

    void Update()
    {
        if (nextCloudTime < Time.time)
        {
            LaunchCloud();
            nextCloudTime = Time.time + Random.Range(minCloudTime, maxCloudTime);
        }
    }

    void LaunchCloud()
    {
        GameObject cloud = Instantiate(clouds[Random.Range(0, clouds.Length)]);
        cloud.transform.SetParent(cloudStuff);
    }
}
