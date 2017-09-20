using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class RequestHintButton : MonoBehaviour {


	void Start(){
			gameObject.SetActive(false); //inactive to begin with.
			if (Dispatcher._State.Mode == Mode.STUDENT) { //only bother subscribing to events that would set active in student mode
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "RequestHint";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				Dispatcher.Instance.NewProblemBegun.Subscribe((ProblemData problem) => {
						gameObject.SetActive (false);
				});
				Dispatcher.Instance.UIInputLocked.Subscribe(() => {
						gameObject.SetActive (false);
				});
				//hint button should only be active if a hint is available, a
				//state that is toggled by events user submits incorrect answer (hint is available)
				//and hint is provided (hint no longer available).
				//as such, shouldn't set the hint button to active again unless the hint is currently available.
				Dispatcher.Instance.UIInputUnLocked.Subscribe(() => {
						gameObject.SetActive(Dispatcher._State.HintAvailable);
				});
				Dispatcher.Instance.HintProvided.Subscribe(() => {
						
						gameObject.SetActive (false);
				});
				Dispatcher.Instance.UserSubmittedIncorrectAnswer.Subscribe(() => {
						gameObject.SetActive(Dispatcher._State.StudentModeState == StudentModeStates.MAIN_ACTIVITY);
				});
		}
	}



	void RequestHint(){
		if (Dispatcher._State.UIInputLocked || Dispatcher._State.Mode == Mode.TEACHER)
			return;

		Dispatcher.Instance.HintRequested.Fire ();
		

	}
}
