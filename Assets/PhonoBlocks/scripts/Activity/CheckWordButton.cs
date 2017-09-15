using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class CheckWordButton : MonoBehaviour {

	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "CheckWord";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;

		Events.Dispatcher.OnNewProblemBegun += (ProblemData problem) => {
			gameObject.SetActive(true);
		};
		//transition automatically from all letters removed to beginning of next problem; no need to press submit button again.
		Events.Dispatcher.OnEnterForceRemoveAllLetters += () =>{
			gameObject.SetActive(false);
		};
		Events.Dispatcher.OnUIInputLocked += () => {
			gameObject.SetActive(false);
		};
		Events.Dispatcher.OnUIInputUnLocked += () => {
			gameObject.SetActive(true);
		};
	}

	void CheckWord(){

		if (State.Current.UIInputLocked)
			return;
		Events.Dispatcher.RecordUserSubmittedTheirLetters ();
	 

	}
}
