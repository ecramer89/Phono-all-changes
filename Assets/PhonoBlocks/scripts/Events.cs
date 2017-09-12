﻿using System.Collections;
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


	public event Action OnTimesAttemptedCurrentProblemIncremented = () => {};
	public void IncrementTimesAttemptedCurrentProblem(){
		OnTimesAttemptedCurrentProblemIncremented ();
	}

	public event Action<AudioClip[]> OnCurrentProblemInstructionsSet = (AudioClip[] instructions) => {};
	public void SetCurrentProblemInstructions(AudioClip[] instructions){
		OnCurrentProblemInstructionsSet (instructions);
	}

	public event Action<string> OnInitialProblemLettersSet = (string initialLetters) => {};
	public void SetInitialProblemLetters(string initial){
		OnInitialProblemLettersSet (initial);
	}


	public event Action OnUserInputLettersUpdated = () => {};
	public event Action<char, int> OnUserEnteredNewLetter = (char newLetter, int atPosition) => {};
	public void RecordNewUserInputLetter(char newLetter, int atPosition){
		OnUserEnteredNewLetter (newLetter, atPosition);
		OnUserInputLettersUpdated ();
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



