using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Extensions;
using System;
public class Selector : MonoBehaviour {

	private static Selector instance;
	public static Selector Instance{
		get {
			return instance;
		}
	}

	public void Start(){
		instance = this;
		Dispatcher.Instance.TestEventA.SubscribeWith((string s)=>{
			Debug.Log($"event a handled in selector {s}");
	
		});
		Dispatcher.Instance.TestEventB.SubscribeWith((string s)=>{
			Debug.Log($"event b handled in selector {s}");
		});
	}

	public void SubscribeToEvents(){
		
			Dispatcher.Instance.OnNewProblemBegun += (ProblemData problem) => {
			solvedOnFirstTry = false;
			currentStateOfUserInputMatchesTarget = false;
			correctlyPlacedLetters = new bool[Parameters.UI.ONSCREEN_LETTER_SPACES];
			//by default, we presume that all of the spaces in which the child has not placed a letter
			//and which are not part of the target word are correctly placed.
			//i.e., when target word is "thin", 
			//there are two spaces left after "n"; since there isn't anything there to begin with, 
			//those spots are correctly placed.
		
			for(int i=problem.targetWord.Length;i<correctlyPlacedLetters.Length;i++){
				correctlyPlacedLetters[i] = true;
			}

			targetWordWithBlanksOnEnd = string.Concat(problem.targetWord, _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES-problem.targetWord.Length));
		};
			
			
		Dispatcher.Instance.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			if(State.Current.Mode == Mode.TEACHER) return; //only relevant in Student mode when there is a target word
			//by which to judge correctness.
			//a letter at a given position is correctly placed if it's part of the target word and has the matching letter OR
			//it's outside the bounds of target word and is blank.
			correctlyPlacedLetters[atPosition] = 
				(atPosition >= State.Current.TargetWord.Length && newLetter == ' ') ||
				(atPosition <  State.Current.TargetWord.Length && newLetter == State.Current.TargetWord[atPosition]);
			
			currentStateOfUserInputMatchesTarget = correctlyPlacedLetters.All(placement => placement);
		};

		Dispatcher.Instance.OnCurrentProblemCompleted += () => {
			solvedOnFirstTry = State.Current.TimesAttemptedCurrentProblem == 1;
		};

		Dispatcher.Instance.OnInteractiveLetterSelected += (InteractiveLetter letter) => {
			allLettersSelected = State.Current.SelectedUserInputLetters == State.Current.UserInputLetters;
			allLettersDeSelected = false;
		};
		Dispatcher.Instance.OnInteractiveLetterDeSelected += (InteractiveLetter letter) => {
			allLettersSelected = false;
			allLettersDeSelected = State.Current.SelectedUserInputLetters.Trim().Length == 0;
		};
	}

	private string targetWordWithBlanksOnEnd;
	public string TargetWordWithBlanksForUnusedPositions{
		get {
			return targetWordWithBlanksOnEnd;
		}
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

	private bool allLettersSelected;
	public bool AllLettersSelected{
		get { 
			return allLettersSelected;
		}

	}

	private bool allLettersDeSelected;
	public bool AllLettersDeSelected{
		get { 
			return allLettersDeSelected;
		}

	}
}
