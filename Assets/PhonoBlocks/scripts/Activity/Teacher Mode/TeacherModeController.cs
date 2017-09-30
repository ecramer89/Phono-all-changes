using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherModeController : PhonoBlocksSubscriber {
	public override void SubscribeToAll(PhonoBlocksScene scene){
		if(scene == PhonoBlocksScene.MainMenu){

			Transaction.Instance.ModeSelected.Subscribe(this,
				(Mode mode) => {
					if(mode != Mode.TEACHER) gameObject.SetActive(false); 
				}
			);
		}

			if(scene == PhonoBlocksScene.Activity){
				if(Transaction.Instance.State.Mode != Mode.TEACHER) return; 

				Transaction.Instance.UserEnteredNewLetter.Subscribe(this,(char newLetter, int atPosition) => {
				ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter, LetterImageTable.instance.GetLetterImageFromLetter);
					Colorer.Instance.ReColor ();
				});

				Transaction.Instance.UserSubmittedTheirLetters.Subscribe(this,() => {
					Transaction.Instance.UserAddedWordToHistory.Fire ();
				});

		}
	}
	/*void Start () {
		Transaction.Instance.ModeSelected.Subscribe(this,
			(Mode mode) => {
				if(mode == Mode.TEACHER){
					Transaction.Instance.UserEnteredNewLetter.Subscribe(this,(char newLetter, int atPosition) => {
						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter);
						Colorer.Instance.ReColor ();
					});

					Transaction.Instance.UserSubmittedTheirLetters.Subscribe(this,() => {
						Transaction.Instance.UserAddedWordToHistory.Fire ();
					});
				}else {
					gameObject.SetActive(false);
				}
			}
		);

}*/
}
