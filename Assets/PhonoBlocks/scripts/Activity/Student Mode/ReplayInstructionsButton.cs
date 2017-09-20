using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ReplayInstructionsButton : MonoBehaviour {

	void Start(){
			if (State.Current.Mode == Mode.STUDENT) {
				gameObject.SetActive(true);
				UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "ReplayInstructions";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;


				Dispatcher.Instance.OnUIInputLocked += () => {
					gameObject.SetActive(false);
				};
				Dispatcher.Instance.OnUIInputUnLocked += () => {
					gameObject.SetActive(true);
				};
			} else {
				gameObject.SetActive(false);
			}

	}

	void ReplayInstructions(){
		if (State.Current.UIInputLocked)
			return;
		AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);

	}
}
