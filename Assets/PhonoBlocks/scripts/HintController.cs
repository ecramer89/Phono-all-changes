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
		{      //maybe make this a couroutine that can "iterate" thru each hint step
				//each of which is an audio file, except for the last which involves a visual change as wellS
				
		//userInputRouter.DisplayLettersOf (currProblem.TargetWord (false));

		       
				switch (currHintIdx) {
				case 0:
						currProblem.PlaySoundedOutWord ();
						//AudioSourceController.PushClip (sound_out_word);
						break;

		case 1: //level two hint

			UserInputRouter.instance.BlockAllUIInput ();
			ArduinoLetterController.instance.ReplaceEachLetterWithBlank ();
			StartCoroutine (PresentTargetLettersAndSoundsOneAtATime());


						//currProblem.PlaySoundedOutWord ();

			/*
						studentActivityController.EnterGuidedLetterPlacementMode();
						targetLetters = studentActivityController.TargetLetters;
						StringBuilder blanksSB = new StringBuilder();
						for(int i=0;i<targetLetters.Length;i++){
							blanksSB.Append(" ");
						}
						UserInputRouter.instance.DisplayLettersOf(blanksSB.ToString());
						targetLetterIndex = 0;
						studentActivityController.SkipToNextLetterToHint();*/
						break;
				}
			

				StudentsDataHandler.instance.LogEvent ("requested_hint", currHintIdx + "", "NA");
			
		}



			IEnumerator PresentTargetLettersAndSoundsOneAtATime(){
				int letterindex = 0;
				while(letterindex < studentActivityController.TargetLetters.Length){
					LetterSoundComponent placeInGrid = studentActivityController.GetTargetLetterSoundComponentFor (letterindex);
					ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (letterindex, studentActivityController.TargetLetters[letterindex]);
					ArduinoLetterController.instance.ChangeDisplayColourOfASingleCell (letterindex, placeInGrid.GetColour ());
					letterindex++;
					yield return new WaitForSeconds (letterindex < studentActivityController.TargetLetters.Length - 1 ? 2 : 5 );
				}
		          
				UserInputRouter.instance.UnBlockAllUIInput();
				//ArduinoLetterController.instance.
				ArduinoLetterController.instance.RevertLettersToDefaultColour ();
			}

		   public void DisplayAndPlaySoundOfCurrentTargetLetter(){
		   if (targetLetterIndex < targetLetters.Length) {
						string next = targetLetters.Substring (0, targetLetterIndex + 1);
						UserInputRouter.instance.DisplayLettersOf (next);
						string pathTo = "audio/sounded_out_words/" + targetLetters + "/" + targetLetters [targetLetterIndex];
						AudioClip targetSound = AudioSourceController.GetClipFromResources (pathTo);
						AudioSourceController.PushClip (targetSound);
				}
			}

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
