using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhonoBlocksEvents {

	static PhonoBlocksEvents instance;
	public static PhonoBlocksEvents Instance {
		get {
			return instance;
		}
	}


	//todo mode should be an enum
	//teacher or student mode
	public delegate void ModeSelected(Mode mode);
	public event ModeSelected onModeSelected = (Mode mode) => {};

	//i.e., consonant digraphs, magic e rule, etc.
	//to do activity should be an enum
	public delegate void ActivitySelected(Activity activity);
	public event ActivitySelected onActivitySelected = (Activity activity) => {};


	//fired by: 
	/*
	 * 
	 * 
	 * 
	 * */
	public delegate void NewProblemBegun(string targetWord, string initialLetters);
	public event NewProblemBegun onNewProblemBegun = 
		(string targetWord, string initialLetters) => {};


	public delegate void UserEnteredLetter(char newLetter, int atPostion);
	//fired by: 
	/*
	 * 
	 * 
	 * 
	 * */
	public event UserEnteredLetter onUserEnteredNewLetter = 
		(char newLetter, int atPosition) => {};

	public delegate void UserControlledLettersUpdated(string userControlledLetters);
	public event UserControlledLettersUpdated onUserControlledLettersUpdated = 
		(string userControlledLetters) =>{};

	public delegate void UserSubmittedAnswer(bool wasCorrect);
	public event UserSubmittedAnswer onUserSubmittedAnswer = (bool wasCorrect)=>{};

	public delegate void UserRequestedHint();
	public event UserRequestedHint onUserRequestedHint = () => {};

	public delegate void AnswerToCurrentProblemProvided();
	public event AnswerToCurrentProblemProvided onAnswerToCurrentProblemProvided = () => {};


}
