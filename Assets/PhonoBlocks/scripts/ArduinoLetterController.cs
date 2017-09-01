using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using Extensions;

//todo this thing has a cached reference to the study activity controller. it doesn't need to check whether its student mode.
//or receive the sac as an argumenrt
public class ArduinoLetterController : MonoBehaviour{
	
		public StudentActivityController studentActivityController;
		public String EMPTY_USER_WORD;
		List<InteractiveLetter> lettersToFlash = new List<InteractiveLetter> ();


		private StringBuilder selectedUserControlledLettersAsStringBuilder;

		public string SelectedUserControlledLettersAsString {
				get {
						return selectedUserControlledLettersAsStringBuilder.ToString ();
				}
		
		}
				

		public static int startingIndexOfUserLetters;
		public static int endingIndexOfUserLetters;
		LetterGridController letterGrid;
		ArduinoUnityInterface tangibleLetters;
		public GameObject letterGridControllerGO;
	    string stringRepresentationOfPrevious;

		public int StartingIndex {
				get {
						return startingIndexOfUserLetters;
				}
				set {
						startingIndexOfUserLetters = value;


				}
		}

		public int EndingIndex {
				get {
						return endingIndexOfUserLetters;
				}
				set {
						endingIndexOfUserLetters = value;
			
				}
		}

		private int maxUserLetters;

		public int MaxArduinoLetters {
				get {
						return maxUserLetters;
				}


				set {
						maxUserLetters = value;

				}

		}

		public void Initialize (
				int startingIndexOfArduinoLetters, 
				int endingIndexOfArduinoLetters, 
				ArduinoUnityInterface tangibleLetters)
		{
				StartingIndex = startingIndexOfArduinoLetters;
				EndingIndex = endingIndexOfArduinoLetters;
				maxUserLetters = EndingIndex + 1 - StartingIndex;

						
		        EMPTY_USER_WORD = "".Fill (" ", maxUserLetters);
				selectedUserControlledLettersAsStringBuilder = new StringBuilder (EMPTY_USER_WORD);
		
				letterGrid = letterGridControllerGO.GetComponent<LetterGridController> ();
				letterGrid.InitializeBlankLetterSpaces (maxUserLetters);
			
				AssignInteractiveLettersToTangibleCounterParts ();
				
		}


			public void TurnAllLettersOff(){
				ChangeDisplayColourOfCells (Color.gray); 

			}



	
		//invoked by the arduino and the keyboard on screen
		public void ReceiveNewUserInputLetter (char newLetter, int atPosition)
		{
				StudentsDataHandler.instance.LogEvent ("change_letter", newLetter + "", atPosition + "");


				if (atPosition < maxUserLetters && atPosition >= StartingIndex) {
						if (IsUpper (newLetter))
								newLetter = ToLower (newLetter);
						ChangeTheLetterOfASingleCell (atPosition, newLetter);
						UserInputRouter.instance.HandleNewUserInputLetter (newLetter,
			                                          atPosition, this);
				}
		}

	


		public void ChangeTheLetterOfASingleCell (int atPosition, char newLetter)
		{
				letterGrid.UpdateLetter (atPosition, newLetter + "");

		}

		public void ChangeDisplayColourOfCells (Color newColour, bool onlySelected=false, int start=-1, int count=7)
		{
				start = (start < StartingIndex ? StartingIndex : start);
				count = (count > MaxArduinoLetters ? MaxArduinoLetters : count);
				if (!onlySelected) {
						for (int i=start; i<count; i++) {
								ChangeDisplayColourOfASingleCell (i, newColour);

						}
				} else {
					
						for (int i=start; i<count; i++) {
								if (selectedUserControlledLettersAsStringBuilder [i] != ' ')
										ChangeDisplayColourOfASingleCell (i, newColour);
						}

				}
		}

		public void ChangeDisplayColourOfASingleCell (int atPosition, Color newColour)
		{
				letterGrid.UpdateLetter (atPosition, newColour, false);
		}

		void ChangeTheImageOfASingleCell (int atPosition, Texture2D newImage)
		{
				InteractiveLetter i = letterGrid.GetInteractiveLetter (atPosition);
				i.SwitchImageTo (newImage);


		}

