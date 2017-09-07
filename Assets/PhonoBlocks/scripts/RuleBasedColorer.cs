using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;

	
public class Colorer  {


	static Color onColor = Color.white;
	static Color offColor = Color.gray;
	static RuleBasedColorer ruleBasedColorer = new MagicEColorer(); //todo, make a GO, start, subscribes to event; color rule selected;
	//nees to persist bw levels. depending on whichrule selected instantiates appropriate rule based colorer


	/*
	 * public top level method. re-colors every letter based on the current user input.
	 * 
	 * */
	public static void ReColor (string updatedUserInputLetters, List<InteractiveLetter> UILetters){
		//1. establish 'blank canvas':
		// - any letter a user has placed is default on color 
		// - any letter user has not placed is default off color
		int index = 0;
		UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters[index] == ' ' ? offColor : onColor);
			index++;
		});

		//2. color letters that instantiate current spelling rules in special rule based colors.
		ruleBasedColorer.ApplyToMatches (updatedUserInputLetters, UILetters);
	}


	public static void TurnOffFlashErroneousLetters(string previousInput, string input,  List<InteractiveLetter> UILetters, string target){
		int index = 0;
		foreach(InteractiveLetter UILetter in UILetters){
			if(previousInput[index] == input[index]) continue; //don't bother flashing/changing anything if this letter didn't change
			if(input[index] == target[index]) continue; //correct letter; nothing to do
			//otherwise it's a new error.
			//turn the letter off
			UILetter.UpdateInputDerivedAndDisplayColor(offColor);
			//configure it to flash in the incorrect colors.
			UILetter.SetFlashColors (offColor, onColor);
			UILetter.SetFlashDurations (Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON);
			UILetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
			UILetter.StartFlash ();
			index++;

		}
	}


	public static void ConfigureFlashFeedbackForTargetRule(string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UIletters){
		ruleBasedColorer.FlashPartialMatches (previousUserInputLetters, updatedUserInputLetters, UIletters);
	}


		
	class MagicEColorer : RuleBasedColorer {
		Color innerVowelColor = Color.red;
		Color consonantsColor = Color.white;
		Color silentEColor = Color.gray;

		public void ApplyToMatches(string updatedUserInputLetters, List<InteractiveLetter> UIletters){
			Match magicE = Decoder.MagicERegex.Match(updatedUserInputLetters);

			if (!magicE.Success)
				return;

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);

			Match innerVowel = Decoder.AnyVowel.Match(magicE.Value);
			magicELetters.ElementAt(innerVowel.Index).UpdateInputDerivedAndDisplayColor(innerVowelColor);
			MatchCollection consonants = Decoder.AnyConsonant.Matches(magicE.Value);
			foreach(Match m in consonants){
				magicELetters.ElementAt(m.Index).UpdateInputDerivedAndDisplayColor(consonantsColor);
			}
		
			magicELetters.Last().UpdateInputDerivedAndDisplayColor(silentEColor);
		}

		//
		public void FlashPartialMatches(string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UIletters){
			Match currentInputMatch = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!currentInputMatch.Success)
				return;

			//don't bother flashing if the previous user input letters also contained a match at the same indices for this rule.
			//todo can bring this up into the more generic class
			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);
			if (previousInstantiationOfRule.Success && 
				previousInstantiationOfRule.Index == currentInputMatch.Index && 
				previousInstantiationOfRule.Length == currentInputMatch.Length)
				return;

			for (int i = currentInputMatch.Index; i < currentInputMatch.Index + currentInputMatch.Length; i++) {
				InteractiveLetter UILetter = UIletters [i];
				UILetter.SetFlashColors (UILetter.ColorDerivedFromInput, offColor);
				UILetter.SetFlashDurations (Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF);
				UILetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME);
				UILetter.StartFlash ();
			}

		}


	}


}
	
interface RuleBasedColorer{
	void ApplyToMatches (string updatedUserInputLetters, List<InteractiveLetter> UIletters);
	void FlashPartialMatches(string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UIletters);
}
	


