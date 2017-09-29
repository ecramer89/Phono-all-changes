using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ToggleSyllableDivisionShowButton : PhonoBlocksSubscriber {
	public override void SubscribeToAll(PhonoBlocksScene forScene){}
	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ToggleSyllableDivisionShow";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
	
	
		Transaction.Instance.ActivitySelected.Subscribe(this,(Activity activity) => {
		  gameObject.SetActive(activity == Activity.SYLLABLE_DIVISION);
		});

		Transaction.Instance.UIInputLocked.Subscribe(this,() => {
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
			gameObject.SetActive(true);
		});
	}

	void ToggleSyllableDivisionShow(){

		if (Transaction.Instance.State.UIInputLocked)
			return;
		Transaction.Instance.SyllableDivisionShowStateToggled.Fire ();


	}
}
