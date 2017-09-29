using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIButtonMessage))]
[RequireComponent(typeof(UIImageButton))]
public class ModeSelectionButton : MonoBehaviour {
	[SerializeField] Mode mode;

	void Start(){
		UIButtonMessage messenger = GetComponent<UIButtonMessage> ();
		messenger.target = gameObject;
		messenger.functionName = "SelectMode";
		messenger.trigger = UIButtonMessage.Trigger.OnClick;
		//any mode selection button (not just the one that was clicked) needs to deactivate 
		//when any one of them is clicked.
		Transaction.Instance.ModeSelected.Subscribe((Mode mode) => {
			//since users need to enter their name following click the student mode selection button,
			//it's a bit nicer to just leave the button onscreen rather than remove it entirely.
			if(mode == Mode.TEACHER || this.mode == Mode.TEACHER){
				gameObject.SetActive(false);
			} else {
				//todo this doesn't seem to be working; find out why
				GetComponent<UIImageButton>().enabled = false; //disable sprite change on hover
				messenger.enabled=false; //leave the student mode button onscreen but 
				//disable its click functionality.
			}
		});
		//teacher mode button stays on while students enter name;
		//disappears after name entered+data successfully loaded or created.
		Transaction.Instance.StudentDataRetrieved.Subscribe(()=>{
			if(this.mode == Mode.STUDENT){
				gameObject.SetActive(false);
			}
		});

	}


	void SelectMode(){
		Transaction.Instance.ModeSelected.Fire (mode);


	}
}
