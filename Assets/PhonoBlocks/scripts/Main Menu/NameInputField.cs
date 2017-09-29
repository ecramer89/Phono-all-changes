using UnityEngine;
using System.Collections;

public class NameInputField : PhonoBlocksSubscriber {
	static string placeholder="name or name*";
	string name=placeholder;
	Rect position;
	bool enterKeyPressed;
	public string Name{
		get {
			return name;
		}
	}

	public override void SubscribeToAll(PhonoBlocksScene forScene){}


	//becomes active when student mode selected. otherwise inactive.
	void Start(){
		position = new Rect (300, 300, 200, 50);
		gameObject.SetActive (false);
		Transaction.Instance.ModeSelected.Subscribe(this,(Mode mode) => {
			if(mode == Mode.STUDENT){
				gameObject.SetActive(true);
			}
		});
		Transaction.Instance.StudentDataRetrieved.Subscribe(this,() => {
			gameObject.SetActive(false);
		});

	}

	/*
	 * The text input field needs to de activate once the student data is retrieved.
	 * since the text input field is what publishes the student name entered event,
	 * it sets off a chain that results in the data handler publishing the student data retrieved event.
	 * unity threw exception when I dispatched student name entered from OnGUI,
	 * supposedly because it traced the input field's call to setActive(false) back to onGUI
	 * as such, necessary to dispatch the event that ultimately triggers input field to set itself inactive
	 * from Update. So to workaround, instead of dispatching event directly from OnGUI, 
	 * just set a flag that causes Update method to dispatch the event the next time it is called.
	 * */

	void OnGUI ()
	{
		//using OnGUI because Unity's built in UI prefabs don't work very well with NGUI.
		//OnGUI also seemed to interfere with Update's use of the Input.KeyDown query,
		//so I query the Event within OnGUI instead to tell when user presses enter key.
		name = GUI.TextField (position, name, 25);
		//submit name when user hits enter key.
		if (Event.current.keyCode == KeyCode.Return && name.Length > 0 && name != placeholder) {
			enterKeyPressed = true;
		}

	}

	void Update(){
		if (enterKeyPressed) {
			Transaction.Instance.StudentNameEntered.Fire (name);
			enterKeyPressed = false;
		}
	}


}
