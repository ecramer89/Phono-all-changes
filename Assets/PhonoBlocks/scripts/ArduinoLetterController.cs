using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using Extensions;

//todo this thing has a cached reference to the study activity controller. it doesn't need to check whether its student mode.
//or receive the sac as an argument
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
				

		LetterGridController letterGrid;
		ArduinoUnityInterface tangibleLetters;
		public GameObject letterGridControllerGO;
	 



		public void Initialize (ArduinoUnityInterface tangibleLetters)
		{
			
	
				selectedUserControlledLettersAsStringBuilder = new StringBuilder (EMPTY_USER_WORD);
		
				letterGrid = letterGridControllerGO.GetComponent<LetterGridController> ();
				letterGrid.InitializeBlankLetterSpaces (Parameters.UI.ONSCREEN_LETTER_SPACES);
	
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

	/*
	 * 
	 * if ui inputs are currently locked then do nothing.
	 * if it's teacher mode then update the letter immediately.
	 * if it's student mode then:
	 *   if its main activity then update the letter.
	 * if it's force correct then don't do anything
	 * if it's froce removal then only update if... why doesn't the sac do this? and have a teacher mode controller
	 * 
	 * */
		public void ReceiveNewUserInputLetter (char newLetter, int atPosition)
		{
				StudentsDataHandler.instance.LogEvent ("change_letter", newLetter + "", atPosition + "");


				if (atPosition < Parameters.UI.ONSCREEN_LETTER_SPACES && atPosition >= 0) {
						if (IsUpper (newLetter))
								newLetter = ToLower (newLetter);
			
						UserInputRouter.instance.HandleNewUserInputLetter (newLetter,
			                                          atPosition, this);
	

			           
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
				start = (start < 0 ? 0 : start);
				count = (count > Parameters.UI.ONSCREEN_LETTER_SPACES ? Parameters.UI.ONSCREEN_LETTER_SPACES : count);
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
				start = (start < 0 ? 0 : start);
				count = (count > Parameters.UI.ONSCREEN_LETTER_SPACES ? Parameters.UI.ONSCREEN_LETTER_SPACES : count);
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

				for (int i=0;i<word.Length; i++) {
						ChangeTheLetterOfASingleCell (i, word[i]);
					
				}
		
				 
		}

		public void activateLinesBeneathLettersOfWord (string word)
		{
		        
				letterGrid.setNumVisibleLetterLines (word.Length);

		        
		}

		//just updates the display images of the cells
		public void DisplayWordInLetterGrid (string word, bool ignoreBlanks=false)
		{
		
		for (int i=0; i<word.Length; i++) {
					if(!ignoreBlanks || word[i] != ' ')
						ChangeTheImageOfASingleCell (i, LetterImageTable.instance.GetLetterImageFromLetter (word.Substring (i, 1) [0]));
			
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
				int indexOfLetterBarCell = 0;
				for (; indexOfLetterBarCell<Parameters.UI.ONSCREEN_LETTER_SPACES; indexOfLetterBarCell++) {
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






}
