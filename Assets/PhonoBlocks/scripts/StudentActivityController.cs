using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class StudentActivityController : MonoBehaviour
{

		HintController hintController;
		private static StudentActivityController instance;
		public static StudentActivityController Instance{
			get {
				return instance;
			}

		}
	    //save references to frequently used audio clips 
		AudioClip excellent;
		AudioClip incorrectSoundEffect;
		AudioClip notQuiteIt;
		AudioClip offerHint;
		AudioClip youDidIt;
		AudioClip correctSoundEffect;
		AudioClip removeAllLetters;
		AudioClip triumphantSoundForSessionDone;


		public void Initialize ()
	{       	instance = this;
				Events.Dispatcher.OnUserEnteredNewLetter += HandleNewArduinoLetter;
				Events.Dispatcher.OnUserSubmittedTheirLetters += HandleSubmittedAnswer;
			    //cache audio clips
				excellent = InstructionsAudio.instance.excellent;
				incorrectSoundEffect = InstructionsAudio.instance.incorrectSoundEffect;
				notQuiteIt = InstructionsAudio.instance.notQuiteIt;
				offerHint = InstructionsAudio.instance.offerHint;
				youDidIt = InstructionsAudio.instance.youDidIt;
				correctSoundEffect = InstructionsAudio.instance.correctSoundEffect;
				removeAllLetters = InstructionsAudio.instance.removeAllLetters;

				triumphantSoundForSessionDone = InstructionsAudio.instance.allDoneSession;

		  
				SetUpNextProblem ();
		}


	bool allUserControlledLettersAreBlank(){
		return State.Current.UserInputLetters.Aggregate(true,(bool result, char nxt)=>result && nxt == ' ');
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


	void MainActivityNewLetterHandler(char letter, int atPosition){
		ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, letter);
		Colorer.Instance.ReColor ();
	}

	void ForceCorrectPlacementNewLetterHandler(char letter, int atPosition){
		InteractiveLetter asInteractiveLetter = State.Current.UILetters[atPosition];
		if (Selector.Instance.IsCorrectlyPlaced(atPosition)){
			//in case the user removed and then replaced a letter correctly.
			asInteractiveLetter.UpdateInputDerivedAndDisplayColor (State.Current.TargetWordColors[atPosition]);
			return;

		}
		//otherwise, don't update the UI letters but do flash the error to indicate that child didn't place the right letter.	
		asInteractiveLetter.UpdateInputDerivedAndDisplayColor (Parameters.Colors.DEFAULT_OFF_COLOR);
		asInteractiveLetter.ConfigureFlashParameters (
			Parameters.Colors.DEFAULT_OFF_COLOR, Parameters.Colors.DEFAULT_ON_COLOR,
			Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON,
			Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER
		);
		asInteractiveLetter.StartFlash ();

	}

	void RemoveAllLettersNewLetterHandler(char letter, int atPosition){
		if (letter != ' ') //don't bother updating the letter unless the user removed it
			return;

		ArduinoLetterController.instance.ChangeTheLetterOfASingleCell (atPosition, letter);
		//once the user removes all letters from the current problem; automatically turn off the display image and go to the next activity.
		if(allUserControlledLettersAreBlank()){ 
			UserInputRouter.instance.RequestTurnOffImage ();
			HandleEndOfActivity ();
		}
	}
		
	 void SetUpNextProblem ()
		{  
			
				Problem currProblem = ProblemsRepository.instance.GetNextProblem ();
				Events.Dispatcher.SetTargetWord (currProblem.TargetWord(true));
				Events.Dispatcher.SetCurrentProblemInstructions (currProblem.Instructions);
				Events.Dispatcher.SetInitialProblemLetters (currProblem.InitialWord);
				Events.Dispatcher.BeginNewProblem ();
				Events.Dispatcher.EnterMainActivity ();


				AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);

				

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
				

		
		public void HandleSubmittedAnswer ()
		{     
		       			
				Events.Dispatcher.IncrementTimesAttemptedCurrentProblem ();
		
				if (Selector.Instance.CurrentStateOfInputMatchesTarget) {
					HandleCorrectAnswer ();
				} else {
					HandleIncorrectAnswer ();				
					
				}


		}

		void HandleCorrectAnswer(){
			AudioSourceController.PushClip (correctSoundEffect);
			if (State.Current.TimesAttemptedCurrentProblem > 1)
				AudioSourceController.PushClip (youDidIt);
			else
				AudioSourceController.PushClip (excellent);
			AudioSourceController.PushClip (AudioSourceController.GetWordFromResources(State.Current.TargetWord));
			CurrentProblemCompleted ();
		}

		void HandleIncorrectAnswer ()
		{
				Events.Dispatcher.RecordUserSubmittedIncorrectAnswer ();
				AudioSourceController.PushClip (incorrectSoundEffect);
				AudioSourceController.PushClip (notQuiteIt);
				if(State.Current.ActivityState == ActivityStates.MAIN_ACTIVITY) 
					AudioSourceController.PushClip (offerHint);
		}

		void CurrentProblemCompleted ()
		{
				Events.Dispatcher.RecordCurrentProblemCompleted ();
	
				if (Selector.Instance.SolvedOnFirstTry) {
		
						UserInputRouter.instance.DisplayNewStarOnScreen (ProblemsRepository.instance.ProblemsCompleted-1);

				}
			
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				Events.Dispatcher.ForceRemoveAllLetters();
			
      
		}
		



}
