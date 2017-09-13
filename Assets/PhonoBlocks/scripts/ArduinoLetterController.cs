using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using Extensions;
using UnityEngine.SceneManagement;

public class ArduinoLetterController : MonoBehaviour{
		public static ArduinoLetterController instance;
		
		private StringBuilder selectedUserControlledLettersAsStringBuilder;

		public string SelectedUserControlledLettersAsString {
				get {
						return selectedUserControlledLettersAsStringBuilder.ToString ();
				}
		
		}
				

		LetterGridController letterGrid;


		public void Start ()
		{
				instance = this;
				

				Events.Dispatcher.OnInitialProblemLettersSet += (string initialProblemLetters) => {
					ReplaceEachLetterWithBlank ();
					PlaceWordInLetterGrid (initialProblemLetters);
					TurnAllLettersOff ();
				};

				Events.Dispatcher.OnTargetWordSet += (string targetWord) => {
					activateLinesBeneathLettersOfWord(targetWord);
				};

				SceneManager.sceneLoaded += (Scene scene, LoadSceneMode arg1) => {
						if (scene.name == "Activity") {
							letterGrid = GameObject.Find("ArduinoLetterGrid").GetComponent<LetterGridController> ();
							letterGrid.InitializeBlankLetterSpaces (Parameters.UI.ONSCREEN_LETTER_SPACES);
							AssignInteractiveLettersToTangibleCounterParts ();

							Events.Dispatcher.UILettersCreated (letterGrid.GetLetters (false));
						}

				};

		}


			public void TurnAllLettersOff(){
				ChangeDisplayColourOfCells (Color.gray); 

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
			for (int i = 0; i < Parameters.UI.ONSCREEN_LETTER_SPACES; i++) {
					ChangeTheLetterOfASingleCell (i, ' ');
			}
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



}
