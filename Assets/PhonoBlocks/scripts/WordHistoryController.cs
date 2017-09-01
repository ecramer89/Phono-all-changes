using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class WordHistoryController : MonoBehaviour
{
		int wordLength;
		public GameObject wordHistoryPanelBackground;
		LetterImageTable letterImageTable;

		public int WordLength {
				get {

						return wordLength;

				}

		}

		public GameObject wordHistoryGrid;
		LetterGridController lettersOfWordInHistory;
		public List<Word> words; //words in the word history.
		Word psuedoWord; //a dummy value to return in case there is some kind of error.
		public Word PsuedoWord {
				get {
						if (psuedoWord == null) {
								psuedoWord = new Word ("whoops");
			
						}
						return psuedoWord;
				}

		}

		public int showImageTime = 60 * 8;

		public void Initialize (int wordLength)
		{
				this.wordLength = wordLength;
				lettersOfWordInHistory = wordHistoryGrid.gameObject.GetComponent<LetterGridController> ();

				wordHistoryGrid.GetComponent<UIGrid> ().maxPerLine = wordLength;
				letterImageTable = GameObject.Find ("DataTables").GetComponent<LetterImageTable> ();
				InteractiveLetter.LetterPressed += PlayWordOfPressedLetter;
	
		}

		public void AddCurrentWordToHistory (List<InteractiveLetter> currentWord, bool playSoundAndShowImage=false)
		{
				Word newWord = CreateNewWordAndAddToList (AddLettersOfNewWordToHistory (currentWord));
				if (playSoundAndShowImage) {		
						AudioSourceController.PushClip (newWord.Sound);
						UserInputRouter.instance.RequestDisplayImage (newWord.AsString, true);
				}
	
			
		}

		string AddLettersOfNewWordToHistory (List<InteractiveLetter> newWord)
		{ 
				StringBuilder currentWordAsString = new StringBuilder ();
				int position = words.Count * wordLength;
				foreach (InteractiveLetter l in newWord) {
					
						
			     GameObject letterInWord = lettersOfWordInHistory.CreateLetterBarCell (l.InputLetter (), l.CurrentDisplayImage (), (position++) + "", (SessionsDirector.IsSyllableDivisionActivity?l.SelectColour:l.ColorDerivedFromInput));
				

						letterInWord.AddComponent<BoxCollider> ();
						letterInWord.AddComponent<UIDragPanelContents> ();
				
						UIDragPanelContents drag = letterInWord.GetComponent<UIDragPanelContents> ();
						drag.draggablePanel = gameObject.GetComponent<UIDraggablePanel> ();
						currentWordAsString.Append (l.InputLetter ());
						
				}
				wordHistoryGrid.GetComponent<UIGrid> ().Reposition ();
				return currentWordAsString.ToString ().Trim ().ToLower ();


		}

		public void ClearWordHistory ()
		{
				words.Clear ();
				//set the letter and display color of each word in history to blank
				List<InteractiveLetter> letters = lettersOfWordInHistory.GetLetters (false);
				foreach (InteractiveLetter letter in letters) {
					letter.UpdateInputLetterAndInputDerivedColor (" ", 
				    lettersOfWordInHistory.GetAppropriatelyScaledImageForLetter(" "), Color.white);
				}
		}

		Word CreateNewWordAndAddToList (string newWordAsString)
		{
				Word newWord = new Word (newWordAsString);

				newWord.Sound = AudioSourceController.GetWordFromResources (newWordAsString);
				words.Add (newWord);
				return newWord;

		}
	
		public void PlayWordOfPressedLetter (GameObject pressedLetterCell)
		{
				InteractiveLetter l = pressedLetterCell.GetComponent<InteractiveLetter> ();
				if (l.IsBlank ()) 
						return;
				Word wordThatLettersBelongTo = RetrieveWordGivenLetterAndIndex (l, IndexOfWordThatLetterBelongsTo (pressedLetterCell));
				AudioSourceController.PushClip (wordThatLettersBelongTo.Sound);
			
		}

		int IndexOfWordThatLetterBelongsTo (GameObject pressedLetterCell)
		{
				return (Int32.Parse (pressedLetterCell.name)) / wordLength;

		}
	
		Word RetrieveWordGivenLetterAndIndex (InteractiveLetter pressedLetter, int idx)
		{
				if (idx > -1 && idx < words.Count)
						return words [idx];
				return PsuedoWord;

		}


}
