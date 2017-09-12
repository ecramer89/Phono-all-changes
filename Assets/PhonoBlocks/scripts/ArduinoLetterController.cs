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
		public static ArduinoLetterController instance;
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

						
		        EMPTY_USER_WORD = "".Fill (' ', maxUserLetters);
				selectedUserControlledLettersAsStringBuilder = new StringBuilder (EMPTY_USER_WORD);
		
				letterGrid = letterGridControllerGO.GetComponent<LetterGridController> ();
				letterGrid.InitializeBlankLetterSpaces (maxUserLetters);

			
				AssignInteractiveLettersToTangibleCounterParts ();
				instance = this;

				Events.Dispatcher.UILettersCreated (letterGrid.GetLetters (false));



				Events.Dispatcher.OnInitialProblemLettersSet += (string initialProblemLetters) => {
					ReplaceEachLetterWithBlank ();
					PlaceWordInLetterGrid (initialProblemLetters);
					TurnAllLettersOff ();
				};

				Events.Dispatcher.OnTargetWordSet += (string targetWord) => {
					activateLinesBeneathLettersOfWord(targetWord);
				};

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
			
						UserInputRouter.instance.HandleNewUserInputLetter (newLetter,
			                                          atPosition, this);
						Events.Dispatcher.RecordNewUserInputLetter (newLetter, atPosition);

			           
				}
		}

	


		public void ChangeTheLetterOfASingleCell (int atPosition, char newLetter)
		{
			ChangeTheLetterOfASingleCell (atPosition, newLetter + "");

		}

		public void ChangeTheLetterOfASingleCell (int atPosition, String newLetter)
		{
		   letterGrid.GetInteractiveLetter (atPosition).
			UpdateInputLetterButNotInputDerivedColor (newLetter, 
			letterGrid.GetAppropriatelyScaledImageForLetter(newLetter));
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
			letterGrid.GetInteractiveLetter (atPosition).UpdateDisplayColour (newColour);
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
				l.RevertToInputDerivedColor ();
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



		public InteractiveLetter GetInteractiveLetterAt(int position){
			return letterGrid.GetInteractiveLetter (position);
		}



						
	/*
		void handleSyllableDivisionMode(
				LetterSoundComponent lc,
				int indexOfLetterBarCell
		){
				asInteractiveLetter = letterGrid.GetInteractiveLetter (indexOfLetterBarCell);
				asInteractiveLetter.UpdateDefaultColour (SessionsDirector.colourCodingScheme.GetColorsForWholeWord ());
				asInteractiveLetter.SetSelectColour (lc.GetColour ());
		}*/



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


			for (int i = (int)'a'; i <= (int)'z'; i++) {
				string s =""+(char)i;
				if (Input.GetKeyDown (s)) {
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
