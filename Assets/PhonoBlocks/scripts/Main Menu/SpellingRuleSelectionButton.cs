using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class SpellingRuleSelectionButton : MonoBehaviour {
	[SerializeField] Activity activity;

	void Start(){
		gameObject.SetActive(false);
		Transaction.Instance.ModeSelected.Subscribe((Mode mode) => {
			if (mode == Mode.TEACHER) {
					UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
					messenger.target = gameObject;
					messenger.functionName = "SelectActivity";
					messenger.trigger = UIButtonMessage.Trigger.OnClick;
					gameObject.SetActive(true);
					Transaction.Instance.ActivitySelected.Subscribe((Activity activity) => {
						gameObject.SetActive(false);
					});
			} else {
				gameObject.SetActive(false);
			}
		});

	}



	void SelectActivity(){

		Transaction.Instance.ActivitySelected.Fire (activity);


	}
}
