using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class StudentActivityController : MonoBehaviour
{

		enum ActivityState
		{
				MAIN_ACTIVITY, //standard functionality (accept user input; update UI; update colors/error feedback)
		        FORCE_CORRECT_LETTER_PLACEMENT, //don't update the GUI letter images unless the inputted letter is correct. still show error feedback.
		        REMOVE_ALL_LETTERS //after the problem is completed & before going to next problem, force user to remove all the tangible letters they've placed on the platform
			
		}

		ActivityState state = ActivityState.MAIN_ACTIVITY;

		public void EnterForcedCorrectLetterPlacementMode(){
			state = ActivityState.FORCE_CORRECT_LETTER_PLACEMENT;

		}
		

		HintController hintController;
		ArduinoLetterController arduinoLetterController;


		public bool StringMatchesTarget (string s)
		{
			return s.Trim().Equals (State.Current.TargetWord.Trim ());

		}


		char[] usersMostRecentChanges; //array containing letters that a user has actually placed 
	    //on the platform. would not contain for example the initial letters that appear on the screen
		//before the user places them there themselves.


	    //save references to frequently used audio clips 
		AudioClip excellent;
		AudioClip incorrectSoundEffect;
		AudioClip notQuiteIt;
		AudioClip offerHint;
		AudioClip youDidIt;
		AudioClip correctSoundEffect;
		AudioClip removeAllLetters;
		AudioClip triumphantSoundForSessionDone;

		//todo refactor; should subscribe to appropriate events. SAC should publish appropriate events.
		GameObject submitWordButton;

	   
		public string UserChangesAsString {
				get {
					return new string (usersMostRecentChanges);
	
				}

		}

		bool allUserControlledLettersAreBlank(){
			return usersMostRecentChanges.Aggregate(true,(bool result, char nxt)=>result && nxt == ' ');
		}

		public void Initialize (GameObject hintButton, ArduinoLetterController arduinoLetterController)
		{
		        //set up dependency between arduino letter controller and this student activity controller.
		        //
				this.arduinoLetterController = arduinoLetterController;
				arduinoLetterController.studentActivityController = this;

				usersMostRecentChanges = new char[UserInputRouter.numOnscreenLetterSpaces];
		
			
				hintController = gameObject.GetComponent<HintController> ();
				hintController.Initialize (hintButton);

	
			    //cache audio clips
				excellent = InstructionsAudio.instance.excellent;
				incorrectSoundEffect = InstructionsAudio.instance.incorrectSoundEffect;
				notQuiteIt = InstructionsAudio.instance.notQuiteIt;
				offerHint = InstructionsAudio.instance.offerHint;
				youDidIt = InstructionsAudio.instance.youDidIt;
				correctSoundEffect = InstructionsAudio.instance.correctSoundEffect;
				removeAllLetters = InstructionsAudio.instance.removeAllLetters;

				triumphantSoundForSessionDone = InstructionsAudio.instance.allDoneSession;

		       //todo: refactor so that events are sent and the button disables itself.
				submitWordButton = GameObject.Find("CheckWordButton");


				SetUpNextProblem ();
		}

		
	public void SetUpNextProblem ()
		{  
			
				ClearSavedUserChanges ();
				hintController.Reset ();
			
				Problem currProblem = ProblemsRepository.instance.GetNextProblem ();
				Events.Dispatcher.SetTargetWord (currProblem.TargetWord(true));
				Events.Dispatcher.SetCurrentProblemInstructions (currProblem.Instructions);
				Events.Dispatcher.SetInitialProblemLetters (currProblem.InitialWord);
		        //save the new target word to the csv record for this acivity
				StudentsDataHandler.instance.RecordActivityTargetWord (currProblem.TargetWord (false));
	
		        

				PlayInstructions (); //dont bother telling to place initial letters during assessment mode

				state = ActivityState.MAIN_ACTIVITY;
				submitWordButton.SetActive (true);

		        

		}

	  

    
		public void PlayInstructions ()
		{
			
			AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);
			
				
		}

	   
		void ClearSavedUserChanges ()
		{
				for (int i=0; i<usersMostRecentChanges.Length; i++) {
						usersMostRecentChanges [i] = ' ';
					
				}

		}



		bool CurrentStateOfLettersMatches (string targetLetters)
		{       
				for (int i=0; i<usersMostRecentChanges.Length; i++) {
						if (i >= targetLetters.Length) {
								if (usersMostRecentChanges [i] != ' ')
										return false;
					} else if (targetLetters [i] != ' ' && usersMostRecentChanges [i] != targetLetters [i])
								return false;
				}
				return true;

		}
				

		void HandleEndOfActivity ()
		{
				if (ProblemsRepository.instance.AllProblemsDone ()) {
						StudentsDataHandler.instance.UpdateUserSessionAndWriteAllUpdatedDataToPlayerPrefs ();
						AudioSourceController.PushClip (triumphantSoundForSessionDone);
						
				} else {
						SetUpNextProblem ();
				}

		}
				

		public void UserRequestsHint ()
		{
		    hintController.ProvideHint ();

		}

	
		public void HandleNewArduinoLetter (char letter, int atPosition)
	{       	string previousUserInput = UserChangesAsString;
				RecordUsersChange (atPosition, letter); 
				switch (state) {
		case ActivityState.MAIN_ACTIVITY:
				arduinoLetterController.ChangeTheLetterOfASingleCell (atPosition, letter);
		
				Colorer.Instance.ReColor (UserChangesAsString,previousUserInput,State.Current.TargetWord);
					break;
		case ActivityState.FORCE_CORRECT_LETTER_PLACEMENT:
				InteractiveLetter asInteractiveLetter = State.Current.UILetters[atPosition];
				if (IsErroneous(atPosition, letter)) {
						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (Parameters.Colors.DEFAULT_OFF_COLOR);
						asInteractiveLetter.ConfigureFlashParameters (
							Parameters.Colors.DEFAULT_OFF_COLOR, Parameters.Colors.DEFAULT_ON_COLOR,
							Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON,
							Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER
						);

					} else {
						//in case the user removed a correct letter, then put it back; need to return the color to what it should be.
						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (State.Current.TargetWordColors[atPosition]);
					}
						break;
		case ActivityState.REMOVE_ALL_LETTERS:
					if (letter != ' ') //don't bother updating the letter unless the user removed it
						return;
			
					arduinoLetterController.ChangeTheLetterOfASingleCell (atPosition, letter);
					//once the user removes all letters from the current problem; automatically turn off the display image and go to the next activity.
					if(allUserControlledLettersAreBlank()){ 
						UserInputRouter.instance.RequestTurnOffImage ();
						HandleEndOfActivity ();
					}
						break;
					}
		}
		


			public bool IsErroneous(int atPosition, char letter){
				return (atPosition >= State.Current.TargetWord.Length && letter == ' ') ||
					(State.Current.TargetWord [atPosition] == letter);
		     }
		
		public void HandleSubmittedAnswer ()
		{     
		        
		StudentsDataHandler.instance.LogEvent ("submitted_answer", UserChangesAsString, State.Current.TargetWord);
					
				Events.Dispatcher.IncrementTimesAttemptedCurrentProblem ();
		
				if (IsSubmissionCorrect ()) {

					AudioSourceController.PushClip (correctSoundEffect);
					if (State.Current.TimesAttemptedCurrentProblem > 1)
						AudioSourceController.PushClip (youDidIt);
					else
						AudioSourceController.PushClip (excellent);
					AudioSourceController.PushClip (AudioSourceController.GetWordFromResources(State.Current.TargetWord));
					CurrentProblemCompleted (true);
					
				} else {
					HandleIncorrectAnswer ();				
					
				}


		}

		protected void HandleIncorrectAnswer ()
		{
		       
				AudioSourceController.PushClip (incorrectSoundEffect);
				AudioSourceController.PushClip (notQuiteIt);
			    //allow the user to access a hint.
				switch(state){
			case ActivityState.MAIN_ACTIVITY:
					hintController.ActivateHintButton ();
					AudioSourceController.PushClip (offerHint);
					break;
			case ActivityState.FORCE_CORRECT_LETTER_PLACEMENT:
					break;
			
				}

		}

		public void CurrentProblemCompleted (bool userSubmittedCorrectAnswer)
		{
			
			   
				UserInputRouter.instance.AddCurrentWordToHistory (false);
				UserInputRouter.instance.RequestDisplayImage (State.Current.TargetWord, false, true);

				bool solvedOnFirstTry = State.Current.TimesAttemptedCurrentProblem == 1;
				if (solvedOnFirstTry) {
		
						UserInputRouter.instance.DisplayNewStarOnScreen (ProblemsRepository.instance.ProblemsCompleted-1);

				}
			
				StudentsDataHandler.instance.RecordActivitySolved (userSubmittedCorrectAnswer, UserChangesAsString, solvedOnFirstTry);
			
		        StudentsDataHandler.instance.SaveActivityDataAndClearForNext (State.Current.TargetWord, State.Current.InitialTargetLetters);
		        
		     
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				state = ActivityState.REMOVE_ALL_LETTERS;
				//disable submit button; we automatically go to next problem when user removes all letters. 
				submitWordButton.SetActive (false);
      
		}

		public void RecordUsersChange (int position, char change)
		{
		
				usersMostRecentChanges [position] = change;

		
		}

		public bool IsSubmissionCorrect ()
		{      
				string target = State.Current.TargetWord;

				bool result = CurrentStateOfLettersMatches (target);

				
				return result;

		}



}
