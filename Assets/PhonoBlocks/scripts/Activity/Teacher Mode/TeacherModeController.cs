using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherModeController : MonoBehaviour {

	void Start () {
		Transaction.Instance.ModeSelected.Subscribe(
			(Mode mode) => {
				if(mode == Mode.TEACHER){
					Transaction.Instance.UserEnteredNewLetter.Subscribe((char newLetter, int atPosition) => {
						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter);
						Colorer.Instance.ReColor ();
					});

					Transaction.Instance.UserSubmittedTheirLetters.Subscribe(() => {
						Transaction.Instance.UserAddedWordToHistory.Fire ();
					});
				}else {
					gameObject.SetActive(false);
				}
			}
		);

}
}
