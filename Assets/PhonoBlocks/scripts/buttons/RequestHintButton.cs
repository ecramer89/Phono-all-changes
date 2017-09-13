using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class RequestHintButton : MonoBehaviour {

	void Start(){
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			if (mode == Mode.STUDENT) {
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "RequestHint";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				Events.Dispatcher.OnNewProblemBegun += () => {
					gameObject.SetActive (false);
				};
				Events.Dispatcher.OnUIInputLocked += () => {
					gameObject.SetActive (false);
				};
				Events.Dispatcher.OnUIInputUnLocked += () => {
					gameObject.SetActive (true);
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
		};
	}



	void RequestHint(){
		if (State.Current.UIInputLocked || State.Current.Mode == Mode.TEACHER)
			return;

		Events.Dispatcher.RecordUserRequestedHint ();
		

	}
}
