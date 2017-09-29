using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButtonMessage))]
public class ReplayInstructionsButton : PhonoBlocksSubscriber {
	public override void SubscribeToAll(PhonoBlocksScene scene){}
	void Start(){
			if (Transaction.Instance.State.Mode == Mode.STUDENT) {
				gameObject.SetActive(true);
				UIButtonMessage messenger= GetComponent<UIButtonMessage> ();
				messenger.target = gameObject;
				messenger.functionName = "ReplayInstructions";
				messenger.trigger = UIButtonMessage.Trigger.OnClick;


			Transaction.Instance.UIInputLocked.Subscribe(this,() => {
						gameObject.SetActive(false);
				});
			Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
						gameObject.SetActive(true);
				});
			} else {
				gameObject.SetActive(false);
			}

	}

	void ReplayInstructions(){
		if (Transaction.Instance.State.UIInputLocked)
			return;
		AudioSourceController.PushClips (Transaction.Instance.State.CurrentProblemInstructions);

	}
}
