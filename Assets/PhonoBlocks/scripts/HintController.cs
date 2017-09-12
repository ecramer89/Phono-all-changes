using UnityEngine;
using System.Collections;
using System.Text;

public class HintController : MonoBehaviour
{
	

		public void Initialize ()
		{
				Events.Dispatcher.OnHintRequested += ProvideHint;
		}


		public void ProvideHint ()
		{      
			if (State.Current.CurrentHintNumber >= Parameters.Hints.NUM_HINTS)
				return;

			switch (State.Current.CurrentHintNumber) {
				    case 1: //level two hint
						Events.Dispatcher.LockUIInput();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (PresentTargetLettersAndSoundsOneAtATime());
					break;

					case 2: //level three hint
						AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (State.Current.TargetWord));
						UserInputRouter.instance.RequestDisplayImage (State.Current.TargetWord, false, true);
						    //place the target letters and colors in the grid
						for (int letterIndex = 0; letterIndex < State.Current.TargetWord.Length; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterIndex, State.Current.TargetWord [letterIndex]);
							ArduinoLetterController.instance.ChangeDisplayColourOfASingleCell (letterIndex, State.Current.TargetWordColors [letterIndex]);
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
						ArduinoLetterController.instance.ChangeDisplayColourOfASingleCell (letterindex, State.Current.TargetWordColors[letterindex]);
						string pathTo = $"audio/sounded_out_words/{targetWord}/{targetWord[letterindex]}";
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				Events.Dispatcher.UnLockUIInput ();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (State.Current.UserInputLetters);
				State.Current.UILetters.ForEach(UILetter=>UILetter.RevertToInputDerivedColor ()); 
			}





}
