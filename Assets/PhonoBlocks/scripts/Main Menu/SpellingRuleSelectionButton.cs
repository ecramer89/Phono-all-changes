using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class SpellingRuleSelectionButton : PhonoBlocksSubscriber {
	[SerializeField] Activity activity;


	public override void SubscribeToAll(PhonoBlocksScene forScene){
		Transaction.Instance.ModeSelected.Subscribe(this,(Mode mode) => {
			if (mode == Mode.TEACHER) {
				UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "SelectActivity";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;
				gameObject.SetActive(true);
			} else {
				gameObject.SetActive(false);
			}
		});
		Transaction.Instance.ActivitySelected.Subscribe(this,(Activity activity) => {
			gameObject.SetActive(false);
		});

	}


	void Start(){
		gameObject.SetActive(false);
	

	}



	void SelectActivity(){

		Transaction.Instance.ActivitySelected.Fire (activity);


	}
}
