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

		Dispatcher.Instance.NewProblemBegun.Subscribe((ProblemData problem) => {
			gameObject.SetActive(true);
		});
		//transition automatically from all letters removed to beginning of next problem; no need to press submit button again.
		Dispatcher.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(() =>{
			gameObject.SetActive(false);
		});
		Dispatcher.Instance.UIInputLocked.Subscribe(() => {
			gameObject.SetActive(false);
		});
		Dispatcher.Instance.UIInputUnLocked.Subscribe(() => {
			gameObject.SetActive(true);
		});
	}

	void CheckWord(){

		if (Dispatcher._State.UIInputLocked)
			return;
		Dispatcher.Instance.UserSubmittedTheirLetters.Fire ();
	 

	}
}
