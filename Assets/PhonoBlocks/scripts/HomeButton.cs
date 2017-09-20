using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class HomeButton : MonoBehaviour {
	[SerializeField] Mode mode;

	void Start(){
		gameObject.SetActive(false);
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ReturnToMainMenu";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
		SceneManager.sceneLoaded += (Scene scene, LoadSceneMode arg1) => {
			gameObject.SetActive(scene.name=="Activity");
		};
	}


	void ReturnToMainMenu(){
		SceneManager.LoadScene("MainMenu");
	}
}
