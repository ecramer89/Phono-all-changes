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


	private Mode mode;
	public Mode Mode{
		get {
			return mode;
		}
	}

	void Start(){
		current = this;

		Events.Dispatcher.OnModeSelected += (Mode mode) => {
			this.mode = mode;
		};
		Events.Dispatcher.OnActivitySelected += (Activity activity) => {
			this.activity = activity;
		};
		Events.Dispatcher.OnUILettersCreated += (List<InteractiveLetter> UILetters) => {
			this.uILetters = UILetters;
		};

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











}
