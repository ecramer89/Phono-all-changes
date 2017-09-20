﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;


[RequireComponent(typeof(State))]
[RequireComponent(typeof(Selector))]
public class Dispatcher: MonoBehaviour  {

	private IEnumerator<Action> dispatch;
	private Queue<PhonoBlocksEvent> events=new Queue<PhonoBlocksEvent>();

	private static Dispatcher instance;
	public static Dispatcher Instance{
		get {
			return instance;

		}
	}
		

	public UnaryTypedEvent<string> TestEventA = new UnaryTypedEvent<string>();
	public UnaryTypedEvent<string> TestEventB = new UnaryTypedEvent<string>();



	public void EnqueueEvent(PhonoBlocksEvent evt){
		events.Enqueue(evt);

	}


	public void Update(){

		if(dispatch != null && dispatch.MoveNext()){
			dispatch.Current();
			return;
		}

		if(events.Count > 0){
			List<Action> subs = events.Dequeue().Subscribers();
			dispatch = subs.GetEnumerator();

		}

	}


	public event Action<InputType> OnInputTypeSelected = (InputType mode) => {};
	public void RecordInputTypeSelected(InputType mode){
		OnInputTypeSelected (mode);
	}
		

	public event Action<Mode> OnModeSelected = (Mode mode) => {};
	public void RecordModeSelected(Mode mode){
		OnModeSelected (mode);
	}

	public event Action<string> OnStudentNameEntered = (string name) => {};
	public void RecordStudentNameEntered(string name){
		OnStudentNameEntered (name);
	}

	public event Action OnStudentDataRetrieved = ()=>{};
	public void RecordStudentDataRetrieved(){
		OnStudentDataRetrieved ();
	}

	public event Action<int> OnSessionSelected = (int session) => {};
	public void RecordSessionSelected(int session){
		OnSessionSelected (session);
	}


	public event Action<Activity> OnActivitySelected = (Activity activity)=>{};
	public void RecordActivitySelected(Activity activity){
		OnActivitySelected (activity);
	}

	public event Action<List<InteractiveLetter>> OnUILettersCreated = (List<InteractiveLetter> UILetters)=>{};
	public void UILettersCreated(List<InteractiveLetter> UILetters){
		OnUILettersCreated(UILetters);
	}


	public event Action<Color[]> OnTargetColorsSet = (Color[] targetColors) => {};
	public void SetTargetColors(Color[] targetColors){
		OnTargetColorsSet (targetColors);
	}


	public event Action OnTimesAttemptedCurrentProblemIncremented = () => {};
	public void IncrementTimesAttemptedCurrentProblem(){
		OnTimesAttemptedCurrentProblemIncremented ();
	}
		

	public event Action OnUserInputLettersUpdated = () => {};
	public event Action<char, int> OnUserEnteredNewLetter = (char newLetter, int atPosition) => {};
	public void RecordNewUserInputLetter(char newLetter, int atPosition){
		OnUserEnteredNewLetter (newLetter, atPosition);
		OnUserInputLettersUpdated ();
	}

	public event Action<ProblemData> OnNewProblemBegun = (ProblemData problem) => {};
	public void RecordNewProblemBegun(ProblemData problem){
		OnNewProblemBegun (problem);
	}
	//distinguished from activity begun; may need to transition back into main activity state from a different state,
	//i.e. such as force correct letter placement.
	public event Action OnEnterStudentModeMainActivity = () => {};
	public void EnterStudentModeMainActivity(){
		OnEnterStudentModeMainActivity ();
	}
	public event Action OnEnterForceCorrectLetterPlacement = () => {};
	public void ForceCorrectLetterPlacement(){
		OnEnterForceCorrectLetterPlacement ();
	}
	public event Action OnEnterForceRemoveAllLetters = () => {};
	public void ForceRemoveAllLetters(){
		OnEnterForceRemoveAllLetters ();
	}

	public event Action OnUIInputLocked = () => {};
	public void LockUIInput(){
		OnUIInputLocked ();
	}
	public event Action OnUIInputUnLocked = ()=>{};
	public void UnLockUIInput(){
		OnUIInputUnLocked ();
	}

	//STUDENT MODE, TEACHER MODE
	//intermediary event fired before one of user submitted incorrect answer or current problem completed (in student mode) OR
	//user added word to history (teacher mode). 
	//One of student or teacher mode subscribes to this event and determines which event(s) to fire as consequence.
	public event Action OnUserSubmittedTheirLetters = () => {};
	public void RecordUserSubmittedTheirLetters(){
		OnUserSubmittedTheirLetters ();
	}

	public event Action OnHintRequested = () => {};
	public void RecordUserRequestedHint(){
		OnHintRequested ();
	}

	//STUDENT MODE
	//fired if user presses check word button and current state of user input letters does not match target
	public event Action OnUserSubmittedIncorrectAnswer = () => {};
	public void RecordUserSubmittedIncorrectAnswer(){
		OnUserSubmittedIncorrectAnswer ();
	}

	//STUDENT MODE
	//fired once for each hint, after each successive hint is provided.
	public event Action OnHintProvided = () => {};
	public void RecordHintProvided(){
		OnHintProvided ();
	}

	//STUDENT MODE
	//fired if user presses check word button and current state of user input letters matces target word.
	public Action OnCurrentProblemCompleted = () =>{};
	public void RecordCurrentProblemCompleted(){
		OnCurrentProblemCompleted ();
	}

	//TEACHER MODE
	//fired when the user presses the check word button
	public Action OnUserAddedWordToHistory = () => {};
	public void RecordUserAddedWordToHistory(){
		OnUserAddedWordToHistory ();
	}

	//STUDENT MODE
	//fired when the user completes all of the problems for a particular session.
	//e.g., for session 1, user has to spell:
	//"bet","dad","tin". 
	//OnSessionCompleted dispatched after Current Problem Completed when target word is "tin".
	public Action OnSessionCompleted = () => {};
	public void RecordSessionCompleted(){
		OnSessionCompleted ();
	}

	public Action OnSyllableDivisionShowStateToggled = () => {};
	public void RecordSyllableDivisionShowStateToggled(){
		OnSyllableDivisionShowStateToggled ();
	}

	public event Action<InteractiveLetter> OnInteractiveLetterSelected = (InteractiveLetter letter) => {};
	public void RecordInteractiveLetterSelected(InteractiveLetter letter){
		OnInteractiveLetterSelected(letter);
	}
	public event Action<InteractiveLetter> OnInteractiveLetterDeSelected = (InteractiveLetter letter) => {};
	public void RecordInteractiveLetterDeSelected(InteractiveLetter letter){
		OnInteractiveLetterDeSelected(letter);
	}
	public event Action<List<Match>> OnTargetWordSyllablesSet = (List<Match> syllables) => {};
	public void RecordTargetWordSyllablesSet(List<Match> syllables){
		OnTargetWordSyllablesSet(syllables);
	}


	void Awake(){
		instance = this;
		//gaurantees that state is first subscriber whose methods are invoked when new events occur.
		//saves other classes that need to frequently refer to these data from having to cache additional
		//references/copies to/of data
		State state = GetComponent<State> ();
		state.SubscribeToEvents ();
		//ensure that selector is the second whose subscribers are invoked.
		//selectors job is to compute/cache derived fields (e.g., whether or not current state of user input matches target,
		//the location of each error, so on).
		Selector selector = GetComponent<Selector> ();
		selector.SubscribeToEvents ();



	}


		


}





