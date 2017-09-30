using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class RequestHintButton : PhonoBlocksSubscriber {


	public override void SubscribeToAll(PhonoBlocksScene scene){
		if(scene == PhonoBlocksScene.MainMenu) return;


		if(Transaction.Instance.State.Mode != Mode.STUDENT) return;

		Transaction.Instance.ActivitySceneLoaded.Subscribe(this,() => {
			UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
			messenger.target = gameObject;
			messenger.functionName = "RequestHint";
			messenger.trigger = UIButtonMessage.Trigger.OnClick;
		});

		Transaction.Instance.NewProblemBegun.Subscribe(this,(ProblemData problem) => {
			gameObject.SetActive (false);
		});
		Transaction.Instance.UIInputLocked.Subscribe(this,() => {
			gameObject.SetActive (false);
		});
		//hint button should only be active if a hint is available, a
		//state that is toggled by events user submits incorrect answer (hint is available)
		//and hint is provided (hint no longer available).
		//as such, shouldn't set the hint button to active again unless the hint is currently available.
		Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
			gameObject.SetActive(Transaction.Instance.State.HintAvailable);
		});
		Transaction.Instance.HintProvided.Subscribe(this,() => {

			gameObject.SetActive (false);
		});
		Transaction.Instance.UserSubmittedIncorrectAnswer.Subscribe(this,() => {
			gameObject.SetActive(Transaction.Instance.State.StudentModeState == StudentModeStates.MAIN_ACTIVITY);
		});

	}



	void Start(){
			gameObject.SetActive(false); //inactive to begin with.
	}



	void RequestHint(){
		if (Transaction.Instance.State.UIInputLocked || Transaction.Instance.State.Mode == Mode.TEACHER)
			return;

		Transaction.Instance.HintRequested.Fire ();
		

	}
}
