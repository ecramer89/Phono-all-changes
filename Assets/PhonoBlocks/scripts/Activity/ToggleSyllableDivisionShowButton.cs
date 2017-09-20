using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ToggleSyllableDivisionShowButton : MonoBehaviour {

	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ToggleSyllableDivisionShow";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
	
	
		Dispatcher.Instance.ActivitySelected.Subscribe((Activity activity) => {
		  gameObject.SetActive(activity == Activity.SYLLABLE_DIVISION);
		});

		Dispatcher.Instance.OnUIInputLocked += () => {
			gameObject.SetActive(false);
		};
		Dispatcher.Instance.OnUIInputUnLocked += () => {
			gameObject.SetActive(true);
		};
	}

	void ToggleSyllableDivisionShow(){

		if (Dispatcher._State.UIInputLocked)
			return;
		Dispatcher.Instance.RecordSyllableDivisionShowStateToggled ();


	}
}
