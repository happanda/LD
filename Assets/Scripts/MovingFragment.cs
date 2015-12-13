using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class MovingFragment : MonoBehaviour
{
    public float outerRadius = 12f; // radius of random start position
    public float speedMin = 2f;
    public float speedMax = 6f;

    private Vector3 flightDir;
    private float speed;
    public Tile type;

    void Awake()
    {
        transform.position = RandomStart();
        flightDir = (Vector3.zero - transform.position).normalized;
        speed = Random.Range(speedMin, speedMax);
        type = TileExt.Random();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + flightDir * speed * Time.deltaTime;
            // if the fragment flies to far from center, destroy it
        if (transform.position.sqrMagnitude > outerRadius * outerRadius * 2)
            Destroy(gameObject);
    }

    public Vector3 RandomStart()
    {
        float angle = Random.value * Mathf.PI * 2f;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * outerRadius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
