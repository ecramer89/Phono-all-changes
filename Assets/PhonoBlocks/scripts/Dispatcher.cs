using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;



public class Dispatcher: MonoBehaviour  {

	private IEnumerator<Action> dispatch;
	private Queue<PhonoBlocksEvent> events=new Queue<PhonoBlocksEvent>();

	private static Dispatcher instance;
	public static Dispatcher Instance{
		get {
			if(state == null){
				//gaurantees that state is first subscriber whose methods are invoked when new events occur.
				//saves other classes that need to frequently refer to these data from having to cache additional
				//references/copies to/of data
				state = new State();
				state.SubscribeToEvents ();
				//ensure that selector is the second whose subscribers are invoked.
				//selectors job is to compute/cache derived fields (e.g., whether or not current state of user input matches target,
				//the location of each error, so on).
				selector = new Selector();
				selector.SubscribeToEvents ();
			}
			return instance;

		}
	}

	private static State state;
	public static State _State{
		get {
			return state;
		}

	}
	private static Selector selector;
	public static Selector _Selector{
		get {
			return selector;

		}
	}
		
	public ParameterlessEvent StudentDataRetrieved = new ParameterlessEvent("StudentDataRetrieved");
	public ParameterlessEvent TimesAttemptedCurrentProblemIncremented = new ParameterlessEvent("TimesAttemptedCurrentProblemIncremented");
	public ParameterlessEvent UserInputLettersUpdated = new ParameterlessEvent("UserInputLettersUpdated");
	public ParameterlessEvent ActivitySceneLoaded = new ParameterlessEvent("ActivitySceneLoaded");
	public ParameterlessEvent StudentModeMainActivityEntered = new ParameterlessEvent("StudentModeMainActivityEntered");
	public ParameterlessEvent StudentModeForceCorrectLetterPlacementEntered = new ParameterlessEvent("StudentModeForceCorrectLetterPlacementEntered");
	public ParameterlessEvent StudentModeForceRemoveAllLettersEntered = new ParameterlessEvent("StudentModeForceRemoveAllLettersEntered");
	public ParameterlessEvent UIInputLocked = new ParameterlessEvent("UIInputLocked");
	public ParameterlessEvent UIInputUnLocked = new ParameterlessEvent("UIInputUnLocked");
	public ParameterlessEvent UserSubmittedTheirLetters = new ParameterlessEvent("UserSubmittedTheirLetters");
	public ParameterlessEvent HintRequested = new ParameterlessEvent("HintRequested");
	public ParameterlessEvent HintProvided = new ParameterlessEvent("HintProvided");
	public ParameterlessEvent UserSubmittedIncorrectAnswer = new ParameterlessEvent("UserSubmittedIncorrectAnswer");
	public ParameterlessEvent CurrentProblemCompleted = new ParameterlessEvent("CurrentProblemCompleted");
	public ParameterlessEvent UserAddedWordToHistory = new ParameterlessEvent("UserAddedWordToHistory");
	public ParameterlessEvent SessionCompleted = new ParameterlessEvent("SessionCompleted");
	public ParameterlessEvent SyllableDivisionShowStateToggled = new ParameterlessEvent("SyllableDivisionShowStateToggled");

	public UnaryParameterizedEvent<InputType> InputTypeSelected = new UnaryParameterizedEvent<InputType>("InputTypeSelected");
	public UnaryParameterizedEvent<Mode> ModeSelected = new UnaryParameterizedEvent<Mode>("ModeSelected");
	public UnaryParameterizedEvent<string> StudentNameEntered = new UnaryParameterizedEvent<string>("StudentNameEntered");
	public UnaryParameterizedEvent<int> SessionSelected = new UnaryParameterizedEvent<int>("SessionSelected");
	public UnaryParameterizedEvent<List<InteractiveLetter>> InteractiveLettersCreated = new UnaryParameterizedEvent<List<InteractiveLetter>>("InteractiveLettersCreated");
	public UnaryParameterizedEvent<Color[]> TargetColorsSet = new UnaryParameterizedEvent<Color[]>("TargetColorsSet");
	public UnaryParameterizedEvent<Activity> ActivitySelected = new UnaryParameterizedEvent<Activity>("ActivitySelected");
	public UnaryParameterizedEvent<ProblemData> NewProblemBegun = new UnaryParameterizedEvent<ProblemData>("NewProblemBegun");
	public UnaryParameterizedEvent<InteractiveLetter> InteractiveLetterSelected = new UnaryParameterizedEvent<InteractiveLetter>("InteractiveLetterSelected");
	public UnaryParameterizedEvent<InteractiveLetter> InteractiveLetterDeselected = new UnaryParameterizedEvent<InteractiveLetter>("InteractiveLetterDeselected");
	public UnaryParameterizedEvent<List<Match>> TargetWordSyllablesSet = new UnaryParameterizedEvent<List<Match>>("TargetWordSyllablesSet");
	public BinaryParameterizedEvent<char, int> UserEnteredNewLetter = new BinaryParameterizedEvent<char, int>("UserEnteredNewLetter");

	public UnaryParameterizedEvent<string> TestEventA = new UnaryParameterizedEvent<string>("TestA");
	public UnaryParameterizedEvent<string> TestEventB = new UnaryParameterizedEvent<string>("TestB");


	public void EnqueueEvent(PhonoBlocksEvent evt){
		events.Enqueue(evt);

	}

	public void Awake(){
		instance = this;
	}


	public void Update(){
		//...um change to "while movenext" and that should solve the problem!!
		//remember that dispatch is effectively the LIST OF SUBSCRIBER EVENTS!!!
		if(dispatch != null && dispatch.MoveNext()){
			dispatch.Current();
			return;
		}

		if(events.Count > 0){
			List<Action> subs = events.Dequeue().Subscribers();
			dispatch = subs.GetEnumerator();

		}

	}


}






