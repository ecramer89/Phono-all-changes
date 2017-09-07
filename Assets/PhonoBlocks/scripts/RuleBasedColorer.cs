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


	static Action<string, List<InteractiveLetter>> applyDefaultColorsToNonMatchingLetters = (string updatedUserInputLetters, List<InteractiveLetter> UILetters) => {
		int index = 0;
		UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters [index] == ' ' ? offColor : onColor);
			index++;
		});
	};


	public static void ReColor (string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UILetters){
		applyDefaultColorsToNonMatchingLetters(
				updatedUserInputLetters,
			ruleBasedColorer.ColorMatchesAndFlashIfNew (
				previousUserInputLetters, 
				updatedUserInputLetters, 
				UILetters)
		);
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


	public static void ConfigureFlashFeedbackForTargetRule(string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UIletters, string targetWord){
		ruleBasedColorer.FlashPartialMatchesToTarget (
			previousUserInputLetters, 
			updatedUserInputLetters, 
			UIletters,
			targetWord);
	}


		
	class MagicEColorer : RuleBasedColorer {
		Color innerVowelColor = Color.red;
		Color consonantsColor = Color.white;
		Color silentEColor = Color.gray;

		public List<InteractiveLetter> ColorMatchesAndFlashIfNew(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			Match magicE = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success)
				return UIletters; //no match found; nothing to do. (todo: in fact, just delegate to the open closed syllable function).
			
			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);
			if (previousInstantiationOfRule.Success && 
				previousInstantiationOfRule.Index == magicE.Index && 
				previousInstantiationOfRule.Length == magicE.Length)
				return UIletters; //matched before, so nothing to change.

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);
			var rest = UIletters.Skip (magicE.Length);

			Match innerVowel = Decoder.AnyVowel.Match(magicE.Value);
			magicELetters.
			ElementAt (innerVowel.Index).
			UpdateInputDerivedAndDisplayColor (innerVowelColor).
			ConfigureFlashParameters (innerVowelColor, onColor, 
				Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
				Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
			).
			StartFlash ();
				
			MatchCollection consonants = Decoder.AnyConsonant.Matches(magicE.Value);
			foreach(Match m in consonants){
				magicELetters.
				ElementAt(m.Index).
				UpdateInputDerivedAndDisplayColor(consonantsColor).
				ConfigureFlashParameters (consonantsColor, onColor, 
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
				).
				StartFlash ();
			}
		
			magicELetters.
			Last().
			UpdateInputDerivedAndDisplayColor(silentEColor).
			ConfigureFlashParameters (silentEColor, onColor, 
				Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
				Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
			).
			StartFlash ();


			return rest.ToList ();

		}

		//no concept of a "partially matched" magic e rule. Just return.
		public List<InteractiveLetter> FlashPartialMatchesToTarget(
			string previousUserInputLetters, 
			string updatedUserInputLetters, 
			List<InteractiveLetter> UIletters, 
			string targetWord){

			return UIletters;

		}


	}


}
	
interface RuleBasedColorer{
	List<InteractiveLetter> ColorMatchesAndFlashIfNew (
		string previousUserInputLetters,
		string updatedUserInputLetters, 
		List<InteractiveLetter> UIletters
	);

	List<InteractiveLetter> FlashPartialMatchesToTarget(
		string previousUserInputLetters, 
		string updatedUserInputLetters, 
		List<InteractiveLetter> UIletters,
		string targetWord);
}
	


