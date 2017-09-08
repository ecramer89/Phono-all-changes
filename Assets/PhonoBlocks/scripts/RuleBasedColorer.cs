using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Extensions;
	
public class Colorer  {

	static Color onColor = Color.white;
	static Color offColor = Color.gray;
	static RuleBasedColorer magicEColorer = new MagicEColorer();
	static RuleBasedColorer openClosedVowelColorer = new OpenClosedVowelColorer();

	static RuleBasedColorer ruleBasedColorer = magicEColorer; //todo, make a GO, start, subscribes to event; color rule selected;
	//nees to persist bw levels. depending on whichrule selected instantiates appropriate rule based colorer


	static Action<string, List<InteractiveLetter>> ReapplyDefaultOrOff = (string updatedUserInputLetters,List<InteractiveLetter> UILetters) => {
		int index = 0;
		UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters [index] == ' ' ? offColor : onColor);
			index++;
		});
	};
		

	static event Action ResetAllInteractiveLetterFlashConfigurations = () => {};
	static event Action StartAllInteractiveLetterFlashes = ()=>{};

	public static void RegisterLettersToColorer(List<InteractiveLetter> UILetters){
		UILetters.ForEach (UILetter => {
			StartAllInteractiveLetterFlashes += UILetter.StartFlash;
			ResetAllInteractiveLetterFlashConfigurations += UILetter.ResetFlashParameters;
		});
	}


	public static void ReColor (string updatedUserInputLetters,string previousUserInputLetters, List<InteractiveLetter> UILetters, string targetWord){

		ResetAllInteractiveLetterFlashConfigurations ();
	
		ReapplyDefaultOrOff (updatedUserInputLetters, UILetters);

		ruleBasedColorer.ColorAndConfigureFlashForStudentMode (
			updatedUserInputLetters, 
			previousUserInputLetters,
			UILetters,
			targetWord
		);


		StartAllInteractiveLetterFlashes ();

	
	}

	//todo: color schemes that return a reduced or otherwise modified list of UI letters can't do that here sicne the indices need to stay aligned.
	public static void TurnOffAndConfigureFlashForErroneousLetters(string input, string previousInput, List<InteractiveLetter> UILetters, string target){
		for (int i = 0; i < UILetters.Count; i++) {
			InteractiveLetter UILetter = UILetters [i];
			//index can exceed length of target word since target word doesn't take trailing blanks into account. (e.g., "_ame___", vs "game")

			//if the letter is an error and is different from previous
			if (i < target.Length && input [i] != target [i] && previousInput [i] != input [i]) {
				UILetter.UpdateInputDerivedAndDisplayColor (offColor);
				UILetter.ConfigureFlashParameters (
					offColor, onColor, 
					Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON,
					Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER
				);

			}
		
		}

	}
		


	class OpenClosedVowelColorer : RuleBasedColorer{

		static Color shortVowelColor = Color.yellow;
		static Color longVowelColor = Color.red;

	
		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			string unMatchedUserInputLetters = updatedUserInputLetters;

			//find closed vowels; update their colors; of what remains, find open vowels, update their colors, return what remains

			MatchCollection closedSyllables = Decoder.ClosedSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match closedSyllable in closedSyllables) {
				var closedSyllableLetters = UIletters.Skip (closedSyllable.Index).Take (closedSyllable.Length);

				InteractiveLetter vowel = closedSyllableLetters.ElementAt (Decoder.AnyVowel.Match (closedSyllable.Value).Index);
	
				vowel.UpdateInputDerivedAndDisplayColor (shortVowelColor);

				int endIndexOfSyllable = closedSyllable.Index + closedSyllable.Length - 1;
				string before = unMatchedUserInputLetters.Substring (0, closedSyllable.Index);
				string gap="".Fill (" ", closedSyllable.Length);
				string after = endIndexOfSyllable < unMatchedUserInputLetters.Length - 1 ? unMatchedUserInputLetters.Substring (endIndexOfSyllable+1, unMatchedUserInputLetters.Length - endIndexOfSyllable - 1) : "";
				unMatchedUserInputLetters = $"{before}{gap}{after}"; 
			}

			//check the remaining input for valid open syllables
			MatchCollection openSyllables = Decoder.OpenSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match openSyllable in openSyllables) {
				var openSyllableLetters = UIletters.Skip (openSyllable.Index).Take (openSyllable.Length);
				Match vowel = Decoder.AnyVowel.Match (openSyllable.Value);
				InteractiveLetter vowelLetter = openSyllableLetters.ElementAt(vowel.Index);
				vowelLetter.UpdateInputDerivedAndDisplayColor (longVowelColor);
			}

		}


		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			ColorAndConfigureFlashForTeacherMode (
				updatedUserInputLetters,
				previousUserInputLetters,
				UIletters);

		}

	}



	class MagicEColorer : RuleBasedColorer {
		static Color innerVowelColor = Color.red;
		static Color silentEColor = Color.gray;

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			Match magicE = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				openClosedVowelColorer.ColorAndConfigureFlashForTeacherMode (
					updatedUserInputLetters, 
					previousUserInputLetters, 
					UIletters
				);
				return;
			}
			InteractiveLetter innerVowel = null;
			InteractiveLetter silentE = null;
			ColorVowelAndSilentE (updatedUserInputLetters, magicE, UIletters, out innerVowel, out silentE);

			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);
			if (previousInstantiationOfRule.Value != magicE.Value) { //flash for each new instantiation of magic e rule.
				innerVowel.ConfigureFlashParameters (innerVowelColor, onColor, 
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
				);
				silentE.ConfigureFlashParameters (silentEColor, onColor, 
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
				);
			}

		}

	
		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			Match magicE = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				openClosedVowelColorer.ColorAndConfigureFlashForTeacherMode (
					updatedUserInputLetters, 
					previousUserInputLetters, 
					UIletters
				);
				return;
			}

			InteractiveLetter innerVowel = null;
			InteractiveLetter silentE = null;
			ColorVowelAndSilentE (updatedUserInputLetters, magicE, UIletters, out innerVowel, out silentE);

             
			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);
			Match targetMatch = Decoder.MagicERegex.Match (targetWord);
		
			if (targetMatch.Value == magicE.Value && 
				(!previousInstantiationOfRule.Success || targetMatch.Value != previousInstantiationOfRule.Value)) { //flash only if the child newly instantiates the target magic e rule.
				innerVowel.ConfigureFlashParameters (innerVowelColor, onColor, 
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
				);
				silentE.ConfigureFlashParameters (silentEColor, onColor, 
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
				);
			}
		}

		void ColorVowelAndSilentE(
			string updatedUserInputLetters, 
			Match magicE,
			List<InteractiveLetter> UIletters, 
			out InteractiveLetter innerVowel, 
			out InteractiveLetter silentE){

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);

			innerVowel = magicELetters.ElementAt (Decoder.AnyVowel.Match (magicE.Value).Index);

			innerVowel.UpdateInputDerivedAndDisplayColor (innerVowelColor);
			//By definition, last letter of magic-e instance is e.
			silentE = magicELetters.Last ();
			silentE.UpdateInputDerivedAndDisplayColor (silentEColor);

		}

	}


}

//note: the digraph colorer will need to update the derived and display color of members of the target digraph that do not 
//match the entire digraph. this needs to occur in the flash partial matches method
//likewise 
	
interface RuleBasedColorer{
	void ColorAndConfigureFlashForTeacherMode (
		string updatedUserInputLetters,
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters
	);

	void ColorAndConfigureFlashForStudentMode( 
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters,
		string targetWord);
}
	

