using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class SpellingRuleSelectionButton : MonoBehaviour {
	[SerializeField] Activity activity;

	void Start(){
		gameObject.SetActive(false);
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			if (mode == Mode.TEACHER) {
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "SelectActivity";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				gameObject.SetActive(true);
				Events.Dispatcher.OnActivitySelected += (Activity activity) => {
					gameObject.SetActive(false);
				};
			} else {
				gameObject.SetActive(false);
			}
		};

	}



	void SelectActivity(){

		Events.Dispatcher.RecordActivitySelected (activity);


	}
}
