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
				

		Events.Dispatcher.OnNewProblemBegun += (ProblemData problem) => {
					ReplaceEachLetterWithBlank ();
					PlaceWordInLetterGrid (problem.initialWord);
					activateLinesBeneathLettersOfWord(problem.targetWord);
				};
				
				SceneManager.sceneLoaded += (Scene scene, LoadSceneMode arg1) => {
						if (scene.name == "Activity") {
							letterGrid = GameObject.Find("ArduinoLetterGrid").GetComponent<LetterGridController> ();
							letterGrid.InitializeBlankLetterSpaces (Parameters.UI.ONSCREEN_LETTER_SPACES);
							List<InteractiveLetter> UILetters = letterGrid.GetLetters ();
							SubscribeToInteractiveLetterEvents (UILetters);
							Events.Dispatcher.UILettersCreated (UILetters);
						}

				};

		}
		
		public void ChangeTheLetterOfASingleCell (int atPosition, char newLetter)
		{
			ChangeTheLetterOfASingleCell (atPosition, newLetter + "");

		}

		public void ChangeTheLetterOfASingleCell (int atPosition, String newLetter)
		{   		InteractiveLetter letter = GetInteractiveLetterAt(atPosition);
		            letter.UpdateLetterImage (letterGrid.GetAppropriatelyScaledImageForLetter(newLetter));
					if(newLetter==" ") {
						Events.Dispatcher.RecordInteractiveLetterDeSelected(letter);
					}
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
						ChangeTheLetterOfASingleCell (i, word[i]);
			
				}
		
		
		}

		public void ReplaceEachLetterWithBlank ()
		{
			for (int i = 0; i < Parameters.UI.ONSCREEN_LETTER_SPACES; i++) {
					ChangeTheLetterOfASingleCell (i, ' ');
			}
		}
		


		void SubscribeToInteractiveLetterEvents (List<InteractiveLetter> UILetters)
		{
				foreach(InteractiveLetter letter in UILetters){
					letter.OnInteractiveLetterSelectToggled += (bool wasSelected, InteractiveLetter l) => {
		
				       if(State.Current.UserInputLetters[l.Position] != ' '){
							if(wasSelected){
								Events.Dispatcher.RecordInteractiveLetterSelected(l);
							}else{
								Events.Dispatcher.RecordInteractiveLetterDeSelected(l);
							}

					 	}
					};

				}
	}

		         //todo colorer or the arduino l controller will do this, on new event colors of letters updated.
		//to be dispatched by the coloroer.

				/*int indexOfLetterBarCell = 0;
				for (; indexOfLetterBarCell<Parameters.UI.ONSCREEN_LETTER_SPACES; indexOfLetterBarCell++) {
						GameObject letterCell = letterGrid.GetLetterCell (indexOfLetterBarCell);
     
						letterCell.GetComponent<InteractiveLetter> ().IdxAsArduinoControlledLetter = ConvertScreenToArduinoIndex (indexOfLetterBarCell);//plus 1 because the indexes are shifted.
				}
		}*/

		int ConvertScreenToArduinoIndex (int screenIndex)
		{       //arduino starts counting at 1
				return screenIndex + 1;
		}



		public InteractiveLetter GetInteractiveLetterAt(int position){
			return letterGrid.GetInteractiveLetter (position);
		}



}