		public void RevertLettersToDefaultColour (bool onlySelected=false, int start=-1, int count=7)
		{
				start = (start < StartingIndex ? StartingIndex : start);
				count = (count > MaxArduinoLetters ? MaxArduinoLetters : count);
				if (!onlySelected) {
						for (int i=start; i<count; i++) {
								RevertASingleLetterToDefaultColour (i);
						}
				} else {
						for (int i=start; i<count; i++) {
								if (selectedUserControlledLettersAsStringBuilder [i] != ' ')
										RevertASingleLetterToDefaultColour (i);
				
						}

				}

		}

		public void RevertASingleLetterToDefaultColour (int atPosition)
		{
				InteractiveLetter l = letterGrid.GetLetterCell (atPosition).GetComponent<InteractiveLetter> ();
				l.RevertToDefaultColour ();
		}


		//updates letters and images of letter cells
		public void PlaceWordInLetterGrid (string word)
		{

				for (int i=0, j=startingIndexOfUserLetters; i<word.Length; i++,j++) {
						ChangeTheLetterOfASingleCell (j, word[i]);
					
				}
		
				 
		}

		public void activateLinesBeneathLettersOfWord (string word)
		{
		        
				letterGrid.setNumVisibleLetterLines (word.Length);

		        
		}

		//just updates the display images of the cells
		public void DisplayWordInLetterGrid (string word, bool ignoreBlanks=false)
		{
		
				for (int i=0, j=startingIndexOfUserLetters; i<word.Length; i++,j++) {
					if(!ignoreBlanks || word[i] != ' ')
						ChangeTheImageOfASingleCell (j, LetterImageTable.instance.GetLetterImageFromLetter (word.Substring (i, 1) [0]));
			
				}
		
		
		}

		public void ReplaceEachLetterWithBlank ()
		{
				PlaceWordInLetterGrid (EMPTY_USER_WORD);
		}

		public UserWord UpdateDefaultColoursAndSoundsOfLetters (bool flash)
		{
		
				UserWord newLetterSoundComponents = GetNewColoursAndSoundsFromDecoder ();
				AssignNewColoursAndSoundsToLetters (newLetterSoundComponents, letterGrid, flash);
				return newLetterSoundComponents;
		
		}

		public List<InteractiveLetter> GetAllUserInputLetters (bool skipBlanks)
		{
		
				return letterGrid.GetLetters (skipBlanks);
		
		}
		

		bool IsUpper (char letter)
		{
				int asInt = (int)letter;
				return asInt > 64 && asInt < 91;


		}

		//97-122 lower case; 65-> upper case
		char ToLower (char newLetter)
		{
				return (char)((int)newLetter + 32);

		}


		UserWord GetNewColoursAndSoundsFromDecoder ()
		{
			
		        string userControlledLettersAsString = studentActivityController.UserChangesAsString;
				Debug.Log ("user controlled letters are " + userControlledLettersAsString);
				return LetterSoundComponentFactoryManager.Decode (userControlledLettersAsString, SessionsDirector.IsSyllableDivisionActivity);
		
		}

		void AssignInteractiveLettersToTangibleCounterParts ()
		{
				int indexOfLetterBarCell = startingIndexOfUserLetters;
				for (; indexOfLetterBarCell<=endingIndexOfUserLetters; indexOfLetterBarCell++) {
						GameObject letterCell = letterGrid.GetLetterCell (indexOfLetterBarCell);
     
						letterCell.GetComponent<InteractiveLetter> ().IdxAsArduinoControlledLetter = ConvertScreenToArduinoIndex (indexOfLetterBarCell);//plus 1 because the indexes are shifted.
				}
		}

		int ConvertScreenToArduinoIndex (int screenIndex)
		{       //arduino starts counting at 1
				return screenIndex + 1;
		}

		void AssignNewColoursAndSoundsToLetters (UserWord letterSoundComponents, LetterGridController letterGridController, bool flash)
		{   
				
				int indexOfLetterBarCell = startingIndexOfUserLetters;

				foreach (LetterSoundComponent p in letterSoundComponents) {
			
								if (p is LetterSoundComposite) {
										LetterSoundComposite l = (LetterSoundComposite)p;
										foreach (LetterSoundComponent lc in l.Children) {
						                //the individual letters that compose a multi letter unit, for example the "b" in blend "bl"
												
												UpdateInterfaceLetter (lc, letterGridController, indexOfLetterBarCell, l);
												indexOfLetterBarCell++;
										}
								} else {
									
										UpdateInterfaceLetter (p, letterGridController, indexOfLetterBarCell);
										indexOfLetterBarCell++;
								}

						
				}
		}


