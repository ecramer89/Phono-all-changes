using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class RequestHintButton : MonoBehaviour {


	void Start(){
			gameObject.SetActive(false); //inactive to begin with.
			if (State.Current.Mode == Mode.STUDENT) { //only bother subscribing to events that would set active in student mode
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "RequestHint";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				Dispatcher.Instance.OnNewProblemBegun += (ProblemData problem) => {
					gameObject.SetActive (false);
				};
				Dispatcher.Instance.OnUIInputLocked += () => {
					gameObject.SetActive (false);
				};
				//hint button should only be active if a hint is available, a
				//state that is toggled by events user submits incorrect answer (hint is available)
				//and hint is provided (hint no longer available).
				//as such, shouldn't set the hint button to active again unless the hint is currently available.
				Dispatcher.Instance.OnUIInputUnLocked += () => {
					gameObject.SetActive(State.Current.HintAvailable);
				};
				Dispatcher.Instance.OnHintProvided += () => {
					
					gameObject.SetActive (false);
				};
				Dispatcher.Instance.OnUserSubmittedIncorrectAnswer += () => {
					gameObject.SetActive(State.Current.StudentModeState == StudentModeStates.MAIN_ACTIVITY);
				};
		}
	}



	void RequestHint(){
		if (State.Current.UIInputLocked || State.Current.Mode == Mode.TEACHER)
			return;

		Dispatcher.Instance.RecordUserRequestedHint ();
		

	}
}
