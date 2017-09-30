using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIButtonMessage))]
public class CheckWordButton : PhonoBlocksSubscriber {

	public override void SubscribeToAll(PhonoBlocksScene forScene){
		if(forScene == PhonoBlocksScene.MainMenu) return;
		Transaction.Instance.NewProblemBegun.Subscribe(this,(ProblemData problem) => {
			gameObject.SetActive(true);
		});
		//transition automatically from all letters removed to beginning of next problem; no need to press submit button again.
		Transaction.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(this,() =>{
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputLocked.Subscribe(this,() => {
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
			gameObject.SetActive(true);
		});
	}


	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "CheckWord";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;

	
	}

	void CheckWord(){

		if (Transaction.Instance.State.UIInputLocked)
			return;
		Transaction.Instance.UserSubmittedTheirLetters.Fire ();
	 

	}
}
