using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ToggleSyllableDivisionShowButton : PhonoBlocksSubscriber {
	public override void SubscribeToAll(PhonoBlocksScene forScene){
		if(forScene == PhonoBlocksScene.MainMenu) return;


		Transaction.Instance.UIInputLocked.Subscribe(this,() => {
			gameObject.SetActive(false);
		});
		Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
			gameObject.SetActive(true);
		});
	}
	void Start(){
		UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "ToggleSyllableDivisionShow";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;

	}

	void ToggleSyllableDivisionShow(){

		if (Transaction.Instance.State.UIInputLocked)
			return;

		WordColorShowStates current = Transaction.Instance.State.WordColorShowState;
		Transaction.Instance.WordColorShowStateSet.Fire (
			current == WordColorShowStates.SHOW_TARGET_UNITS ?
			WordColorShowStates.SHOW_WHOLE_WORD : WordColorShowStates.SHOW_TARGET_UNITS);
	}
}