		bool flash;
		int timesToFlash;
		Color newDefaultColor;
	    Color flashColor;
	    InteractiveLetter asInteractiveLetter;


		void UpdateInterfaceLetter (
				LetterSoundComponent lc, 
				LetterGridController letterGridController, 
				int indexOfLetterBarCell, 
				LetterSoundComposite parent = null)
	{          
		   
				flash = false;
				timesToFlash = 0;
		        bool isPartOfCompletedGrapheme = !ReferenceEquals (parent, null);
				char newLetter = lc.AsString [0];
				newDefaultColor = lc.GetColour ();
		        asInteractiveLetter = letterGridController.GetInteractiveLetter (indexOfLetterBarCell);

	
		        bool letterIsNew = !((parent != null ? parent : lc).Equals (asInteractiveLetter.LetterSoundComponentIsPartOf));


				flashColor = SessionsDirector.colourCodingScheme.GetColorsForOff ();
		        flash = false;


				if (SessionsDirector.IsSyllableDivisionActivity) {
					handleSyllableDivisionMode (lc, indexOfLetterBarCell);
				}
				else if (SessionsDirector.IsStudentMode) {
					handleStudentMode (letterIsNew,
					isPartOfCompletedGrapheme,
					indexOfLetterBarCell, 
					newLetter, parent, lc);

				} else if (SessionsDirector.IsTeacherMode) {
					handleTeacherMode (isPartOfCompletedGrapheme, letterIsNew, 
				     newLetter, lc, indexOfLetterBarCell, parent);
				}

		     
		        asInteractiveLetter.LetterSoundComponentIsPartOf = parent != null ? parent : lc; 

		      //flash only if letter is new and not a blank
			        
				if (letterIsNew && newLetter != ' ') {
						asInteractiveLetter.StartFlash ();
				}


			}
						

		void handleSyllableDivisionMode(
				LetterSoundComponent lc,
				int indexOfLetterBarCell
		){
				asInteractiveLetter = letterGrid.GetInteractiveLetter (indexOfLetterBarCell);
				asInteractiveLetter.UpdateDefaultColour (SessionsDirector.colourCodingScheme.GetColorsForWholeWord ());
				asInteractiveLetter.SetSelectColour (lc.GetColour ());
		}

		void handleTeacherMode(
				bool isPartOfCompletedGrapheme,
				bool letterIsNew,
				char newLetter,
				LetterSoundComponent lc,
				int indexOfLetterBarCell,
				LetterSoundComponent parent

		){

				//in teacher mode. if rule is r controlled vowel, consonant or vowel digraphs, need to check if this
				//letter is the first letter of any valid of these graphemes and flash it in that color if it is.
				if(!isPartOfCompletedGrapheme){
						string currentRule = SessionsDirector.instance.GetCurrentRule;
						flash = letterIsNew;
						timesToFlash = TimingParameters.TIMES_TO_FLASH_CORRECT_PORTION_OF_FINAL_GRAPHEME;
						switch(currentRule){
						case "rControlledVowel":
								if(SpeechSoundReference.IsFirstLetterOfRControlledVowel(newLetter)){
										flashColor = SessionsDirector.instance.CurrentActivityColorRules.GetColorsForRControlledVowel();
										newDefaultColor = Color.gray;
								}
								break;
						case "consonantDigraphs":
								if(SpeechSoundReference.IsFirstLetterOfConsonantDigraph(newLetter)){
										flashColor = SessionsDirector.instance.CurrentActivityColorRules.GetColorsForConsonantDigraphs();
										newDefaultColor = Color.gray;
								}
								break;
						case "vowel Digraphs":
								if(SpeechSoundReference.IsFirstLetterOfVowelDigraph(newLetter)){
										flashColor = SessionsDirector.instance.CurrentActivityColorRules.GetColorsForVowelDigraphs();
										newDefaultColor = Color.gray;
								}
								break;
						default:
								flash = false;
								timesToFlash = 0;
								break;

						}
				}


		}


