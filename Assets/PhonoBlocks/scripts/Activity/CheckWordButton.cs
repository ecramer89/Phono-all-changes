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

		Transaction.Instance.NewProblemBegun.Subscribe((ProblemData problem) => {
			gameObject.SetActive(true);
		});
		//transition automatically from all letters removed to beginning of next problem; no need to press submit button again.
		Transaction.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(() =>{
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputLocked.Subscribe(() => {
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputUnLocked.Subscribe(() => {
			gameObject.SetActive(true);
		});
	}

	void CheckWord(){

		if (Transaction.State.UIInputLocked)
			return;
		Transaction.Instance.UserSubmittedTheirLetters.Fire ();
	 

	}
}
