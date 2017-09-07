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


	static Action<string, List<InteractiveLetter>> applyDefaultColorsToNonMatchingLetters = (string updatedUserInputLetters, List<InteractiveLetter> UILetters) => {
		int index = 0;
		UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters [index] == ' ' ? offColor : onColor);
			index++;
		});
	};


	public static void ReColor (string updatedUserInputLetters,string previousUserInputLetters, List<InteractiveLetter> UILetters){
		applyDefaultColorsToNonMatchingLetters (
			updatedUserInputLetters, UILetters);

		ruleBasedColorer.ColorAndConfigureFlashForMatches (
			updatedUserInputLetters, 
			previousUserInputLetters,
			UILetters
		);
	
	}

	//todo: color schemes that return a reduced or otherwise modified list of UI letters can't do that here sicne the indices need to stay aligned.
	public static void TurnOffFlashErroneousLetters(string input, string previousInput, List<InteractiveLetter> UILetters, string target){
		for (int i = 0; i < UILetters.Count; i++) {
			InteractiveLetter UILetter = UILetters [i];

			//index can exceed length of target word since target word doesn't take trailing blanks into account. (e.g., "_ame___", vs "game")
			if (i < target.Length && input [i] != target [i] && previousInput [i] != input [i]) {
				UILetter.UpdateInputDerivedAndDisplayColor (offColor);
				//note: we can't necessarily assume this for all of the letters. if the letter is correctly placed,
				//then even if it's the same letter as before, may still need to flash it if it now instantiates a spelling rule
				//on account of a different, newly placed letter. e.g. the "gam"->"game"; "a" hasn't changed from prev. but needs to flash.
				//that's why the continue is within the check on whether the letter is erroneous- otherwise we'd skip initiating the flash coroutine.
				UILetter.SetFlashColors (offColor, onColor);
				UILetter.SetFlashDurations (Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON);
				UILetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
			}


			//start flash co-routines of all letters (including those that were configured by the rule-specific colorer).
			UILetter.StartFlash ();
		}


	}


	public static void FlashFeedback(string updatedUserInputLetters, string previousUserInputLetters, List<InteractiveLetter> UIletters, string targetWord){
		ruleBasedColorer.ColorAndConfigureFlashForPartialMatch (
				updatedUserInputLetters,
				previousUserInputLetters, 
				UIletters,
				targetWord);

		TurnOffFlashErroneousLetters (
			updatedUserInputLetters,
			previousUserInputLetters,
			UIletters,
			targetWord);
	}



	class OpenClosedVowelColorer : RuleBasedColorer{

		static Color shortVowelColor = Color.yellow;
		static Color longVowelColor = Color.red;

	
		public void ColorAndConfigureFlashForMatches(
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

		//no concept of a "partially matched" magic e rule. Just return.
		public void ColorAndConfigureFlashForPartialMatch(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			return;

		}

	}


		
	class MagicEColorer : RuleBasedColorer {
		static Color innerVowelColor = Color.red;
		static Color silentEColor = Color.gray;

		public void ColorAndConfigureFlashForMatches(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			Match magicE = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				openClosedVowelColorer.ColorAndConfigureFlashForMatches (
					updatedUserInputLetters, 
					previousUserInputLetters, 
					UIletters
				);
				return;
			}
			
			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);

			if (previousInstantiationOfRule.Success && 
				previousInstantiationOfRule.Value == magicE.Value)
				return; //exact same match existed before, so don't bother changing anything.

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);

			Match innerVowel = Decoder.AnyVowel.Match(magicE.Value);
			magicELetters.
			ElementAt (innerVowel.Index).
			UpdateInputDerivedAndDisplayColor (innerVowelColor).
			ConfigureFlashParameters (innerVowelColor, onColor, 
				Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
				Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
			);

			//don't need to color consonants; they can retain their default on color.
		
			//By definition, last letter of magic-e instance is e.
			magicELetters.
			Last ().
			UpdateInputDerivedAndDisplayColor (silentEColor).
			ConfigureFlashParameters (silentEColor, onColor, 
				Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
				Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
			);

		}

		//no concept of a "partially matched" magic e rule. Just return.
		public void ColorAndConfigureFlashForPartialMatch(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			return;

		}


	}


}

//note: the digraph colorer will need to update the derived and display color of members of the target digraph that do not 
//match the entire digraph. this needs to occur in the flash partial matches method
//likewise 
	
interface RuleBasedColorer{
	void ColorAndConfigureFlashForMatches (
		string updatedUserInputLetters,
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters
	);

	void ColorAndConfigureFlashForPartialMatch( 
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters,
		string targetWord);
}
	


