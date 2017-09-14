using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIButtonMessage))]
public class ModeSelectionButton : MonoBehaviour {
	[SerializeField] Mode mode;

	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "SelectMode";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
		//any mode selection button (not just the one that was clicked) needs to deactivate 
		//when any one of them is clicked.
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
				gameObject.SetActive(false);
		};

	}


	void SelectMode(){
		Events.Dispatcher.RecordModeSelected (mode);
	}
}
