using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class BackButton : MonoBehaviour {

	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "GoBack";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
	}


	void GoBack(){
		Stack<Action> mainMenuNavStack = Transaction.Instance.State.MainMenuNavigationStack;
		if(mainMenuNavStack.Count>0) mainMenuNavStack.Pop()();
	}
}
