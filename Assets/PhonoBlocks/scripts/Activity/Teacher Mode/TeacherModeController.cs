using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherModeController : MonoBehaviour {

	void Start () {
		Dispatcher.Instance.OnModeSelected += (Mode mode) => {
			if(mode == Mode.TEACHER){
				Dispatcher.Instance.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter);
					Colorer.Instance.ReColor ();
				};

				Dispatcher.Instance.OnUserSubmittedTheirLetters += () => {
					Dispatcher.Instance.RecordUserAddedWordToHistory ();
				};
			}else {
				gameObject.SetActive(false);
			}
		};
	}
	

}
