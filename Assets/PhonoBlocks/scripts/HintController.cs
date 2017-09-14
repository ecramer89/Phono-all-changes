using UnityEngine;
using System.Collections;
using System.Text;
using Extensions;

public class HintController : MonoBehaviour
{
	

		public void Start ()
		{
				Events.Dispatcher.OnHintRequested += ProvideHint;
		}


		public void ProvideHint ()
		{      
			if (State.Current.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (State.Current.CurrentHintNumber) {
				case Parameters.Hints.Descriptions.
					PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE: 
						Events.Dispatcher.LockUIInput();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (PresentTargetLettersAndSoundsOneAtATime());
					break;

				case Parameters.Hints.Descriptions.
					PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT:
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (State.Current.TargetWord));
						    //place the target letters and colors in the grid
						for (int letterIndex = 0; letterIndex < State.Current.TargetWord.Length; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterIndex, State.Current.TargetWord [letterIndex]);
							Colorer.ChangeDisplayColourOfASingleLetter (letterIndex, State.Current.TargetWordColors [letterIndex]);
						}
						Events.Dispatcher.ForceCorrectLetterPlacement ();
					break;

					default: //level 1 hint, and whatever would happen should number of hints exceed 3.
						AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (State.Current.TargetWord));
						break;
				
				}

			
				Events.Dispatcher.RecordHintProvided ();

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
				Events.Dispatcher.UnLockUIInput ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (
						State.Current.UserInputLetters.Union(State.Current.PlaceHolderLetters)
				);
				State.Current.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
