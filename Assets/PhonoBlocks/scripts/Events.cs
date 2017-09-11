using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(State))]
public class Events: MonoBehaviour  {

	private static Events events;
	public static Events Dispatcher{
		get {
			return events;

		}
	}
		

	public event Action<Mode> OnModeSelected = (Mode mode) => {};
	public void ModeSelected(Mode mode){
		OnModeSelected (mode);
	}


	public event Action<Activity> OnActivitySelected = (Activity activity)=>{};
	public void ActivitySelected(Activity activity){
		OnActivitySelected (activity);
	}

	public event Action<List<InteractiveLetter>> OnUILettersCreated = (List<InteractiveLetter> UILetters)=>{};
	public void UILettersCreated(List<InteractiveLetter> UILetters){
		OnUILettersCreated(UILetters);
	}


	public event Action<string> OnTargetWordSet = (string targetWord) => { };
	public void TargetWordSet(string targetWord){

	}


	void Awake(){
		events = this;
		State state = GetComponent<State> ();
		state.SubscribeToEvents ();
	}









}




