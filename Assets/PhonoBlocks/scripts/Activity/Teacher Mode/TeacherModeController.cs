using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherModeController : MonoBehaviour {

	void Start () {
		Dispatcher.Instance.ModeSelected.Subscribe(
			(Mode mode) => {
				if(mode == Mode.TEACHER){
					Dispatcher.Instance.UserEnteredNewLetter.Subscribe((char newLetter, int atPosition) => {
						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter);
						Colorer.Instance.ReColor ();
					});

					Dispatcher.Instance.UserSubmittedTheirLetters.Subscribe(() => {
						Dispatcher.Instance.UserAddedWordToHistory.Fire ();
					});
				}else {
					gameObject.SetActive(false);
				}
			}
		);

}
}
