using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

public class PhonoBlocksState: MonoBehaviour {

	//todo divide up into sep. classes. after enumerating all so we know what they are.
	Mode selectedMode;
	Activity selectedActivity;
	String targetWord;
	String initialLetters;
	String lettersUserHasPlaced; 
	//derived fields of lettersUserHasPlaced:
	bool[] correctLetterPlacements;
	int? indexOfMostRecentLetterChange;

	void Start(){ 

		PhonoBlocksEvents.Instance.onModeSelected += (Mode mode) => {
			selectedMode = mode;
		};

		PhonoBlocksEvents.Instance.onActivitySelected += (Activity activity) => {
			selectedActivity = activity;
		};
			
		PhonoBlocksEvents.Instance.onNewProblemBegun += (string targetWord, string initialLetters) => {
			this.targetWord = targetWord;
			this.initialLetters = initialLetters;
			lettersUserHasPlaced = "".Fill(" ", targetWord.Length);
			indexOfMostRecentLetterChange = null;
			correctLetterPlacements = new bool[targetWord.Length];

		};

		PhonoBlocksEvents.Instance.onUserEnteredNewLetter += (char newLetter, int atPosition) => {
			lettersUserHasPlaced = lettersUserHasPlaced.ReplaceAt(atPosition, newLetter);
			indexOfMostRecentLetterChange = atPosition;
			correctLetterPlacements[atPosition] = newLetter == targetWord[atPosition];
		};

	}


	//state
		//the current target word
		/*   -as string
		 *   -as decoded letter sound components
		 * 
		 * 
		 * student data handler records (for publishing to csv)
		 * 
		 * 
		 * 
		 * */
		//the current letters that user has placed
		//the letters that should currently appear in the grid

		//always/only decode the letters that the user has placed.
		//not those currently appearing on the grid.



	//events:
		//new problem begun:
		/*
		 *  user controlled letters are cleared
		 *  initial word is displayed in the arduino letter controller. all colors are gray. 
		 *  lines appear under the speaces where child has to put in words.
		 *  the image turns off
		 *  th hint button deactivates
		 *  the instructions play
		 *  
		 * 
		 * 
	     //user placed a letter:
	       //-current letters user has placed updates
	       //-the letter appears on the grid
	       //-the current letters user has placed are decoded
		   //-the new colors (based on the decoding of the user letters)
		   //are placed in the letters
	     //user submitted their answer
	     //user requested hint
	*/

}
