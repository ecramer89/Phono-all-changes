using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;
public class State {


	public void SubscribeToEvents(){

		Dispatcher.Instance.ActivitySceneLoaded.Subscribe(() => {
			previousUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			userInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			selectedUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
		});

		Dispatcher.Instance.InputTypeSelected.Subscribe(
			(InputType type) => {
				inputType = type;
			}
		);

		Dispatcher.Instance.ActivitySelected.Subscribe((Activity activity) => {
			this.activity = activity;
			if(activity == Activity.SYLLABLE_DIVISION){
				syllableDivisionShowState = SyllableDivisionShowStates.SHOW_WHOLE_WORD;
			}
		});

		Dispatcher.Instance.ModeSelected.Subscribe((Mode mode) => {
			this.mode = mode;
		});

		Dispatcher.Instance.SessionSelected.Subscribe((int session) => {
			this.session = session;
		});

		Dispatcher.Instance.InteractiveLettersCreated.Subscribe((List<InteractiveLetter> letters) => {
			this.uILetters = letters;
		});
			
		Dispatcher.Instance.TargetColorsSet.Subscribe((Color[] targetWordColors) => {
			this.targetWordColors = targetWordColors;
		});

		Dispatcher.Instance.NewProblemBegun.Subscribe((ProblemData problem) => {
			placeHolderLetters = problem.initialWord;
			this.targetWord = problem.targetWord;
			currentProblemInstrutions = problem.instructions;
			timesAttemptedCurrentProblem = 0;
			previousUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			userInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			selectedUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			currentHintNumber = 0;
			//only matters in syllable division activity, but may as well reset whenever.
			syllableDivisionShowState = SyllableDivisionShowStates.SHOW_WHOLE_WORD;

		});
	
		Dispatcher.Instance.TimesAttemptedCurrentProblemIncremented.Subscribe(() => {
			timesAttemptedCurrentProblem++;
		});
			
	 
		Dispatcher.Instance.UserEnteredNewLetter.Subscribe((char newLetter, int atPosition) => {
			previousUserInputLetters = userInputLetters;
			userInputLetters = userInputLetters.ReplaceAt(atPosition, newLetter);
				
		});
	
		Dispatcher.Instance.StudentModeMainActivityEntered.Subscribe(() => {
			studentModeState = StudentModeStates.MAIN_ACTIVITY;
		});
		Dispatcher.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(() => {
			studentModeState = StudentModeStates.FORCE_CORRECT_LETTER_PLACEMENT;
		});
		Dispatcher.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(() => {
			studentModeState = StudentModeStates.REMOVE_ALL_LETTERS;
		});

		Dispatcher.Instance.UIInputUnLocked.Subscribe(() => {
			uIInputLocked = false;
		});
		Dispatcher.Instance.UIInputLocked.Subscribe(() => {
			uIInputLocked = true;
		});
		Dispatcher.Instance.UserSubmittedIncorrectAnswer.Subscribe(() => {
			this.hintAvailable=this.currentHintNumber < Parameters.Hints.NUM_HINTS;
		});
		Dispatcher.Instance.HintProvided.Subscribe(() => {
			this.hintAvailable = false;
			this.currentHintNumber++;
		});
		Dispatcher.Instance.SyllableDivisionShowStateToggled.Subscribe(() => {
			syllableDivisionShowState = syllableDivisionShowState == SyllableDivisionShowStates.SHOW_DIVISION ?
				SyllableDivisionShowStates.SHOW_WHOLE_WORD : SyllableDivisionShowStates.SHOW_DIVISION;
		});
		Dispatcher.Instance.InteractiveLetterSelected.Subscribe((InteractiveLetter letter) => {
			selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
				userInputLetters[letter.Position]);
		});
		Dispatcher.Instance.InteractiveLetterDeselected.Subscribe((InteractiveLetter letter) => {
			selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
				' ');
		});
		Dispatcher.Instance.TargetWordSyllablesSet.Subscribe((List<Match> syllables) => {
			this.targetWordSyllables = syllables;

		});
	
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
	private string userInputLetters=_String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
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
	private string selectedUserInputLetters;
	public string SelectedUserInputLetters{
		get {
			return selectedUserInputLetters;
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


	private StudentModeStates studentModeState;
	public StudentModeStates StudentModeState{
		get {

			return studentModeState;
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


	//only relevant to syllable division mode
	private SyllableDivisionShowStates syllableDivisionShowState;
	public SyllableDivisionShowStates SyllableDivisionShowState{
		get {
			return syllableDivisionShowState;
		}

	}
	//only relevant to syllable division mode
	private List<Match> targetWordSyllables;
	public List<Match> TargetWordSyllables{
		get {
			return targetWordSyllables;

		}

	}



}
