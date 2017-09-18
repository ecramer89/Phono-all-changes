using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class StudentActivityController : MonoBehaviour
{
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


		void Start ()
	{       		instance = this;
		
					Events.Dispatcher.OnModeSelected += (Mode mode) => {
					
						if (mode == Mode.STUDENT) {
							Events.Dispatcher.OnUserEnteredNewLetter += HandleNewArduinoLetter;
							Events.Dispatcher.OnUserSubmittedTheirLetters += HandleSubmittedAnswer;
							CacheAudioClips ();
							Events.Dispatcher.OnSessionSelected += (int session) => {
								ProblemsRepository.instance.Initialize (session);
							};
							SceneManager.sceneLoaded += (Scene scene, LoadSceneMode arg1) => {
								if(scene.name=="Activity"){
											SetUpNextProblem ();
								}
							};
						} else {
							gameObject.SetActive (false);
						}
					};

					

		}


	void CacheAudioClips(){
		excellent = InstructionsAudio.instance.excellent;
		incorrectSoundEffect = InstructionsAudio.instance.incorrectSoundEffect;
		notQuiteIt = InstructionsAudio.instance.notQuiteIt;
		offerHint = InstructionsAudio.instance.offerHint;
		youDidIt = InstructionsAudio.instance.youDidIt;
		correctSoundEffect = InstructionsAudio.instance.correctSoundEffect;
		removeAllLetters = InstructionsAudio.instance.removeAllLetters;

		triumphantSoundForSessionDone = InstructionsAudio.instance.allDoneSession;
	}


	bool AllUserControlledLettersAreBlank(){
		return State.Current.UserInputLetters.Aggregate(true,(bool result, char nxt)=>result && nxt == ' ');
	}

	void HandleNewArduinoLetter(char newLetter, int atPosition){
		switch (State.Current.StudentModeState) {
		case StudentModeStates.MAIN_ACTIVITY:
			MainActivityNewLetterHandler(newLetter, atPosition);
			break;
		case StudentModeStates.FORCE_CORRECT_LETTER_PLACEMENT:
			ForceCorrectPlacementNewLetterHandler(newLetter, atPosition);
			break;
		case StudentModeStates.REMOVE_ALL_LETTERS:
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
		if(AllUserControlledLettersAreBlank()){ 
			HandleEndOfActivity ();
		}
	}
		
	 void SetUpNextProblem ()
		{  
		        Events.Dispatcher.RecordNewProblemBegun (
					ProblemsRepository.instance.GetNextProblem ()
				);
				Events.Dispatcher.EnterMainActivity ();
				AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);

				

		}

		void HandleEndOfActivity ()
		{
				if (ProblemsRepository.instance.AllProblemsDone ()) {
						Events.Dispatcher.RecordSessionCompleted ();
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
			CurrentProblemCompleted ();
		}

		void HandleIncorrectAnswer ()
		{
				Events.Dispatcher.RecordUserSubmittedIncorrectAnswer ();
				AudioSourceController.PushClip (incorrectSoundEffect);
				AudioSourceController.PushClip (notQuiteIt);
				if(State.Current.StudentModeState == StudentModeStates.MAIN_ACTIVITY) 
					AudioSourceController.PushClip (offerHint);
		}

	    
		void CurrentProblemCompleted ()
		{
				Events.Dispatcher.RecordCurrentProblemCompleted ();
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				Events.Dispatcher.ForceRemoveAllLetters();
			
      
		}
		



}
