using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherMode : MonoBehaviour {

	void Start () {
		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			if(mode == Mode.TEACHER){
				Events.Dispatcher.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter);
					Colorer.Instance.ReColor ();
				};

				Events.Dispatcher.OnUserSubmittedTheirLetters += () => {
					Events.Dispatcher.RecordUserAddedWordToHistory ();
				};
			}else {
				gameObject.SetActive(false);
			}
		};
	}
	

}
