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
				MAIN_ACTIVITY,
		        FORCE_CORRECT_LETTER_PLACEMENT,
		        REMOVE_ALL_LETTERS
			
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
			
				SetUpNextProblem ();
	
			    //cache audio clips
				excellent = InstructionsAudio.instance.excellent;
				incorrectSoundEffect = InstructionsAudio.instance.incorrectSoundEffect;
				notQuiteIt = InstructionsAudio.instance.notQuiteIt;
				offerHint = InstructionsAudio.instance.offerHint;
				youDidIt = InstructionsAudio.instance.youDidIt;
				correctSoundEffect = InstructionsAudio.instance.correctSoundEffect;
				removeAllLetters = InstructionsAudio.instance.removeAllLetters;

				triumphantSoundForSessionDone = InstructionsAudio.instance.allDoneSession;
		}

		
	public void SetUpNextProblem ()
		{  
			
				ClearSavedUserChanges ();
				hintController.Reset ();
			
				currProblem = ProblemsRepository.instance.GetNextProblem ();
	     
		        //save the new target word to the csv record for this acivity
				StudentsDataHandler.instance.RecordActivityTargetWord (currProblem.TargetWord (false));

		       //decode the target word; cache the decoded letter sound components.
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
		        
				UserInputRouter.instance.RequestTurnOffImage ();
				hintController.DeActivateHintButton ();
			
				PlayInstructions (); //dont bother telling to place initial letters during assessment mode

				state = State.MAIN_ACTIVITY;

		        

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

		Func<bool> childInstantiatedTargetSpellingRule = () => {
			return (parent != null && parent.Equals(targetComponent) || 
					(SessionsDirector.instance.IsMagicERule && IsSubmissionCorrect()));
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
					if (letter != ' ')
						return;
						arduinoLetterController.ChangeTheLetterOfASingleCell (atPosition, letter);
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
			if (state == State.REMOVE_ALL_LETTERS) {
			   //go to next problem if all the letters were removed.
				if(allUserControlledLettersAreBlank()) {
					HandleEndOfActivity ();
				}
				
			} else {
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
		        
		     
				state = State.REMOVE_ALL_LETTERS;
      
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
