using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		Events.Dispatcher.OnActivitySelected += (Activity activity) => {
			this.activity = activity;
		};

		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			this.mode = mode;
		};

		Events.Dispatcher.OnUILettersCreated += (List<InteractiveLetter> letters) => {
			this.uILetters = letters;
		};

		Events.Dispatcher.OnTargetWordSet += (string targetWord) => {
			this.targetWord = targetWord;
		};

		Events.Dispatcher.OnTargetColorsSet += (Color[] targetWordColors) => {
			this.targetWordColors = targetWordColors;
		};

	}

	private Mode mode;
	public Mode Mode{
		get {
			return mode;
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

	private string targetWord;
	public string TargetWord{
		get {

			return targetWord;
		}
	}

	private Color[] targetWordColors;
	public Color[] TargetWordColors{
		get {
			return targetWordColors;
		}

	}











}
