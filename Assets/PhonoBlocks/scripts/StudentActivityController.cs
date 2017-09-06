using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class StudentActivityController : MonoBehaviour
{

		enum State
		{
				MAIN_ACTIVITY, //standard functionality (accept user input; update UI; update colors/error feedback)
		        FORCE_CORRECT_LETTER_PLACEMENT, //don't update the GUI letter images unless the inputted letter is correct. still show error feedback.
		        REMOVE_ALL_LETTERS //after the problem is completed & before going to next problem, force user to remove all the tangible letters they've placed on the platform
			
		}

		State state = State.MAIN_ACTIVITY;

		public void EnterForcedCorrectLetterPlacementMode(){
			state = State.FORCE_CORRECT_LETTER_PLACEMENT;

		}
		

		HintController hintController;
		ArduinoLetterController arduinoLetterController;
		Problem currProblem;

		public string TargetLetters{
		get {
			string targetWord = currProblem.TargetWord(true);

			return targetWord;
		}
	}
	    //cache the list of letter sound component objects that result from 'decoding'
	    //the target word. this is done so that we can check whether a given letter is part of 
	    //a target digraph, blend, etc.
		UserWord targetWordAsLetterSoundComponents;

		public bool StringMatchesTarget (string s)
		{
				return s.Equals (currProblem.TargetWord (true));

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
			
				currProblem = ProblemsRepository.instance.GetNextProblem ();
	     
		        //save the new target word to the csv record for this acivity
				StudentsDataHandler.instance.RecordActivityTargetWord (currProblem.TargetWord (false));

		       //decode the target word; cache the decoded letter sound components.
		       //this allows us to easily access the expected multi letter unit or color of target letter for each given position.
		        targetWordAsLetterSoundComponents = LetterSoundComponentFactoryManager.Decode (currProblem.TargetWord (true), 
		                                                                               SessionsDirector.IsSyllableDivisionActivity);



		        //clear the letters currently in the grid
		        //place the images of the initial letters into the grid
		        arduinoLetterController.ReplaceEachLetterWithBlank ();
				arduinoLetterController.PlaceWordInLetterGrid (currProblem.InitialWord);
	            //turn off all the letters to begin with.
		        arduinoLetterController.TurnAllLettersOff ();
		        //turn on the underlines that indicate which letters the child has to place
				arduinoLetterController.activateLinesBeneathLettersOfWord(currProblem.TargetWord(true));
		        

				PlayInstructions (); //dont bother telling to place initial letters during assessment mode

				state = State.MAIN_ACTIVITY;
				submitWordButton.SetActive (true);

		        

		}

	//todo consider abstracting in color coding scheme
	public void AssignNewColorToLetter(
		int indexOfLetterBarCell, 
		LetterSoundComponent parent,
		InteractiveLetter asInteractiveLetter,
		LetterSoundComponent asLetterSoundComponent,
		LetterGridController letterGrid){

		if(IsErroneous(indexOfLetterBarCell)){
			Color[] errorColors = SessionsDirector.colourCodingScheme.GetErrorColors();
			asInteractiveLetter.UpdateInputDerivedAndDisplayColor (errorColors[0]);
			asInteractiveLetter.SetFlashColors (errorColors [0], errorColors [1]);
			asInteractiveLetter.SetFlashDurations (Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON);
			asInteractiveLetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
			return;
		} 
			
		//correct letter; see whether it's part of a multi-letter unit.
		LetterSoundComponent targetComponent = 
			GetTargetLetterSoundComponentFor(indexOfLetterBarCell);

		Func<bool> childCompletedPortionOfTargetSpellingRule = () => {
			return targetComponent != null && targetComponent.IsComposite();
		};

		//the vowel needs to be correct; the e needs to be there; there needs to be two consonants but it doesn't matter what they are
		//if the regex matches vowel consonant e regex and if 
		//the match to vowel regex matches target vowel and if

		Func<bool> childInstantiatedTargetSpellingRule = () => {
			return (parent != null && parent.Equals(targetComponent) || 
				(SessionsDirector.instance.IsMagicERule && 
					true)); //todo if input instantiates magic e rule
		};
				
				if(childCompletedPortionOfTargetSpellingRule()){

						if (childInstantiatedTargetSpellingRule()) {
							asInteractiveLetter.UpdateInputDerivedAndDisplayColor (asLetterSoundComponent.GetColour ());
							asInteractiveLetter.SetFlashColors (asLetterSoundComponent.GetColour(), SessionsDirector.colourCodingScheme.GetColorsForOff());
							asInteractiveLetter.SetFlashDurations (Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF);
							asInteractiveLetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME);			
							return;

						}


						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (SessionsDirector.instance.CurrentActivityColorRules.GetColorForPortionOfTargetComposite ());

						if (!SessionsDirector.instance.IsConsonantBlends) {
							asInteractiveLetter.SetFlashColors (asLetterSoundComponent.GetColour(), targetComponent.GetColour ());
							asInteractiveLetter.SetFlashDurations (Parameters.Flash.Durations.CORRECT_TARGET_COLOR, Parameters.Flash.Durations.CORRECT_OFF);
							asInteractiveLetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_CORRECT_PORTION_OF_FINAL_GRAPHEME);
						}

						return;
					}

					asInteractiveLetter.UpdateInputDerivedAndDisplayColor(asLetterSoundComponent.GetColour());
			}

	  

    
		public void PlayInstructions ()
		{
			
				currProblem.PlayCurrentInstruction ();
			
				
		}

	   
		void ClearSavedUserChanges ()
		{
				for (int i=0; i<usersMostRecentChanges.Length; i++) {
						usersMostRecentChanges [i] = ' ';
					
				}

		}

		public LetterSoundComponent GetTargetLetterSoundComponentFor(int index){
		   return targetWordAsLetterSoundComponents.GetLetterSoundComponentForIndexRelativeWholeWord (index);
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
		    hintController.ProvideHint (currProblem);
	

		}

	
		public void HandleNewArduinoLetter (char letter, int atPosition)
	    {    
				RecordUsersChange (atPosition, letter); 
				switch (state) {
				case State.MAIN_ACTIVITY:
					arduinoLetterController.ChangeTheLetterOfASingleCell (atPosition, letter);
					arduinoLetterController.UpdateDefaultColoursAndSoundsOfLetters (true);
					break;
				case State.FORCE_CORRECT_LETTER_PLACEMENT:
					InteractiveLetter asInteractiveLetter = arduinoLetterController.GetInteractiveLetterAt (atPosition);
					if (IsErroneous (atPosition)) {
						Color[] errorColors = SessionsDirector.colourCodingScheme.GetErrorColors ();
						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (errorColors [0]);
						asInteractiveLetter.SetFlashColors (errorColors [0], errorColors [1]);
						asInteractiveLetter.SetFlashDurations (Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON);
						asInteractiveLetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
					} else {
						//in case the user removed a correct letter, then put it back; need to return the color to what it should be.
						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (GetTargetLetterSoundComponentFor (atPosition).GetColour());
					}
						break;
				case State.REMOVE_ALL_LETTERS:
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
		


	   public bool IsErroneous(int atPosition){
		if (!ReferenceEquals(currProblem, null) && !ReferenceEquals(currProblem.TargetWord (true), null)) {
			string target = currProblem.TargetWord(true);
			if(atPosition > target.Length-1 || atPosition > usersMostRecentChanges.Length) return false;
			char targetChar = target[atPosition];
			char actualChar = usersMostRecentChanges[atPosition];
			return (int)actualChar != 32 && targetChar != actualChar;
			}
			return false;
	    }

		bool PositionIsOutsideBoundsOfTargetWord (int wordRelativeIndex)
		{
				return wordRelativeIndex >= currProblem.TargetWord (true).Length; 
		}

		public void HandleSubmittedAnswer ()
		{     
		        
				StudentsDataHandler.instance.LogEvent ("submitted_answer", UserChangesAsString, currProblem.TargetWord (false));
					
				currProblem.IncrementTimesAttempted ();
		
				if (IsSubmissionCorrect ()) {

					AudioSourceController.PushClip (correctSoundEffect);
					if (currProblem.TimesAttempted > 1)
						AudioSourceController.PushClip (youDidIt);
					else
						AudioSourceController.PushClip (excellent);
					currProblem.PlayAnswer ();
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
				case State.MAIN_ACTIVITY:
					hintController.ActivateHintButton ();
					AudioSourceController.PushClip (offerHint);
					break;
				case State.FORCE_CORRECT_LETTER_PLACEMENT:
					break;
			
				}

		}

		public void CurrentProblemCompleted (bool userSubmittedCorrectAnswer)
		{
			
			   
				UserInputRouter.instance.AddCurrentWordToHistory (false);
				UserInputRouter.instance.RequestDisplayImage (currProblem.TargetWord (true), false, true);

				bool solvedOnFirstTry = currProblem.TimesAttempted == 1;
				if (solvedOnFirstTry) {
		
						UserInputRouter.instance.DisplayNewStarOnScreen (ProblemsRepository.instance.ProblemsCompleted-1);

				}
			
				StudentsDataHandler.instance.RecordActivitySolved (userSubmittedCorrectAnswer, UserChangesAsString, solvedOnFirstTry);
			
				StudentsDataHandler.instance.SaveActivityDataAndClearForNext (currProblem.TargetWord (false), currProblem.InitialWord);
		        
		     
		        //require user to remove all of the tangible letters from the platform before advancing to the next problem.
		        //don't want the letters still on platform from problem n being interpreted as input for problem n+1.
				state = State.REMOVE_ALL_LETTERS;
				//disable submit button; we automatically go to next problem when user removes all letters. 
				submitWordButton.SetActive (false);
      
		}

		public void RecordUsersChange (int position, char change)
		{
		
				usersMostRecentChanges [position] = change;

		
		}

		public bool IsSubmissionCorrect ()
		{      
				string target = currProblem.TargetWord (true);

				bool result = CurrentStateOfLettersMatches (target);

				
				return result;

		}



}
