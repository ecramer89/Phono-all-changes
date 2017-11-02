using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;
using System.Text.RegularExpressions;
using System.Linq;
public class PhonoBlocksState : PhonoBlocksSubscriber {

	public override void SubscribeToAll(PhonoBlocksScene nextToLoad){
		if(nextToLoad == PhonoBlocksScene.MainMenu){

			Transaction.Instance.MainMenuNavigationStateChanged.Subscribe(this,(ParameterlessEvent navback)=>{
				mainMenuNavigationStack.Push(navback);
			});

			Transaction.Instance.InputTypeSelected.Subscribe(this,
				(InputType type) => {
					inputType = type;
				}
			);

			Transaction.Instance.ActivitySelected.Subscribe(this,(Activity activity) => {
				this.activity = activity;
			});

			Transaction.Instance.ModeSelected.Subscribe(this,(Mode mode) => {
				this.mode = mode;
			});

			Transaction.Instance.SessionSelected.Subscribe(this,(int session) => {
				this.session = session;
			});


		}

		if(nextToLoad == PhonoBlocksScene.Activity){
			Transaction.Instance.ActivitySceneLoaded.Subscribe(this,() => {
				mainMenuNavigationStack = new Stack<ParameterlessEvent>(); //clear stack, though it should never be accessed
				//within activity mode regardless
				previousUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				userInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				selectedUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				resetWordColorShowState();
			});
			Transaction.Instance.InteractiveLettersCreated.Subscribe(this,(List<InteractiveLetter> letters) => {
				this.uILetters = letters;
			});

			Transaction.Instance.TargetColorsSet.Subscribe(this,(Color[] targetWordColors) => {
				this.targetWordColors = targetWordColors;
			});

			Transaction.Instance.NewProblemBegun.Subscribe(this,(ProblemData problem) => {
				placeHolderLetters = problem.initialWord;
				this.targetWord = problem.targetWord;
				currentProblemInstrutions = problem.instructions;
				timesAttemptedCurrentProblem = 0;
				previousUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				userInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				selectedUserInputLetters = _String.Fill(" ", Parameters.UI.ONSCREEN_LETTER_SPACES);
				currentHintNumber = 0;
				resetWordColorShowState();
			});

			Transaction.Instance.TimesAttemptedCurrentProblemIncremented.Subscribe(this,() => {
				timesAttemptedCurrentProblem++;
			});


			Transaction.Instance.UserEnteredNewLetter.Subscribe(this,(char newLetter, int atPosition) => {
		
				previousUserInputLetters = userInputLetters;
				userInputLetters = userInputLetters.ReplaceAt(atPosition, newLetter);

			});

			Transaction.Instance.StudentModeMainActivityEntered.Subscribe(this,() => {
				studentModeState = StudentModeStates.MAIN_ACTIVITY;
			});
			Transaction.Instance.StudentModeForceCorrectLetterPlacementEntered.Subscribe(this,() => {
				studentModeState = StudentModeStates.FORCE_CORRECT_LETTER_PLACEMENT;
			});
			Transaction.Instance.StudentModeForceRemoveAllLettersEntered.Subscribe(this,() => {
				studentModeState = StudentModeStates.REMOVE_ALL_LETTERS;
			});

			Transaction.Instance.UIInputUnLocked.Subscribe(this,() => {
				uIInputLocked = false;
			});
			Transaction.Instance.UIInputLocked.Subscribe(this,() => {
				uIInputLocked = true;
			});
			Transaction.Instance.UserSubmittedIncorrectAnswer.Subscribe(this,() => {
				this.hintAvailable=this.currentHintNumber < Parameters.Hints.NUM_HINTS;
			});
			Transaction.Instance.HintProvided.Subscribe(this,() => {
				this.hintAvailable = false;
				this.currentHintNumber++;
			});
			Transaction.Instance.WordColorShowStateSet.Subscribe(this,(WordColorShowStates newState) => {
				wordColorShowState = newState;
			});
		
			Transaction.Instance.InteractiveLetterSelected.Subscribe(this,(InteractiveLetter letter) => {
				selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
					userInputLetters[letter.Position]);
			});
			Transaction.Instance.InteractiveLetterDeselected.Subscribe(this,(InteractiveLetter letter) => {
				selectedUserInputLetters = selectedUserInputLetters.ReplaceAt(letter.Position,
					' ');
			});
			Transaction.Instance.TargetWordSyllablesSet.Subscribe(this,(List<Match> syllables) => {
				this.targetWordSyllables = syllables;

			});

		}


	}

	void resetWordColorShowState(){
		wordColorShowState = activity == Activity.SYLLABLE_DIVISION ?
			WordColorShowStates.SHOW_WHOLE_WORD :
			WordColorShowStates.SHOW_TARGET_UNITS;
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

	//whether should show the letters in the whole word color
	//or color code by target unit
	private WordColorShowStates wordColorShowState;
	public WordColorShowStates WordColorShowState{
		get {
			return wordColorShowState;
		}

	}
	//only relevant to syllable division mode
	private List<Match> targetWordSyllables;
	public List<Match> TargetWordSyllables{
		get {
			return targetWordSyllables;

		}

	}


	/*stack of actions that would undo the last main menu navigation state change */
	private Stack<ParameterlessEvent> mainMenuNavigationStack = new Stack<ParameterlessEvent>();
	public Stack<ParameterlessEvent> MainMenuNavigationStack{
		get {
			return mainMenuNavigationStack;//probably not best form to return entire stack, 
			//which is mutable, but...
		}

	}



}
