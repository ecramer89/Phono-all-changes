using UnityEngine;
using System.Collections;
using System.Text;

public class HintController : MonoBehaviour
{
		StudentActivityController studentActivityController;
		public AudioClip sound_out_word;
		GameObject hintButton;
		int currHintIdx = -1;
		public const int NUM_HINTS = 2;

	    //for guided letter placement mode
		string targetLetters="";
	    public int NumTargetLetters{
		get { return targetLetters.Length;}
		}
		string blanks="";
		AudioClip[] targetLetterSounds;
		int targetLetterIndex=0;
		public int TargetLetterIndex {
				get { return targetLetterIndex;}
		}

		public void Initialize (GameObject hintButton)
		{
				this.hintButton = hintButton;
				studentActivityController = gameObject.GetComponent<StudentActivityController> ();
				sound_out_word = InstructionsAudio.instance.soundOutTheWord;
		}

		public void Reset ()
		{
				currHintIdx = -1;
				targetLetters = "";
				targetLetterSounds = null;
				targetLetterIndex=0;
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
	    
			switch (currHintIdx) {
					case 0:
							currProblem.PlaySoundedOutWord ();
							break;

				case 1: //level two hint

					UserInputRouter.instance.BlockAllUIInput ();
					ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
					StartCoroutine (PresentTargetLettersAndSoundsOneAtATime());
					break;
				}
			

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
						LetterSoundComponent placeInGrid = studentActivityController.GetTargetLetterSoundComponentFor (letterindex);
						ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, studentActivityController.TargetLetters [letterindex]);
						ArduinoLetterController.instance.ChangeDisplayColourOfASingleCell (letterindex, placeInGrid.GetColour ());
						yield return new WaitForSeconds (Parameters.Hints.LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER);
					}
				}
				UserInputRouter.instance.UnBlockAllUIInput();
				ArduinoLetterController.instance.PlaceWordInLetterGrid (studentActivityController.UserChangesAsString);
				ArduinoLetterController.instance.RevertLettersToDefaultColour ();
			}

		   /*public void DisplayAndPlaySoundOfCurrentTargetLetter(){
		   if (targetLetterIndex < targetLetters.Length) {
						string next = targetLetters.Substring (0, targetLetterIndex + 1);
						UserInputRouter.instance.DisplayLettersOf (next);
						string pathTo = "audio/sounded_out_words/" + targetLetters + "/" + targetLetters [targetLetterIndex];
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
				}
			}*/

		public void AdvanceTargetLetter(){
			targetLetterIndex++;
		}

		public bool UsedLastHint ()
		{
				return currHintIdx == NUM_HINTS;
		}
	
		public bool OnLastHint ()
		{
		
				return currHintIdx == NUM_HINTS - 1;
		}

		public void AdvanceHint ()
		{

				if (currHintIdx < NUM_HINTS)
						currHintIdx++;
		}


}
