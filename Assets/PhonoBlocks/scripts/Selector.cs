using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Extensions;
public class Selector : MonoBehaviour {

	private static Selector instance;
	public static Selector Instance{
		get {
			return instance;
		}
	}

	public void Start(){
		instance = this;
	}

	public void SubscribeToEvents(){
		Events.Dispatcher.OnNewProblemBegun += () => {
			solvedOnFirstTry = false;
			currentStateOfUserInputMatchesTarget = false;
		};

		Events.Dispatcher.OnTargetWordSet += (string targetWord) => {
			correctlyPlacedLetters = new bool[Parameters.UI.ONSCREEN_LETTER_SPACES];
			//by default, we presume that all of the spaces in which the child has not placed a letter
			//and which are not part of the target word are correctly placed.
			//i.e., when target word is "thin", 
			//there are two spaces left after "n"; since there isn't anything there to begin with, 
			//those spots are correctly placed.
			targetWord = targetWord.Trim();
			for(int i=targetWord.Length;i<correctlyPlacedLetters.Length;i++){
				correctlyPlacedLetters[i] = true;
			}
		};
			
		Events.Dispatcher.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			//a letter at a given position is correctly placed if it's part of the target word and has the matching letter OR
			//it's outside the bounds of target word and is blank.
			correctlyPlacedLetters[atPosition] = 
				(atPosition >= State.Current.TargetWord.Length && newLetter == ' ') ||
				(atPosition <  State.Current.TargetWord.Length && newLetter == State.Current.TargetWord[atPosition]);
			
			currentStateOfUserInputMatchesTarget = correctlyPlacedLetters.All(placement => placement);
		};

		Events.Dispatcher.OnCurrentProblemCompleted += () => {
			solvedOnFirstTry = State.Current.TimesAttemptedCurrentProblem == 1;
		};
	}


	private bool currentStateOfUserInputMatchesTarget;
	public bool CurrentStateOfInputMatchesTarget{
		get {
			return currentStateOfUserInputMatchesTarget;
		}

	}
	private bool[] correctlyPlacedLetters;
	public bool IsCorrectlyPlaced(int atPosition){
		return correctlyPlacedLetters[atPosition];
	}

	private bool solvedOnFirstTry;
	public bool SolvedOnFirstTry{
		get {
			return solvedOnFirstTry;
		}
	}
}
