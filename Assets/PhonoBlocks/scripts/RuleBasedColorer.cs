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
				updatedUserInputLetters, 
				previousUserInputLetters,
				UILetters)
		);
	}


	public static void TurnOffFlashErroneousLetters(string previousInput, string input,  List<InteractiveLetter> UILetters, string target){
		for (int i = 0; i < UILetters.Count; i++) {
			InteractiveLetter UILetter = UILetters [i];
			if (i >= target.Length || input [i] == target [i])
					continue; //correct letter; nothing to do
				//otherwise it's a new error.
				//turn the letter off
				UILetter.UpdateInputDerivedAndDisplayColor (offColor);
				//configure it to flash in the incorrect colors.
			if (previousInput [i] == input [i]) continue; //skip the flashing if this letter hasn't changed from before.
				UILetter.SetFlashColors (offColor, onColor);
				UILetter.SetFlashDurations (Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON);
				UILetter.SetNumFlashCycles (Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER);
				UILetter.StartFlash ();
			}
	}




	public static void FlashFeedback(string previousUserInputLetters, string updatedUserInputLetters, List<InteractiveLetter> UIletters, string targetWord){
		TurnOffFlashErroneousLetters (
			previousUserInputLetters,
			updatedUserInputLetters, 
			ruleBasedColorer.ColorAndFlashPartialMatchesIfNew (
				previousUserInputLetters, 
				updatedUserInputLetters, 
				UIletters,
				targetWord),
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

			return UIletters.Skip (magicE.Length).Take(UIletters.Count - magicE.Length).ToList ();

		}

		//no concept of a "partially matched" magic e rule. Just return.
		public List<InteractiveLetter> ColorAndFlashPartialMatchesIfNew(
			string previousUserInputLetters, 
			string updatedUserInputLetters, 
			List<InteractiveLetter> UIletters, 
			string targetWord){

			return UIletters;

		}


	}


}

//note: the digraph colorer will need to update the derived and display color of members of the target digraph that do not 
//match the entire digraph. this needs to occur in the flash partial matches method
//likewise 
	
interface RuleBasedColorer{
	List<InteractiveLetter> ColorMatchesAndFlashIfNew (
		string previousUserInputLetters,
		string updatedUserInputLetters, 
		List<InteractiveLetter> UIletters
	);

	List<InteractiveLetter> ColorAndFlashPartialMatchesIfNew(
		string previousUserInputLetters, 
		string updatedUserInputLetters, 
		List<InteractiveLetter> UIletters,
		string targetWord);
}
	


