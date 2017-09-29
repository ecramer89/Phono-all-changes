using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

public class WordHistoryController : PhonoBlocksSubscriber
{

	public override void SubscribeToAll(PhonoBlocksScene forScene){}
		private static WordHistoryController instance;
		public static WordHistoryController Instance{
			get {
				return instance;
			}

		}

		public GameObject wordHistoryPanelBackground;
		LetterImageTable letterImageTable;


		public GameObject wordHistoryGrid;
		LetterGridController lettersOfWordInHistory;
		List<Word> words; //words in the word history.
		Word psuedoWord; //a dummy value to return in case there is some kind of error.

		public void Start(){
			instance = this;
			
		Transaction.Instance.ActivitySceneLoaded.Subscribe(this,() => {
				words = new List<Word> ();
				wordHistoryPanelBackground = GameObject.Find("WordHistoryBackground");
				wordHistoryGrid = GameObject.Find("WordHistoryGrid");
				lettersOfWordInHistory = wordHistoryGrid.gameObject.GetComponent<LetterGridController> ();
				wordHistoryGrid.GetComponent<UIGrid> ().maxPerLine = Parameters.UI.ONSCREEN_LETTER_SPACES;
				letterImageTable = GameObject.Find ("DataTables").GetComponent<LetterImageTable> ();
			});

			//subscribe to events
		    Transaction.Instance.CurrentProblemCompleted.Subscribe(this,AddCurrentWordToHistory);
			Transaction.Instance.UserAddedWordToHistory.Subscribe(this,AddCurrentWordToHistory);
		}

		public int showImageTime = 60 * 8;


		public void AddCurrentWordToHistory ()
		{           
				AddLettersOfNewWordToHistory ();
		        //cache an audio clip and string for each word that gets saved to the History
				Word newWord = CreateNewWordAndAddToList (Transaction.Instance.State.UserInputLetters.Trim());
				AudioSourceController.PushClip (newWord.Sound);
			
		}

		void AddLettersOfNewWordToHistory ()
		{ 
				
				int position = words.Count * Parameters.UI.ONSCREEN_LETTER_SPACES;
				foreach (InteractiveLetter l in Transaction.Instance.State.UILetters) {
					
			     		GameObject letterInWord = lettersOfWordInHistory.CreateLetterBarCell (
							l.CurrentDisplayImage (), 
							(position++) + "", l.ColorFromInput
						);

						letterInWord.GetComponent<InteractiveLetter>().LetterPressed+=PlayWordOfPressedLetter;
						letterInWord.AddComponent<BoxCollider> ();
						letterInWord.AddComponent<UIDragPanelContents> ();
		
				}
				wordHistoryGrid.GetComponent<UIGrid> ().Reposition ();


		}

		public void ClearWordHistory ()
		{
				words.Clear ();
				//set the letter and display color of each word in history to blank
				List<InteractiveLetter> letters = lettersOfWordInHistory.GetLetters ();
				foreach (InteractiveLetter letter in letters) {
					letter.UpdateLetterImageAndInputDerivedColor ( 
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
				Word wordThatLettersBelongTo = RetrieveWordGivenLetterAndIndex (l, IndexOfWordThatLetterBelongsTo (pressedLetterCell));
				AudioSourceController.PushClip (wordThatLettersBelongTo.Sound);
			
		}

		int IndexOfWordThatLetterBelongsTo (GameObject pressedLetterCell)
		{
				return (Int32.Parse (pressedLetterCell.name)) / Parameters.UI.ONSCREEN_LETTER_SPACES;

		}
	
		Word RetrieveWordGivenLetterAndIndex (InteractiveLetter pressedLetter, int idx)
		{
				if (idx > -1 && idx < words.Count)
						return words [idx];
				return psuedoWord;

		}

	//private inner class; just a convenient package for all the data pertaining to a specific word.
	class Word : MonoBehaviour
	{

		string asString;

		public string AsString {
			get {
				return this.asString;

			}


		}

		Texture2D image;

		public Texture2D Image {
			get {
				return this.image;

			}

			set {
				this.image = value;

			}

		}

		AudioClip sound;

		public AudioClip Sound {
			get {
				return this.sound;

			}

			set {
				this.sound = value;

			}



		}

		public Word (string asString_)
		{
			asString = asString_;
			sound = (AudioClip)Resources.Load ("audio/words/" + asString, typeof(AudioClip));
		}

	}


}
