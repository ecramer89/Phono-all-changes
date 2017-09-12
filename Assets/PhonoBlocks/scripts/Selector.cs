using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
			correctlyPlacedLetters = new bool[Parameters.UI.ONSCREEN_LETTER_SPACES];
			currentStateOfUserInputMatchesTarget = false;
		};
			
		Events.Dispatcher.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			correctlyPlacedLetters[atPosition] = 
				(atPosition >= State.Current.TargetWord.Length && newLetter == ' ') ||
				newLetter == State.Current.TargetWord[atPosition];
			
			currentStateOfUserInputMatchesTarget = correctlyPlacedLetters.All(placements => true);
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
