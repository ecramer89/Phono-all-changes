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
		{      instance = this;
		
				Dispatcher.Instance.ModeSelected.Subscribe((Mode mode) => {
					
						if (mode == Mode.STUDENT) {
							Dispatcher.Instance.OnUserEnteredNewLetter += HandleNewArduinoLetter;
							Dispatcher.Instance.OnUserSubmittedTheirLetters += HandleSubmittedAnswer;
							CacheAudioClips ();
							SubscribeToEvents();
						} else {
							gameObject.SetActive (false);
						}
				});				

		}



	void SubscribeToEvents(){
		Dispatcher.Instance.OnSessionSelected += (int session) => {
			ProblemsRepository.instance.Initialize (session);
		};
		SceneManager.sceneLoaded += (Scene scene, LoadSceneMode arg1) => {
			if(scene.name=="Activity"){
				SetUpNextProblem ();
			}
		};

		Dispatcher.Instance.OnActivitySelected+=(Activity activity) => {
			if(activity == Activity.SYLLABLE_DIVISION){
				Dispatcher.Instance.OnNewProblemBegun+=(ProblemData problem)=>{
					Dispatcher.Instance.RecordTargetWordSyllablesSet(SpellingRuleRegex.Syllabify(problem.targetWord));
				};
			}
		};

		//placeholder letters have the blank letter outline
		Dispatcher.Instance.OnNewProblemBegun += (ProblemData problem) => {
			for(int i=0;i<problem.initialWord.Length;i++){
				ArduinoLetterController.instance.ChangeTheImageOfASingleCell(i, LetterImageTable.instance.GetLetterOutlineImageFromLetter(problem.initialWord[i]));
			}
		};
		//if the user removes a letter at a position of a place holder letter,
		//then restore the dashed letter outline for the placeholder.
		Dispatcher.Instance.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			if(newLetter!=' ' || atPosition >= State.Current.PlaceHolderLetters.Length) return;
			char placeholderLetter = State.Current.PlaceHolderLetters[atPosition];
			if(placeholderLetter == ' ') return;

			ArduinoLetterController.instance.ChangeTheImageOfASingleCell(
				atPosition, 
				LetterImageTable.instance.GetLetterOutlineImageFromLetter(placeholderLetter));

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
		        Dispatcher.Instance.RecordNewProblemBegun (
					ProblemsRepository.instance.GetNextProblem ()
				);
			
				Dispatcher.Instance.EnterStudentModeMainActivity ();
				AudioSourceController.PushClips (State.Current.CurrentProblemInstructions);

				

		}

		void HandleEndOfActivity ()
		{
				if (ProblemsRepository.instance.AllProblemsDone ()) {
						Dispatcher.Instance.RecordSessionCompleted ();
						AudioSourceController.PushClip (triumphantSoundForSessionDone);
						
				} else {
						SetUpNextProblem ();
				}

		}
				

		
		public void HandleSubmittedAnswer ()
		{     
		       			
				Dispatcher.Instance.IncrementTimesAttemptedCurrentProblem ();
		
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
				Dispatcher.Instance.RecordUserSubmittedIncorrectAnswer ();
				AudioSourceController.PushClip (incorrectSoundEffect);
				AudioSourceController.PushClip (notQuiteIt);
				if(State.Current.StudentModeState == StudentModeStates.MAIN_ACTIVITY) 
					AudioSourceController.PushClip (offerHint);
		}

	    
		void CurrentProblemCompleted ()
		{
				Dispatcher.Instance.RecordCurrentProblemCompleted ();
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				Dispatcher.Instance.ForceRemoveAllLetters();
			
      
		}
		



}
