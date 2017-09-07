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

		ruleBasedColorer.ColorMatchesAndFlashIfNew (
			updatedUserInputLetters, 
			previousUserInputLetters,
			UILetters
		);
	
	}

	//todo: color schemes that return a reduced or otherwise modified list of UI letters can't do that here sicne the indices need to stay aligned.
	public static void TurnOffFlashErroneousLetters(string input, string previousInput, List<InteractiveLetter> UILetters, string target){
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


	public static void FlashFeedback(string updatedUserInputLetters, string previousUserInputLetters, List<InteractiveLetter> UIletters, string targetWord){
		TurnOffFlashErroneousLetters (
			updatedUserInputLetters,
			previousUserInputLetters, 
			ruleBasedColorer.ColorAndFlashPartialMatchesIfNew (
				updatedUserInputLetters,
				previousUserInputLetters, 
				UIletters,
				targetWord),
			targetWord);
	}



	class OpenClosedVowelColorer : RuleBasedColorer{

		static Color shortVowelColor = Color.yellow;
		static Color longVowelColor = Color.red;
		static Color consonantColor = Color.white;

		public void ColorMatchesAndFlashIfNew(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			string unMatchedUserInputLetters = updatedUserInputLetters;
			//find closed vowels; update their colors; of what remains, find open vowels, update their colors, return what remains
			MatchCollection closedSyllables = Decoder.ClosedSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match closedSyllable in closedSyllables) {
				//if this portion of the user input string was a closed syllable before, no need to update the colors/flash
				if (Decoder.ClosedSyllable.IsMatch (previousUserInputLetters.Substring (closedSyllable.Index, closedSyllable.Length)))
					continue;

				var closedSyllableLetters = UIletters.Skip (closedSyllable.Index).Take (closedSyllable.Length);

				InteractiveLetter vowel = closedSyllableLetters.ElementAt (Decoder.AnyVowel.Match (closedSyllable.Value).Index);
				vowel.UpdateInputDerivedAndDisplayColor (shortVowelColor);
				vowel.ConfigureFlashParameters (
					shortVowelColor, onColor,
					Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
					Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME);

				MatchCollection consonants = Decoder.AnyConsonant.Matches (updatedUserInputLetters);
				foreach (Match consonant in consonants) {
					InteractiveLetter consLetter = closedSyllableLetters.ElementAt (consonant.Index);
					consLetter.UpdateInputDerivedAndDisplayColor (consonantColor);
					//don't bother flashing the consonants.
				}

				//this wont work; need the entries to be contiguous (i.e, inject blanks)
				//treat already matched etters as though they are blanks
				int endIndexOfSyllable = closedSyllable.Index + closedSyllable.Length - 1;
				string before = unMatchedUserInputLetters.Substring (0, closedSyllable.Index);
				string gap="".Fill (" ", closedSyllable.Length);
				string after = endIndexOfSyllable < unMatchedUserInputLetters.Length - 1 ? unMatchedUserInputLetters.Substring (endIndexOfSyllable+1, unMatchedUserInputLetters.Length - endIndexOfSyllable - 1) : "";
				unMatchedUserInputLetters = $"{before}{gap}{after}"; 
			}

			//check the remaining input for valid open syllables
			MatchCollection openSyllables = Decoder.OpenSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match openSyllable in openSyllables) {
				//if this portion of the user input string was a closed syllable before, no need to update the colors/flash
				if (Decoder.OpenSyllable.IsMatch (previousUserInputLetters.Substring (openSyllable.Index, openSyllable.Length)))
					continue;
				
				//note that because the indices/lengths of unMatchedUserInputLetters and UILetters are aligned,
				//we take the UILetters that match the open syllable from the original (full) list of UILetters
				var openSyllableLetters = UIletters.Skip (openSyllable.Index).Take (openSyllable.Length);
				Match consonant = Decoder.AnyConsonant.Match (openSyllable.Value);
				if (consonant.Success) { 
					//not all open syllables contain consonants
					InteractiveLetter consonantLetter = openSyllableLetters.ElementAt(consonant.Index);
					consonantLetter.UpdateInputDerivedAndDisplayColor (consonantColor);
				}
				Match vowel = Decoder.AnyVowel.Match (openSyllable.Value);
				InteractiveLetter vowelLetter = openSyllableLetters.ElementAt(vowel.Index);
				vowelLetter.UpdateInputDerivedAndDisplayColor (longVowelColor);
			}

		}

		//no concept of a "partially matched" magic e rule. Just return.
		public List<InteractiveLetter> ColorAndFlashPartialMatchesIfNew(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			return UIletters;

		}

	}


		
	class MagicEColorer : RuleBasedColorer {
		static Color innerVowelColor = Color.red;
		static Color consonantsColor = Color.white;
		static Color silentEColor = Color.gray;

		public void ColorMatchesAndFlashIfNew(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			Match magicE = Decoder.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				openClosedVowelColorer.ColorMatchesAndFlashIfNew (
					updatedUserInputLetters, 
					previousUserInputLetters, 
					UIletters
				);
				return;
			}
			
			Match previousInstantiationOfRule = Decoder.MagicERegex.Match (previousUserInputLetters);
			if (previousInstantiationOfRule.Success && 
				previousInstantiationOfRule.Index == magicE.Index && 
				previousInstantiationOfRule.Length == magicE.Length)
				return; //matched before, so nothing to change.

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

		}

		//no concept of a "partially matched" magic e rule. Just return.
		public List<InteractiveLetter> ColorAndFlashPartialMatchesIfNew(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
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
	void ColorMatchesAndFlashIfNew (
		string updatedUserInputLetters,
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters
	);

	List<InteractiveLetter> ColorAndFlashPartialMatchesIfNew( 
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		List<InteractiveLetter> UIletters,
		string targetWord);
}
	


