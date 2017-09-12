using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class StudentActivityController : MonoBehaviour
{

		HintController hintController;
		ArduinoLetterController arduinoLetterController;

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


		public void Initialize (GameObject hintButton, ArduinoLetterController arduinoLetterController)
		{
				Events.Dispatcher.OnUserEnteredNewLetter += HandleNewArduinoLetter;
				Events.Dispatcher.OnUserSubmittedTheirLetters += HandleSubmittedAnswer;

			
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


	bool allUserControlledLettersAreBlank(){
		return State.Current.UserInputLetters.Aggregate(true,(bool result, char nxt)=>result && nxt == ' ');
	}


	void MainActivityNewLetterHandler(char letter, int atPosition){
		ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, letter);
		Colorer.Instance.ReColor ();
	}

	void ForceCorrectPlacementNewLetterHandler(char letter, int atPosition){
		InteractiveLetter asInteractiveLetter = State.Current.UILetters[atPosition];
		if (IsErroneous(atPosition, letter)) {
			asInteractiveLetter.UpdateInputDerivedAndDisplayColor (Parameters.Colors.DEFAULT_OFF_COLOR);
			asInteractiveLetter.ConfigureFlashParameters (
				Parameters.Colors.DEFAULT_OFF_COLOR, Parameters.Colors.DEFAULT_ON_COLOR,
				Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON,
				Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER
			);
			return;
		}

		//in case the user removed a correct letter, then put it back; need to return the color to what it should be.
		asInteractiveLetter.UpdateInputDerivedAndDisplayColor (State.Current.TargetWordColors[atPosition]);

	}

	void RemoveAllLettersNewLetterHandler(char letter, int atPosition){
		if (letter != ' ') //don't bother updating the letter unless the user removed it
			return;

		arduinoLetterController.ChangeTheLetterOfASingleCell (atPosition, letter);
		//once the user removes all letters from the current problem; automatically turn off the display image and go to the next activity.
		if(allUserControlledLettersAreBlank()){ 
			UserInputRouter.instance.RequestTurnOffImage ();
			HandleEndOfActivity ();
		}
	}


	void HandleNewArduinoLetter(char newLetter, int atPosition){
		switch (State.Current.ActivityState) {
		case ActivityStates.MAIN_ACTIVITY:
			MainActivityNewLetterHandler(newLetter, atPosition);
			break;
		case ActivityStates.FORCE_CORRECT_LETTER_PLACEMENT:
			ForceCorrectPlacementNewLetterHandler(newLetter, atPosition);
			break;
		case ActivityStates.REMOVE_ALL_LETTERS:
			RemoveAllLettersNewLetterHandler(newLetter, atPosition);
			break;
		}
	}

		
	 void SetUpNextProblem ()
		{  
			
				hintController.Reset ();
			
				Problem currProblem = ProblemsRepository.instance.GetNextProblem ();
				Events.Dispatcher.SetTargetWord (currProblem.TargetWord(true));
				Events.Dispatcher.SetCurrentProblemInstructions (currProblem.Instructions);
				Events.Dispatcher.SetInitialProblemLetters (currProblem.InitialWord);
		        //save the new target word to the csv record for this acivity
				StudentsDataHandler.instance.RecordActivityTargetWord (currProblem.TargetWord (false));

				PlayInstructions (); //dont bother telling to place initial letters during assessment mode

				Events.Dispatcher.BeginNewProblem ();
				Events.Dispatcher.EnterMainActivity ();
				submitWordButton.SetActive (true);


		        

		}

	  

    
		void PlayInstructions ()
		{
			
			AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);
			
				
		}

	 

		bool CurrentStateOfUserInputMatchesTarget ()
		{       
		
			for (int i = 0; i < State.Current.UserInputLetters.Length; i++) {
			if (IsErroneous (i, State.Current.UserInputLetters [i]))
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



		bool IsErroneous(int atPosition, char letter){
			return (atPosition >= State.Current.TargetWord.Length && letter == ' ') ||
			(State.Current.TargetWord [atPosition] == letter);
     	}
		
		public void HandleSubmittedAnswer ()
		{     
		        
		StudentsDataHandler.instance.LogEvent ("submitted_answer", State.Current.UserInputLetters, State.Current.TargetWord);
					
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
			switch(State.Current.ActivityState){
				case ActivityStates.MAIN_ACTIVITY:
					hintController.ActivateHintButton ();
					AudioSourceController.PushClip (offerHint);
					break;
				case ActivityStates.FORCE_CORRECT_LETTER_PLACEMENT:
					break;
			
				}

		}

		void CurrentProblemCompleted (bool userSubmittedCorrectAnswer)
		{
			
			   
				UserInputRouter.instance.AddCurrentWordToHistory (false);
				UserInputRouter.instance.RequestDisplayImage (State.Current.TargetWord, false, true);

				bool solvedOnFirstTry = State.Current.TimesAttemptedCurrentProblem == 1;
				if (solvedOnFirstTry) {
		
						UserInputRouter.instance.DisplayNewStarOnScreen (ProblemsRepository.instance.ProblemsCompleted-1);

				}
			
				StudentsDataHandler.instance.RecordActivitySolved (userSubmittedCorrectAnswer, State.Current.UserInputLetters, solvedOnFirstTry);
			
		        StudentsDataHandler.instance.SaveActivityDataAndClearForNext (State.Current.TargetWord, State.Current.InitialTargetLetters);
		        
		     
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				Events.Dispatcher.ForceRemoveAllLetters();
				//disable submit button; we automatically go to next problem when user removes all letters. 
				submitWordButton.SetActive (false);
      
		}
		
		bool IsSubmissionCorrect ()
		{      
				return CurrentStateOfUserInputMatchesTarget ();

		}



}
