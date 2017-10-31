using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class ModeSelectionButton : PhonoBlocksSubscriber {
	[SerializeField] Mode mode;
	public override void SubscribeToAll(PhonoBlocksScene forScene){
		if(forScene != PhonoBlocksScene.MainMenu) return;

		//any mode selection button (not just the one that was clicked) needs to deactivate 
		//when any one of them is clicked.
		Transaction.Instance.ModeSelected.Subscribe(this,(Mode mode) => {
			//since users need to enter their name following click the student mode selection button,
			//it's a bit nicer to just leave the button onscreen rather than remove it entirely.
			if(mode == Mode.TEACHER || this.mode == Mode.TEACHER){
				gameObject.SetActive(false);
			} else {
				GetComponent<UIImageButton>().enabled = false; //disable sprite change on hover
				GetComponent<UIButtonMessage> ().enabled=false; //leave the student mode button onscreen but 
				//disable its click functionality.
			}
		});
		Transaction.Instance.UndoModeSelected.Subscribe(this, ()=>{
			gameObject.SetActive(true);
			GetComponent<UIImageButton>().enabled = true; 
			GetComponent<UIButtonMessage> ().enabled=true;
		});
		//teacher mode button stays on while students enter name;
		//disappears after name entered+data successfully loaded or created.
		Transaction.Instance.StudentDataRetrieved.Subscribe(this,()=>{
			if(this.mode == Mode.STUDENT){
				gameObject.SetActive(false);
			}
		});
		Transaction.Instance.UndoStudentDataRetrieved.Subscribe(this, ()=>{
				gameObject.SetActive(this.mode == Mode.STUDENT);
		});
	}


	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "SelectMode";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;


	}


	void SelectMode(){
		Transaction.Instance.ModeSelected.Fire (mode);
		//push the event that will undo the changes resulting from mode selection to the stack.
		Transaction.Instance.MainMenuNavigationStateChanged.Fire(Transaction.Instance.UndoModeSelected);
	}
}
