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
			if (State.Current.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (State.Current.CurrentHintNumber) {
				case Parameters.Hints.Descriptions.
					PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE: 
						Dispatcher.Instance.LockUIInput();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (
							State.Current.Activity == Activity.SYLLABLE_DIVISION ? 
								PresentTargetSyllablesAndSyllableSoundsOneAtATime() :
								PresentTargetLettersAndSoundsOneAtATime()
						);
					break;

				case Parameters.Hints.Descriptions.
					PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT:
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (State.Current.TargetWord));
						    //place the target letters and colors in the grid
						for (int letterIndex=0; letterIndex < Parameters.UI.ONSCREEN_LETTER_SPACES; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (
								letterIndex, 
								Selector.Instance.TargetWordWithBlanksForUnusedPositions [letterIndex]);
							Colorer.ChangeDisplayColourOfASingleLetter (letterIndex, State.Current.TargetWordColors [letterIndex]);
						}
						Dispatcher.Instance.ForceCorrectLetterPlacement ();
					break;

					default: //level 1 hint, and whatever would happen should number of hints exceed 3.
						AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (State.Current.TargetWord));
						break;
				
				}

			
				Dispatcher.Instance.RecordHintProvided ();

		}



	IEnumerator PresentTargetSyllablesAndSyllableSoundsOneAtATime(){
		int syllableIndex = -1;
		int numSyllables = State.Current.TargetWordSyllables.Count;
		while(true){
			syllableIndex++;
			if (syllableIndex > numSyllables) break;
			if (syllableIndex == numSyllables)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
			else {
				Match targetSyllable = State.Current.TargetWordSyllables[syllableIndex];
				for(int i=0;i<targetSyllable.Value.Length;i++){
					int indexOfLetterInTargetWord = targetSyllable.Index+i;
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (indexOfLetterInTargetWord, targetSyllable.Value[i]);
					Colorer.ChangeDisplayColourOfASingleLetter (indexOfLetterInTargetWord, State.Current.TargetWordColors[indexOfLetterInTargetWord]);
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
			State.Current.UserInputLetters.Union(State.Current.PlaceHolderLetters)
		);
		State.Current.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 

	}

			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = -1;
				int numLetters = State.Current.TargetWord.Length;
				while(true){
					letterindex++;
					if (letterindex > numLetters) break;
					if (letterindex == numLetters)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
					else {
						string targetWord = State.Current.TargetWord;

						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, targetWord[letterindex]);
						Colorer.ChangeDisplayColourOfASingleLetter (letterindex, State.Current.TargetWordColors[letterindex]);
						string pathTo = $"audio/sounded_out_words/{targetWord}/{targetWord[letterindex]}";
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				Dispatcher.Instance.UnLockUIInput ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (
						State.Current.UserInputLetters.Union(State.Current.PlaceHolderLetters)
				);
				State.Current.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
