using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ReplayInstructionsButton : MonoBehaviour {

	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ReplayInstructions";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;


		Events.Dispatcher.OnUIInputLocked += () => {
			gameObject.SetActive(false);
		};
		Events.Dispatcher.OnUIInputUnLocked += () => {
			gameObject.SetActive(true);
		};
	}

	void ReplayInstructions(){
		if (State.Current.UIInputLocked)
			return;
		AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);

	}
}
