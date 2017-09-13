using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class SessionButton : MonoBehaviour {
	[SerializeField] int session;

	void Start(){
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			if (mode == Mode.STUDENT) {
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "SelectSession";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				gameObject.SetActive(true);
			} else {
				gameObject.SetActive(false);
			}
		};
	}



	void SelectSession(){

		Events.Dispatcher.RecordSessionSelected (session);
		gameObject.SetActive(false);

	}
}
