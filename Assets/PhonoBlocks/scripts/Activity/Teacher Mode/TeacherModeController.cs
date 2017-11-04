using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherModeController : PhonoBlocksSubscriber {


	public override void SubscribeToAll(PhonoBlocksScene scene){
			if(scene == PhonoBlocksScene.Activity){
				if(Transaction.Instance.State.Mode != Mode.TEACHER) {
					gameObject.SetActive(false); 
					return; 
				}

				Transaction.Instance.UserEnteredNewLetter.Subscribe(this,(char newLetter, int atPosition) => {
				ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, newLetter, LetterImageTable.instance.GetLetterImageFromLetter);
					Colorer.Instance.ReColor ();
				});

				Transaction.Instance.UserSubmittedTheirLetters.Subscribe(this,() => {
					Transaction.Instance.UserAddedWordToHistory.Fire ();
				});
				
		
			Transaction.Instance.WordColorShowStateSet.Subscribe(this, (WordColorShowStates newWordState)=>{

			
				string trimmedCurrentUserWord=Transaction.Instance.State.UserInputLetters.Trim(); 
				AudioClip clip = null;
				AudioClip alternative = null;
				if(newWordState == WordColorShowStates.SHOW_TARGET_UNITS){
					clip = AudioSourceController.GetSoundedOutWordFromResources(trimmedCurrentUserWord);
					alternative = InstructionsAudio.instance.trySoundingOutWordYourself;
				} else {
					clip= AudioSourceController.GetWordFromResources(trimmedCurrentUserWord);
					alternative = InstructionsAudio.instance.tryReadingWholeWordYourself;
				}
		
				AudioSourceController.PushClip(clip == null ? alternative : clip);
			});   

		}
	}
}
