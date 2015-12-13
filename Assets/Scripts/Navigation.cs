using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Navigation : MonoBehaviour {

	public Button quit;
	public Button play;

	void Start () {
		play = play.GetComponent<Button>();
		quit = quit.GetComponent<Button>();
	}

	public void QuitPress () {
		Application.Quit();
	}

	public void PlayPress() {
		Application.LoadLevel(1);
	}



}
