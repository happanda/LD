﻿using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class Clouds : MonoBehaviour
{
    public GameObject[] clouds;

    public float minCloudTime = 3.0f;
    public float maxCloudTime = 5.0f;

    private float nextCloudTime;

    void Awake()
    {
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
    }
}