using UnityEngine;
using System.Collections;
using System.Text;
using Extensions;
using System.Text.RegularExpressions;
public class HintController : MonoBehaviour
{
	

		public void Start ()
		{
			Transaction.Instance.HintRequested.Subscribe(ProvideHint);
		}


		public void ProvideHint ()
		{      
			if (Transaction.State.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (Transaction.State.CurrentHintNumber) {
				case Parameters.Hints.Descriptions.
					PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE: 
					Transaction.Instance.UIInputLocked.Fire();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (
							Transaction.State.Activity == Activity.SYLLABLE_DIVISION ? 
								PresentTargetSyllablesAndSyllableSoundsOneAtATime() :
								PresentTargetLettersAndSoundsOneAtATime()
						);
					break;

				case Parameters.Hints.Descriptions.
					PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT:
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (Transaction.State.TargetWord));
						    //place the target letters and colors in the grid
						for (int letterIndex=0; letterIndex < Parameters.UI.ONSCREEN_LETTER_SPACES; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (
								letterIndex, 
								Transaction.Selector.TargetWordWithBlanksForUnusedPositions [letterIndex]);
							Colorer.ChangeDisplayColourOfASingleLetter (letterIndex, Transaction.State.TargetWordColors [letterIndex]);
						}
						Transaction.Instance.StudentModeForceCorrectLetterPlacementEntered.Fire ();
					break;

					default: //level 1 hint, and whatever would happen should number of hints exceed 3.
						AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (Transaction.State.TargetWord));
						break;
				
				}

			
				Transaction.Instance.HintProvided.Fire ();

		}



	IEnumerator PresentTargetSyllablesAndSyllableSoundsOneAtATime(){
		int syllableIndex = -1;
		int numSyllables = Transaction.State.TargetWordSyllables.Count;
		while(true){
			syllableIndex++;
			if (syllableIndex > numSyllables) break;
			if (syllableIndex == numSyllables)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
			else {
				Match targetSyllable = Transaction.State.TargetWordSyllables[syllableIndex];
				for(int i=0;i<targetSyllable.Value.Length;i++){
					int indexOfLetterInTargetWord = targetSyllable.Index+i;
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (indexOfLetterInTargetWord, targetSyllable.Value[i]);
					Colorer.ChangeDisplayColourOfASingleLetter (indexOfLetterInTargetWord, Transaction.State.TargetWordColors[indexOfLetterInTargetWord]);
				}
				//todo, when record the syllables- put them here
				//string pathTo = $"audio/sounded_out_syllables/{targetWord}/{targetWord[syllableIndex]}";
				//AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
				//AudioSourceController.PushClip (targetSound);
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
			}
		}
		Transaction.Instance.UIInputUnLocked.Fire ();
		ArduinoLetterController.instance.PlaceWordInLetterGrid (
			Transaction.State.UserInputLetters.Union(Transaction.State.PlaceHolderLetters)
		);
		Transaction.State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 

	}

			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = -1;
				int numLetters = Transaction.State.TargetWord.Length;
				while(true){
					letterindex++;
					if (letterindex > numLetters) break;
					if (letterindex == numLetters)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
					else {
						string targetWord = Transaction.State.TargetWord;

						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, targetWord[letterindex]);
						Colorer.ChangeDisplayColourOfASingleLetter (letterindex, Transaction.State.TargetWordColors[letterindex]);
						string pathTo = $"audio/sounded_out_words/{targetWord}/{targetWord[letterindex]}";
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				Transaction.Instance.UIInputUnLocked.Fire ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (
						Transaction.State.UserInputLetters.Union(Transaction.State.PlaceHolderLetters)
				);
				Transaction.State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
