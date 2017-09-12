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
	public void SetTargetWord(string targetWord){
		OnTargetWordSet (targetWord);
	}

	public event Action<Color[]> OnTargetColorsSet = (Color[] targetColors) => {};
	public void SetTargetColors(Color[] targetColors){
		OnTargetColorsSet (targetColors);
	}


	void Awake(){
		events = this;
		//gaurantees that state is first subscriber whose methods are invoked when new events occur.
		//saves other classes that need to frequently refer to these data from having to cache additional
		//references/copies to/of data
		State state = GetComponent<State> ();
		state.SubscribeToEvents ();
	}









}




