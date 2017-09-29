using UnityEngine;
using System.Collections;
using System.Text;
using Extensions;
using System.Text.RegularExpressions;
public class HintController : PhonoBlocksSubscriber
{
	public override void SubscribeToAll(PhonoBlocksScene scene){}


		public void Start ()
		{
			Transaction.Instance.HintRequested.Subscribe(this, ProvideHint);
		}


		public void ProvideHint ()
		{      
			if (Transaction.Instance.State.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (Transaction.Instance.State.CurrentHintNumber) {
				case Parameters.Hints.Descriptions.
					PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE: 
					Transaction.Instance.UIInputLocked.Fire();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (
							Transaction.Instance.State.Activity == Activity.SYLLABLE_DIVISION ? 
								PresentTargetSyllablesAndSyllableSoundsOneAtATime() :
								PresentTargetLettersAndSoundsOneAtATime()
						);
					break;

				case Parameters.Hints.Descriptions.
					PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT:
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (Transaction.Instance.State.TargetWord));
						    //place the target letters and colors in the grid
						for (int letterIndex=0; letterIndex < Parameters.UI.ONSCREEN_LETTER_SPACES; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (
								letterIndex, 
								Transaction.Instance.Selector.TargetWordWithBlanksForUnusedPositions [letterIndex]);
							Colorer.ChangeDisplayColourOfASingleLetter (letterIndex, Transaction.Instance.State.TargetWordColors [letterIndex]);
						}
						Transaction.Instance.StudentModeForceCorrectLetterPlacementEntered.Fire ();
					break;

					default: //level 1 hint, and whatever would happen should number of hints exceed 3.
						AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (Transaction.Instance.State.TargetWord));
						break;
				
				}

			
				Transaction.Instance.HintProvided.Fire ();

		}



	IEnumerator PresentTargetSyllablesAndSyllableSoundsOneAtATime(){
		int syllableIndex = -1;
		int numSyllables = Transaction.Instance.State.TargetWordSyllables.Count;
		while(true){
			syllableIndex++;
			if (syllableIndex > numSyllables) break;
			if (syllableIndex == numSyllables)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
			else {
				Match targetSyllable = Transaction.Instance.State.TargetWordSyllables[syllableIndex];
				for(int i=0;i<targetSyllable.Value.Length;i++){
					int indexOfLetterInTargetWord = targetSyllable.Index+i;
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (indexOfLetterInTargetWord, targetSyllable.Value[i]);
					Colorer.ChangeDisplayColourOfASingleLetter (indexOfLetterInTargetWord, Transaction.Instance.State.TargetWordColors[indexOfLetterInTargetWord]);
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
			Transaction.Instance.State.UserInputLetters.Union(Transaction.Instance.State.PlaceHolderLetters)
		);
		Transaction.Instance.State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 

	}

			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = -1;
				int numLetters = Transaction.Instance.State.TargetWord.Length;
				while(true){
					letterindex++;
					if (letterindex > numLetters) break;
					if (letterindex == numLetters)
				yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD);
					else {
						string targetWord = Transaction.Instance.State.TargetWord;

						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, targetWord[letterindex]);
						Colorer.ChangeDisplayColourOfASingleLetter (letterindex, Transaction.Instance.State.TargetWordColors[letterindex]);
						string pathTo = $"audio/sounded_out_words/{targetWord}/{targetWord[letterindex]}";
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				Transaction.Instance.UIInputUnLocked.Fire ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (
						Transaction.Instance.State.UserInputLetters.Union(Transaction.Instance.State.PlaceHolderLetters)
				);
				Transaction.Instance.State.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
