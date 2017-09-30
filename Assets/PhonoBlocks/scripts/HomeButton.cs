using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class HomeButton : MonoBehaviour {
	[SerializeField] Mode mode;

	void Start(){

		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ReturnToMainMenu";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;


	}


	void ReturnToMainMenu(){
		Transaction.Instance.Restart();

	}
}
