using UnityEngine;
using System.Collections;
using System.Text;
using Extensions;
using System.Text.RegularExpressions;
public class HintController : MonoBehaviour
{
	

		public void Start ()
		{
				Dispatcher.Instance.OnHintRequested += ProvideHint;
		}


		public void ProvideHint ()
		{      
			if (Dispatcher._State.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (Dispatcher._State.CurrentHintNumber) {
				case Parameters.Hints.Descriptions.
					PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE: 
						Dispatcher.Instance.LockUIInput();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (
							Dispatcher._State.Activity == Activity.SYLLABLE_DIVISION ? 
								PresentTargetSyllablesAndSyllableSoundsOneAtATime() :
								PresentTargetLettersAndSoundsOneAtATime()
						);
					break;

				case Parameters.Hints.Descriptions.
					PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT:
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (Dispatcher._State.TargetWord));
						    //place the target letters and colors in the grid
						for (int letterIndex=0; letterIndex < Parameters.UI.ONSCREEN_LETTER_SPACES; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (
								letterIndex, 
								Dispatcher._Selector.TargetWordWithBlanksForUnusedPositions [letterIndex]);
							Colorer.ChangeDisplayColourOfASingleLetter (letterIndex, Dispatcher._State.TargetWordColors [letterIndex]);
						}
						Dispatcher.Instance.ForceCorrectLetterPlacement ();
					break;

					default: //level 1 hint, and whatever would happen should number of hints exceed 3.
						AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (Dispatcher._State.TargetWord));
						break;
				
				}

			
				Dispatcher.Instance.RecordHintProvided ();

		}



	IEnumerator PresentTargetSyllablesAndSyllableSoundsOneAtATime(){
		int syllableIndex = -1;
		int numSyllables = Dispatcher._State.TargetWordSyllables.Count;
		while(true){
			syllableIndex++;
			if (syllableIndex > numSyllables) break;
			if (syllableIndex == numSyllables)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
			else {
				Match targetSyllable = Dispatcher._State.TargetWordSyllables[syllableIndex];
				for(int i=0;i<targetSyllable.Value.Length;i++){
					int indexOfLetterInTargetWord = targetSyllable.Index+i;
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (indexOfLetterInTargetWord, targetSyllable.Value[i]);
					Colorer.ChangeDisplayColourOfASingleLetter (indexOfLetterInTargetWord, Dispatcher._State.TargetWordColors[indexOfLetterInTargetWord]);
				}
				//todo, when record the syllables- put them here
				//string pathTo = $"audio/sounded_out_syllables/{targetWord}/{targetWord[syllableIndex]}";
				//AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
				//AudioSourceController.PushClip (targetSound);
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
			}
		}
		Dispatcher.Instance.UnLockUIInput ();
		ArduinoLetterController.instance.PlaceWordInLetterGrid (
			Dispatcher._State.UserInputLetters.Union(Dispatcher._State.PlaceHolderLetters)
		);
		Dispatcher._State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 

	}

			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = -1;
				int numLetters = Dispatcher._State.TargetWord.Length;
				while(true){
					letterindex++;
					if (letterindex > numLetters) break;
					if (letterindex == numLetters)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
					else {
						string targetWord = Dispatcher._State.TargetWord;

						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, targetWord[letterindex]);
						Colorer.ChangeDisplayColourOfASingleLetter (letterindex, Dispatcher._State.TargetWordColors[letterindex]);
						string pathTo = $"audio/sounded_out_words/{targetWord}/{targetWord[letterindex]}";
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				Dispatcher.Instance.UnLockUIInput ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (
						Dispatcher._State.UserInputLetters.Union(Dispatcher._State.PlaceHolderLetters)
				);
				Dispatcher._State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
