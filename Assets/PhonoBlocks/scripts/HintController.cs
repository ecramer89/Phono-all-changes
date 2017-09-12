using UnityEngine;
using System.Collections;
using System.Text;

public class HintController : MonoBehaviour
{
		StudentActivityController studentActivityController;
		public AudioClip sound_out_word;
		GameObject hintButton;
		int currHintIdx = 0;
		const int NUM_HINTS=3;

		public void Initialize (GameObject hintButton)
		{
				this.hintButton = hintButton;
				studentActivityController = gameObject.GetComponent<StudentActivityController> ();
				sound_out_word = InstructionsAudio.instance.soundOutTheWord;
		}

		public void Reset ()
		{
				currHintIdx = 0;
	
		}

		public void DeActivateHintButton ()
		{

				hintButton.SetActive (false);
		}

		public void ActivateHintButton ()
		{

				if (!hintButton.activeSelf)
						hintButton.SetActive (true);

		}

		public bool HintButtonActive ()
		{
				return hintButton.activeSelf;

		}


		public void ProvideHint (Problem currProblem)
		{      
			if (currHintIdx >= NUM_HINTS)
				return;


			switch (currHintIdx) {
					case 0:
							currProblem.PlaySoundedOutWord ();
					break;

				    case 1: //level two hint
						UserInputRouter.instance.BlockAllUIInput ();
						ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
						StartCoroutine (PresentTargetLettersAndSoundsOneAtATime());
					break;

					case 2: //level three hint
						currProblem.PlayAnswer ();
						UserInputRouter.instance.RequestDisplayImage (currProblem.TargetWord (true), false, true);
					    //place the target letters and colors in the grid
						for (int letterIndex = 0; letterIndex < State.Current.TargetWord.Length; letterIndex++) {
							ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterIndex, State.Current.TargetWord [letterIndex]);
							ArduinoLetterController.instance.ChangeDisplayColourOfASingleCell (letterIndex, State.Current.TargetWordColors[letterIndex]);
						}
						studentActivityController.EnterForcedCorrectLetterPlacementMode ();
					break;
				}

				currHintIdx++;
				DeActivateHintButton ();
			

				StudentsDataHandler.instance.LogEvent ("requested_hint", currHintIdx + "", "NA");
			
		}


			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = -1;
				int numLetters = studentActivityController.TargetLetters.Length;
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
				UserInputRouter.instance.UnBlockAllUIInput();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (studentActivityController.UserChangesAsString);
				ArduinoLetterController.instance.RevertLettersToDefaultColour ();
			}





}