		void handleStudentMode(
				bool letterIsNew,
				bool isPartOfCompletedGrapheme,
				int indexOfLetterBarCell, 
				char newLetter,
				LetterSoundComponent parent,
				LetterSoundComponent lc
		){


				if(studentActivityController.IsErroneous(indexOfLetterBarCell)){
						Color[] errorColors = SessionsDirector.colourCodingScheme.GetErrorColors();
						letterGrid.UpdateLetter (indexOfLetterBarCell, errorColors[0]); 
						asInteractiveLetter.SetFlashColors (errorColors [0], errorColors [1]);
						asInteractiveLetter.SetFlashDurations (1, 1);
						asInteractiveLetter.SetNumFlashCycles (TimingParameters.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
						return;
				} 


				//correct letter; see whether it's part of a multi-letter unit.
				LetterSoundComponent targetComponent = 
						studentActivityController.GetTargetLetterSoundComponentFor(indexOfLetterBarCell);

		          if (targetComponent != null && targetComponent.IsComposite()) {
						if(parent != null && targetComponent.AsString.Equals(parent.AsString) || 
							(SessionsDirector.instance.IsMagicERule && studentActivityController.IsSubmissionCorrect())) {
							letterGrid.UpdateLetter (indexOfLetterBarCell, newDefaultColor); 
							asInteractiveLetter.SetFlashColors (newDefaultColor, SessionsDirector.colourCodingScheme.GetColorsForOff());
							asInteractiveLetter.SetFlashDurations (1, .5f);
							asInteractiveLetter.SetNumFlashCycles (TimingParameters.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME);			
						    return;
							
						}

					letterGrid.UpdateLetter (indexOfLetterBarCell, SessionsDirector.instance.CurrentActivityColorRules.GetColorForPortionOfTargetComposite ()); 

					//If it isn't blends, then should flash.
					//todo abstract this into the color coding scheme as well.
					if (!SessionsDirector.instance.IsConsonantBlends) {
						asInteractiveLetter.SetFlashColors (newDefaultColor, targetComponent.GetColour ());
						asInteractiveLetter.SetFlashDurations (.5f, 1);
						asInteractiveLetter.SetNumFlashCycles (TimingParameters.TIMES_TO_FLASH_CORRECT_PORTION_OF_FINAL_GRAPHEME);
					}
				}

		}


		//all of this is for testing; simulates arduino functionality.
		static int testPosition = -1;

		public void SetTestPosition (int newPosition)
		{
				testPosition = newPosition;
			
				UpdateLetterBarIfPositionAndLetterSpecified ();
		}
	    
		static String testLetter;

		public void SetTestLetter (String newLetter)
		{
				testLetter = newLetter;
				UpdateLetterBarIfPositionAndLetterSpecified ();
				
		}

		public void ClearTestLetter ()
		{
				testLetter = null;
				

		}

		public void ClearTestPosition ()
		{
			
				testPosition = -1;

		}

		void Update ()
		{
				if (Input.anyKeyDown) {
						if (Input.GetMouseButton (0) || Input.GetMouseButton (1) || Input.GetMouseButton (2))
								return;

						if (ParseNumericKey () || ParseLetterKey ())
								UpdateLetterBarIfPositionAndLetterSpecified ();




				}
	

		}

		bool ParseNumericKey ()
		{
				String s;
	
				for (int i=0; i<maxUserLetters; i++) {
						s = "" + i;
						if (Input.GetKey (s)) {
								//testPosition = i;
								SetTestPosition (i);
								return true;
						}
				}
				return false;
		}

		bool ParseLetterKey ()
		{       

				//deleting a character
				if (Input.GetKeyDown (KeyCode.Backspace)) {
						//testLetter = " ";
						SetTestLetter (" ");
						return true;
				}

				foreach (string s in SpeechSoundReference.Vowels()) {
					
						if (Input.GetKeyDown (s)) {
								SetTestLetter (s);
								return true;
						}
				}

				foreach (string s in SpeechSoundReference.Consonants()) {
				
						if (Input.GetKeyDown (s)) {
								//testLetter = s;
								SetTestLetter (s);
								return true;
						}
						


				}
				return false;

		}

		void UpdateLetterBarIfPositionAndLetterSpecified ()
		{
				if (testPosition != -1 && testLetter != null) {
						
						ReceiveNewUserInputLetter (testLetter [0], testPosition);
						ClearTestPosition ();
						
				}
		}


}
