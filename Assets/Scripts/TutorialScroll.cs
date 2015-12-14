using UnityEngine;
using System.Collections;

public class TutorialScroll : MonoBehaviour {
	
	public float scrollSpeed = 1;

	// Use this for initialization
	void Start () {
		transform.position += Vector3.left * (Time.deltaTime * scrollSpeed);
	
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.x>-32) {
		transform.position += Vector3.left * (Time.deltaTime * scrollSpeed);
		}
		else {
			Application.LoadLevel(1);
		}
	
	}
}
