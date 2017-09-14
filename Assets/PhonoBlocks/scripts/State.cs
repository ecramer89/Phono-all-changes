using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class State: MonoBehaviour  {
	private static State current;
	public static State Current{
		get {
			return current;
		}

	}

	public void Start(){
		current = this;
	}

	public void SubscribeToEvents(){
		Events.Dispatcher.OnInputTypeSelected += (InputType type) => {
			inputType = type;
		};

		Events.Dispatcher.OnActivitySelected += (Activity activity) => {
			this.activity = activity;
		};

		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			this.mode = mode;
		};

		Events.Dispatcher.OnSessionSelected += (int session) => {
			this.session = session;
		};

		Events.Dispatcher.OnUILettersCreated += (List<InteractiveLetter> letters) => {
			this.uILetters = letters;
		};

		Events.Dispatcher.OnTargetWordSet += (string targetWord) => {
			this.targetWord = targetWord;
		};

		Events.Dispatcher.OnTargetColorsSet += (Color[] targetWordColors) => {
			this.targetWordColors = targetWordColors;
		};
		Events.Dispatcher.OnNewProblemBegun += () => {
			timesAttemptedCurrentProblem = 0;
		};
		Events.Dispatcher.OnTimesAttemptedCurrentProblemIncremented += () => {
			timesAttemptedCurrentProblem++;
		};

		Events.Dispatcher.OnCurrentProblemInstructionsSet += (AudioClip[] instuctions) => {
			currentProblemInstrutions = instuctions;
		};

		Events.Dispatcher.OnProblemPlaceholderLettersSet += (string initialLetters) => {
			placeHolderLetters = initialLetters;
		};

		Events.Dispatcher.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			previousUserInputLetters = userInputLetters;
			userInputLetters = userInputLetters.ReplaceAt(atPosition, newLetter);
				
		};
	
		Events.Dispatcher.OnEnterMainActivity += () => {
			activityState = ActivityStates.MAIN_ACTIVITY;
		};
		Events.Dispatcher.OnEnterForceCorrectLetterPlacement += () => {
			activityState = ActivityStates.FORCE_CORRECT_LETTER_PLACEMENT;
		};
		Events.Dispatcher.OnEnterForceRemoveAllLetters += () => {
			activityState = ActivityStates.REMOVE_ALL_LETTERS;
		};
		Events.Dispatcher.OnNewProblemBegun += () => {
			userInputLetters = "".Fill(' ', Parameters.UI.ONSCREEN_LETTER_SPACES);
			currentHintNumber = 0;
		};
		Events.Dispatcher.OnUIInputUnLocked += () => {
			uIInputLocked = false;
		};
		Events.Dispatcher.OnUIInputLocked += () => {
			uIInputLocked = true;
		};
		Events.Dispatcher.OnUserSubmittedIncorrectAnswer += () => {
			this.hintAvailable=this.currentHintNumber < Parameters.Hints.NUM_HINTS;
		};
		Events.Dispatcher.OnHintProvided += () => {
			this.hintAvailable = false;
			this.currentHintNumber++;
		};
	
	}

	private InputType inputType;
	public InputType InputType{
		get {
			return inputType;
		}
	}

	private Mode mode;
	public Mode Mode{
		get {
			return mode;
		}
	}

	private int session;
	public int Session {
		get {
			return session;
		}
	}

	private bool uIInputLocked;
	public bool UIInputLocked{
		get {

			return uIInputLocked;
		}

	}


	private Activity activity;
	public Activity Activity{
		get {
			return activity;
		}
	}


	private List<InteractiveLetter> uILetters;
	public List<InteractiveLetter> UILetters{
		get {

			return uILetters;
		}

	}

	private string previousUserInputLetters;
	private string userInputLetters;
	public string UserInputLetters{
		get {

			return userInputLetters;
		}

	}

	public string PreviousUserInputLetters{
		get {
			return previousUserInputLetters;
		}

	}

	private string targetWord;
	public string TargetWord{
		get {

			return targetWord;
		}
	}
		

	private string placeHolderLetters;
	public string PlaceHolderLetters{
		get {

			return placeHolderLetters;
		}

	}


	private Color[] targetWordColors;
	public Color[] TargetWordColors{
		get {
			return targetWordColors;
		}

	}


	private int timesAttemptedCurrentProblem;
	public int TimesAttemptedCurrentProblem{
		get {
			return timesAttemptedCurrentProblem;
		}
	}

	private AudioClip[] currentProblemInstrutions;
	public AudioClip[] CurrentProblemInstructions{
		get {
			return currentProblemInstrutions;
		}

	}


	private ActivityStates activityState;
	public ActivityStates ActivityState{
		get {

			return activityState;
		}

	}

	private bool hintAvailable;
	public bool HintAvailable{
		get {
			return hintAvailable;
		}
	}

	private int currentHintNumber;
	public int CurrentHintNumber{
		get {
			return currentHintNumber;
		}

	}

}
