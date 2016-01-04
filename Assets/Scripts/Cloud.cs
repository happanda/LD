using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour
{
    void Awake()
    {
        Vector3 pos = transform.position;
        pos.x = Random.value * 24f - 12f;
        transform.position = pos;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, pos - new Vector3(0f, 100f, 0f), Time.deltaTime * 1f);
        if (pos.y < -24f)
            Destroy(gameObject);
    }
}
