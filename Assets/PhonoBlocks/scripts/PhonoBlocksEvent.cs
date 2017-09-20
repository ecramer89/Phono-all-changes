using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public interface PhonoBlocksEvent  {

	List<Action> Subscribers();
}

public class UnaryTypedEvent<T> : PhonoBlocksEvent{
	private List<Action<T>> typedSubscribers;
	private List<Action> generifiedSubscribers;

	public UnaryTypedEvent(){
		typedSubscribers = new List<Action<T>>();
	}

	public void SubscribeWith(Action<T> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Dispatch(T type){
		generifiedSubscribers = new List<Action>();
		foreach(Action<T> subscriber in typedSubscribers){
			generifiedSubscribers.Add(()=>subscriber(type));
		}
		Dispatcher.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}

}






/*
public class OnInputTypeSelected : PhonoBlocksEvent{
	private List<Action<InputType>> typedSubscribers;
	private List<Action> generifiedSubscribers;
	public OnInputTypeSelected(Dispatcher dispatcher) : base(dispatcher){
		typedSubscribers = new List<Action<InputType>>();
	}

	public void SubscribeWith(Action<InputType> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Dispatch(InputType type){
		generifiedSubscribers = typedSubscribers.Select((Action<InputType> subscriber) => ()=>subscriber(type)); 
		dispatcher.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}
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
*/
 
