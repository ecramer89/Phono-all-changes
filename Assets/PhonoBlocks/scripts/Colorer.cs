using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public abstract class Colorer  {
	//given a decoded (user input) word and an expected input
	//return a list of delegates which when called with an interactive letter will
	//update the default and display colors of the letter and activate a flash routine if required.

	void TurnOffErroneousLetters(string input,  List<InteractiveLetter> UILetters, string target){

	}
	protected abstract void ColorInstancesOfRules (string input, List<InteractiveLetter> UILetters);
	protected abstract void ConfigureFlashFeedback (string input, string previous, List<InteractiveLetter> UILetters, string target);


}


//
public class MagicEColorer : Colorer{
	
	protected override void ColorInstancesOfRules (string input,  List<InteractiveLetter> UIletters){
		int coloredLetters = 0;
		foreach (Match magicEMatch in Decoder.MagicERegex.Matches(input)) {
			if (coloredLetters == UIletters.Count)
				break;

			int start = magicEMatch.Index;
			int end = start + magicEMatch.Length;
			string magicE = input.Substring (start, magicEMatch.Length);

		

			coloredLetters += magicEMatch.Length;

		}
	}

	protected override void ConfigureFlashFeedback(string input, string previous, List<InteractiveLetter> UILetters, string target){

	}

}
