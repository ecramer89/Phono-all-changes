using UnityEngine;
using System.Collections;

public class NameInputField : MonoBehaviour {
	static string placeholder="name or name*";
	string name=placeholder;
	public string Name{
		get {
			return name;
		}
	}
	//becomes active when student mode selected. otherwise inactive.
	void Start(){
		gameObject.SetActive (false);
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			if(mode == Mode.STUDENT){
				gameObject.SetActive(true);
			}
		};

	}


	void OnGUI ()
	{

		name = GUI.TextField (new Rect (gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 200, 20), name, 25);
		//submit name when user hits enter key.
		if (Event.current.keyCode == KeyCode.Return && name.Length > 0 && name != placeholder) {
			Events.Dispatcher.RecordStudentNameEntered (name);

		}

	}


}
