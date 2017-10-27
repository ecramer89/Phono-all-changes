using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhonoBlocksState), typeof(PhonoBlocksSelector))]
public class Transaction: MonoBehaviour  {
	[SerializeField] GameObject destroyOnReturnToMainMenu; //Game objects that persist between main menu and activity and which need to be explicitly destroyed before returning to the main menu
	private IEnumerator<Action> dispatch; //list of event handler actions for the current event
	private Queue<PhonoBlocksEvent> events=new Queue<PhonoBlocksEvent>(); //queue of events that are waiting to be dispatched

	private static Transaction instance;
	public static Transaction Instance{
		get {
			return instance;

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


	public UnaryParameterizedEvent<SyllableDivisionShowStates> SyllableDivisionShowStateSet = new UnaryParameterizedEvent<SyllableDivisionShowStates>("SyllableDivisionShowStateSet");
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

	private List<PhonoBlocksEvent> phonoblocksEvents;
	private PhonoBlocksState state;
	public PhonoBlocksState State{
		get {
			return state;
		}
		
	}

	private PhonoBlocksSelector selector;
	public PhonoBlocksSelector Selector{
		get {
			return selector;

		}

	}


	public void Awake(){

		instance = this;
		state = GetComponent<PhonoBlocksState>();
		selector =GetComponent<PhonoBlocksSelector>();
		phonoblocksEvents = new List<PhonoBlocksEvent>();
		foreach(FieldInfo field in typeof(Transaction).GetFields()){
			var value = field.GetValue(this);
			if(value != null && typeof(PhonoBlocksEvent).IsAssignableFrom(value.GetType())){
				phonoblocksEvents.Add((PhonoBlocksEvent)value);
			}

		}
		SceneManager.sceneLoaded += TellSubscribersToRegisterToEvents;
	}

	void TellSubscribersToRegisterToEvents(Scene scene, LoadSceneMode mode){
		foreach(PhonoBlocksEvent evt in phonoblocksEvents){
			evt.ClearSubscribers();

		}


		PhonoBlocksScene currentScene = scene.name == 
			PhonoBlocksScene.MainMenu.ToString() ? PhonoBlocksScene.MainMenu : PhonoBlocksScene.Activity;

		PhonoBlocksSubscriber[] subscribersInScene = FindObjectsOfType(typeof(PhonoBlocksSubscriber)) as PhonoBlocksSubscriber[];
		Array.Sort(subscribersInScene, //sort ascending by priorty (higher value => higher priority) so that we subscribe the state and selector first.
			(PhonoBlocksSubscriber left, PhonoBlocksSubscriber right)=>right.Priority-left.Priority);


		if(subscribersInScene == null) return;  //check safety of cast
		foreach(PhonoBlocksSubscriber subscriber in subscribersInScene){

			subscriber.SubscribeToAll(currentScene); 
			Debug.Log($"Subscriber: {subscriber.GetType()}");
		}


		if(currentScene == PhonoBlocksScene.Activity){
			ActivitySceneLoaded.Fire();
		}

	}


	public void EnqueueEvent(PhonoBlocksEvent evt){
		events.Enqueue(evt);

	}



	public void Update(){

		if(dispatch != null && dispatch.MoveNext()){
			do{
				dispatch.Current();
			}while(dispatch.MoveNext());

			return;
		}

		if(events.Count > 0){
			dispatch = events.Dequeue().SubscriptionHandlers();
		}

	}
	//invoked when home button press 
	//home button activity->main menu 
	public void Restart(){
		
		foreach(PhonoBlocksEvent evt in phonoblocksEvents){
			evt.ClearSubscribers();
		}

		SceneManager.sceneLoaded-=TellSubscribersToRegisterToEvents;


		Destroy(destroyOnReturnToMainMenu);
		
		SceneManager.LoadScene(PhonoBlocksScene.MainMenu.ToString());

	}



}






