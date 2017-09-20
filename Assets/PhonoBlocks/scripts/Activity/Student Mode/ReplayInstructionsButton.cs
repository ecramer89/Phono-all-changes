using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ReplayInstructionsButton : MonoBehaviour {

	void Start(){
			if (Dispatcher._State.Mode == Mode.STUDENT) {
				gameObject.SetActive(true);
				UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "ReplayInstructions";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;


				Dispatcher.Instance.UIInputLocked.Subscribe(() => {
						gameObject.SetActive(false);
				});
				Dispatcher.Instance.UIInputUnLocked.Subscribe(() => {
						gameObject.SetActive(true);
				});
			} else {
				gameObject.SetActive(false);
			}

	}

	void ReplayInstructions(){
		if (Dispatcher._State.UIInputLocked)
			return;
		AudioSourceController.PushClips (Dispatcher._State.CurrentProblemInstructions);

	}
}
