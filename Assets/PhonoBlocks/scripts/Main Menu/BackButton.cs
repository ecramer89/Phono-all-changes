using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class BackButton : PhonoBlocksSubscriber {

	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "GoBack";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
		gameObject.SetActive(false);

	}

	public override void SubscribeToAll(PhonoBlocksScene forScene){
		if(forScene != PhonoBlocksScene.MainMenu) return;
		Transaction.Instance.MainMenuNavigationStateChanged.Subscribe(this, (ParameterlessEvent obj) => {
			gameObject.SetActive(true);
		});
	}


	void GoBack(){
		Stack<ParameterlessEvent> mainMenuNavStack = Transaction.Instance.State.MainMenuNavigationStack;
		if(mainMenuNavStack.Count>0) mainMenuNavStack.Pop().Fire();
		gameObject.SetActive(mainMenuNavStack.Count>0);
	}
}
