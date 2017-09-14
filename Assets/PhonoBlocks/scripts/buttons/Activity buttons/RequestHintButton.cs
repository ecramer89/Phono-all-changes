using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class RequestHintButton : MonoBehaviour {


	void Start(){
			if (State.Current.Mode == Mode.STUDENT) {
				gameObject.SetActive(false); //inactive to begin with.
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "RequestHint";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				Events.Dispatcher.OnNewProblemBegun += (Problem problem) => {
					gameObject.SetActive (false);
				};
				Events.Dispatcher.OnUIInputLocked += () => {
					gameObject.SetActive (false);
				};
				//hint button should only be active if a hint is available, a
				//state that is toggled by events user submits incorrect answer (hint is available)
				//and hint is provided (hint no longer available).
				//as such, shouldn't set the hint button to active again unless the hint is currently available.
				Events.Dispatcher.OnUIInputUnLocked += () => {
					gameObject.SetActive(State.Current.HintAvailable);
				};
				Events.Dispatcher.OnHintProvided += () => {
					
					gameObject.SetActive (false);
				};
				Events.Dispatcher.OnUserSubmittedIncorrectAnswer += () => {
					if (State.Current.ActivityState == ActivityStates.MAIN_ACTIVITY) {

						gameObject.SetActive (true);
					}
				};
			} else {
				gameObject.SetActive(false);
			}
	}



	void RequestHint(){
		if (State.Current.UIInputLocked || State.Current.Mode == Mode.TEACHER)
			return;

		Events.Dispatcher.RecordUserRequestedHint ();
		

	}
}
