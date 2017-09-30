using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using Extensions;


public class ArduinoLetterController : PhonoBlocksSubscriber{
	    
		public static ArduinoLetterController instance;
		
		private StringBuilder selectedUserControlledLettersAsStringBuilder;

		public string SelectedUserControlledLettersAsString {
				get {
						return selectedUserControlledLettersAsStringBuilder.ToString ();
				}
		
		}
				

		LetterGridController letterGrid;


	public override void SubscribeToAll(PhonoBlocksScene forScene){


		if(forScene == PhonoBlocksScene.Activity){
			Transaction.Instance.ActivitySceneLoaded.Subscribe(this,() => {
				letterGrid = GameObject.Find("ArduinoLetterGrid").GetComponent<LetterGridController> ();
				letterGrid.InitializeBlankLetterSpaces (Parameters.UI.ONSCREEN_LETTER_SPACES);
				List<InteractiveLetter> UILetters = letterGrid.GetLetters ();
				SubscribeToInteractiveLetterEvents(UILetters);
				Transaction.Instance.InteractiveLettersCreated.Fire (UILetters);
			});


		Transaction.Instance.NewProblemBegun.Subscribe(this, (ProblemData problem) => {
			ReplaceEachLetterWithBlank ();
			PlaceWordInLetterGrid (problem.initialWord, LetterImageTable.instance.GetLetterOutlineImageFromLetter);
			activateLinesBeneathLettersOfWord(problem.targetWord);
		});
		



		Transaction.Instance.InteractiveLetterSelected.Subscribe(this,(InteractiveLetter letter) => {
				letter.ToggleSelectHighlight(true);
				if(Transaction.Instance.Selector.AllLettersSelected 
				&& Transaction.Instance.State.SyllableDivisionShowState == SyllableDivisionShowStates.SHOW_WHOLE_WORD){
				

					Transaction.Instance.SyllableDivisionShowStateSet.Fire(SyllableDivisionShowStates.SHOW_DIVISION);
			}
		});


		Transaction.Instance.InteractiveLetterDeselected.Subscribe(this,(InteractiveLetter letter) => {
				letter.ToggleSelectHighlight(false);

			
			if(Transaction.Instance.Selector.AllLettersDeSelected &&
				Transaction.Instance.State.SyllableDivisionShowState == SyllableDivisionShowStates.SHOW_DIVISION){
					Transaction.Instance.SyllableDivisionShowStateSet.Fire(SyllableDivisionShowStates.SHOW_WHOLE_WORD);
			}
		});

		}
	}

		public void Start ()
		{
				instance = this;
				
		}




	void SubscribeToInteractiveLetterEvents (List<InteractiveLetter> UILetters)
	{
		foreach(InteractiveLetter letter in UILetters){
			letter.OnInteractiveLetterSelectToggled += (bool wasSelected, InteractiveLetter l) => {
				if(Transaction.Instance.State.Activity != Activity.SYLLABLE_DIVISION) return;
				if(Transaction.Instance.State.UserInputLetters[l.Position] == ' ') return;
				//don't send event if position is already selected or deselected
				if(wasSelected && Transaction.Instance.State.SelectedUserInputLetters[l.Position] != ' ' || 
					!wasSelected && Transaction.Instance.State.SelectedUserInputLetters[l.Position] == ' ') return; //don't select a letter if already selected
				if(Transaction.Instance.State.Mode == Mode.STUDENT && !Transaction.Instance.Selector.CurrentStateOfInputMatchesTarget) return;

				if(wasSelected){
					Transaction.Instance.InteractiveLetterSelected.Fire(l);
				}else{
					Transaction.Instance.InteractiveLetterDeselected.Fire(l);
				}

			};

		}
	}
		
	public void ChangeTheLetterOfASingleCell (int atPosition, char newLetter,Func<char, Texture2D> imageGetter)
		{
			ChangeTheLetterOfASingleCell (atPosition, newLetter + "", imageGetter);

		}

		public void ChangeTheImageOfASingleCell(int atPosition, Texture2D image){
			InteractiveLetter letter = GetInteractiveLetterAt(atPosition);
			letter.UpdateLetterImage(letterGrid.ConfigureTextureForLetterGrid(image));
		}

	public void ChangeTheLetterOfASingleCell (int atPosition, String newLetter, Func<char, Texture2D> imageGetter)
		{   		InteractiveLetter letter = GetInteractiveLetterAt(atPosition);
					ChangeTheImageOfASingleCell(atPosition, imageGetter(newLetter[0]));
					if(newLetter==" ") { //deselect letters that the user removes
						Transaction.Instance.InteractiveLetterDeselected.Fire(letter);
					}
		}


		//updates letters and images of letter cells
	public void PlaceWordInLetterGrid (string word, Func<char, Texture2D> imageGetter)
		{

				for (int i=0;i<word.Length; i++) {
					ChangeTheLetterOfASingleCell (i, word[i],imageGetter);
					
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
				ChangeTheLetterOfASingleCell (i, word[i], LetterImageTable.instance.GetLetterImageFromLetter);
			
				}
		
		
		}

		public void ReplaceEachLetterWithBlank ()
		{
			for (int i = 0; i < Parameters.UI.ONSCREEN_LETTER_SPACES; i++) {
			ChangeTheLetterOfASingleCell (i, ' ', LetterImageTable.instance.GetBlankLetterImage);
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
