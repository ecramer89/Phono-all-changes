using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;
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


		SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => {
			if(scene.name == "Activity"){
				previousUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				userInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				selectedUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
			}
		
		};

		Dispatcher.Instance.InputTypeSelected.Subscribe(
			(InputType type) => {
				inputType = type;
			}
		);

		Dispatcher.Instance.OnActivitySelected += (Activity activity) => {
			this.activity = activity;
			if(activity == Activity.SYLLABLE_DIVISION){
				syllableDivisionShowState = SyllableDivisionShowStates.SHOW_WHOLE_WORD;
			}
		};

		Dispatcher.Instance.ModeSelected.Subscribe((Mode mode) => {
			this.mode = mode;
		});

		Dispatcher.Instance.OnSessionSelected += (int session) => {
			this.session = session;
		};

		Dispatcher.Instance.OnUILettersCreated += (List<InteractiveLetter> letters) => {
			this.uILetters = letters;
		};
			
		Dispatcher.Instance.OnTargetColorsSet += (Color[] targetWordColors) => {
			this.targetWordColors = targetWordColors;
		};

		Dispatcher.Instance.OnNewProblemBegun += (ProblemData problem) => {
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

		};
	
		Dispatcher.Instance.OnTimesAttemptedCurrentProblemIncremented += () => {
			timesAttemptedCurrentProblem++;
		};
			
	 
		Dispatcher.Instance.OnUserEnteredNewLetter += (char newLetter, int atPosition) => {
			previousUserInputLetters = userInputLetters;
			userInputLetters = userInputLetters.ReplaceAt(atPosition, newLetter);
				
		};
	
		Dispatcher.Instance.OnEnterStudentModeMainActivity += () => {
			studentModeState = StudentModeStates.MAIN_ACTIVITY;
		};
		Dispatcher.Instance.OnEnterForceCorrectLetterPlacement += () => {
			studentModeState = StudentModeStates.FORCE_CORRECT_LETTER_PLACEMENT;
		};
		Dispatcher.Instance.OnEnterForceRemoveAllLetters += () => {
			studentModeState = StudentModeStates.REMOVE_ALL_LETTERS;
		};

		Dispatcher.Instance.OnUIInputUnLocked += () => {
			uIInputLocked = false;
		};
		Dispatcher.Instance.OnUIInputLocked += () => {
			uIInputLocked = true;
		};
		Dispatcher.Instance.OnUserSubmittedIncorrectAnswer += () => {
			this.hintAvailable=this.currentHintNumber < Parameters.Hints.NUM_HINTS;
		};
		Dispatcher.Instance.OnHintProvided += () => {
			this.hintAvailable = false;
			this.currentHintNumber++;
		};
		Dispatcher.Instance.OnSyllableDivisionShowStateToggled += () => {
			syllableDivisionShowState = syllableDivisionShowState == SyllableDivisionShowStates.SHOW_DIVISION ?
				SyllableDivisionShowStates.SHOW_WHOLE_WORD : SyllableDivisionShowStates.SHOW_DIVISION;
		};
		Dispatcher.Instance.OnInteractiveLetterSelected += (InteractiveLetter letter) => {
			selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
				userInputLetters[letter.Position]);
		};
		Dispatcher.Instance.OnInteractiveLetterDeSelected += (InteractiveLetter letter) => {
			selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
				' ');
		};
		Dispatcher.Instance.OnTargetWordSyllablesSet += (List<Match> syllables) => {
			this.targetWordSyllables = syllables;

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
