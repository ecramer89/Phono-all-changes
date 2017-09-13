using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherMode : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Events.Dispatcher.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			ArduinoLetterController.instance.ChangeTheLetterOfASingleCell(atPosition, newLetter);
			Colorer.Instance.ReColor();
		};

		Events.Dispatcher.OnUserSubmittedTheirLetters += () => {
			Events.Dispatcher.RecordUserAddedWordToHistory();

		};
		
	}
	

}
