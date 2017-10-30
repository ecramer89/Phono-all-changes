using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class ExitButton : MonoBehaviour {
	[SerializeField] Mode mode;

	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "Exit";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
	}


	//note: this only has effect if running the build; not if just running through the editor.
	void Exit(){
		Application.Quit();
	}
}
