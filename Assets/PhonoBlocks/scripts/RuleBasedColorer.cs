using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Extensions;
	
public class Colorer  {

	static Color onColor = Parameters.Colors.DEFAULT_ON_COLOR;
	static Color offColor = Parameters.Colors.DEFAULT_OFF_COLOR;
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

		//todo; switch according to whether current mode is teacher or student
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

		static Color shortVowelColor = Parameters.Colors.OpenClosedVowelColors.SHORT_VOWEL;
		static Color longVowelColor =  Parameters.Colors.OpenClosedVowelColors.LONG_VOWEL;
		static Color consonantColorFirstAlternation = Parameters.Colors.OpenClosedVowelColors.FIRST_CONSONANT_COLOR;
		static Color consonantColorSecondAlternation = Parameters.Colors.OpenClosedVowelColors.SECOND_CONSONANT_COLOR;
	
		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			//color consonants in alternating blue-green.
			ColorConsonants(UIletters, updatedUserInputLetters);
			//color vowels by syllable type.
			ColorVowels(updatedUserInputLetters,UIletters);

		}

		void ColorConsonants(List<InteractiveLetter> UIletters, string updatedUserInputLetters){
			//color consonants in alternating blue-green
			MatchCollection consonants = SpellingRuleRegex.AnyConsonant.Matches (updatedUserInputLetters);
			for (int i = 0; i < consonants.Count; i++) {
				Color consonantColor = i % 2 == 0 ? consonantColorFirstAlternation : consonantColorSecondAlternation;
				Match consonant = consonants [i];
				//"any consonant" will match consonant digraphs and blends as well as single letters.
				//as such, some matches might contain more than one letter, which need to be colored.
				List<InteractiveLetter> consonantLetters = UIletters.GetRange (consonant.Index, consonant.Length);
				foreach (InteractiveLetter consonantLetter in consonantLetters) {
					consonantLetter.UpdateInputDerivedAndDisplayColor (consonantColor);
				}
			}
		}

		public void ColorVowels(string updatedUserInputLetters, List<InteractiveLetter> UIletters){
			//color vowels according to syllable type.
			string unMatchedUserInputLetters = updatedUserInputLetters;

			//first check for closed syllables. color any vowel in a closed syllable 'short'
			MatchCollection closedSyllables = SpellingRuleRegex.ClosedSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match closedSyllable in closedSyllables) {
				var closedSyllableLetters = UIletters.Skip (closedSyllable.Index).Take (closedSyllable.Length);
				InteractiveLetter vowel = closedSyllableLetters.ElementAt (SpellingRuleRegex.AnyVowel.Match (closedSyllable.Value).Index);
				//update vowel color
				vowel.UpdateInputDerivedAndDisplayColor (shortVowelColor);
				//update the string that will be used to check for any remaining open syllables.
				//e.g., as in string BABAB (should be parsed as BAB and AB).
				int endIndexOfSyllable = closedSyllable.Index + closedSyllable.Length - 1;
				string before = unMatchedUserInputLetters.Substring (0, closedSyllable.Index);
				string gap="".Fill (" ", closedSyllable.Length);
				string after = endIndexOfSyllable < unMatchedUserInputLetters.Length - 1 ? unMatchedUserInputLetters.Substring (endIndexOfSyllable+1, unMatchedUserInputLetters.Length - endIndexOfSyllable - 1) : "";
				unMatchedUserInputLetters = $"{before}{gap}{after}"; 
			}

			//check remaining portions of input for open syllables.
			//color any vowel in open syllable 'long'
			MatchCollection openSyllables = SpellingRuleRegex.OpenSyllable.Matches(unMatchedUserInputLetters);
			foreach (Match openSyllable in openSyllables) {
				var openSyllableLetters = UIletters.Skip (openSyllable.Index).Take (openSyllable.Length);
				Match vowel = SpellingRuleRegex.AnyVowel.Match (openSyllable.Value);
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
		static Color innerVowelColor = Parameters.Colors.MagicEColors.INNER_VOWEL;
		static Color silentEColor = Parameters.Colors.MagicEColors.SILENT_E;

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			Match magicE = RevertToOpenClosedColorIfNoMatchFound (updatedUserInputLetters, UIletters);
			if (!magicE.Success)
				return;

			Match previousInstantiationOfRule = SpellingRuleRegex.MagicERegex.Match (previousUserInputLetters);
		
			ColorAndConfigureFlashForVowelAndSilentE (updatedUserInputLetters, magicE, UIletters,
				() => {
				return previousInstantiationOfRule.Value != magicE.Value;
			}
			);


		}


	

	
	
		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){

			Match magicE = RevertToOpenClosedColorIfNoMatchFound (updatedUserInputLetters, UIletters);
			if (!magicE.Success)
				return;

			Match previousInstantiationOfRule = SpellingRuleRegex.MagicERegex.Match (previousUserInputLetters);
			Match targetMatch = SpellingRuleRegex.MagicERegex.Match (targetWord);
		
			ColorAndConfigureFlashForVowelAndSilentE (updatedUserInputLetters, magicE, UIletters, 
				() => {
				return targetMatch.Value == magicE.Value && 
					(!previousInstantiationOfRule.Success || targetMatch.Value != previousInstantiationOfRule.Value);
				}
			);

        
		}

		Match RevertToOpenClosedColorIfNoMatchFound(string updatedUserInputLetters, List<InteractiveLetter> UILetters){
			Match magicE = SpellingRuleRegex.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				OpenClosedVowelColorer openClosed = (OpenClosedVowelColorer)openClosedVowelColorer;
				openClosed.ColorVowels (
					updatedUserInputLetters, 
					UILetters
				);
			}

			return magicE;

		}
			

		void ColorAndConfigureFlashForVowelAndSilentE(
			string updatedUserInputLetters, 
			Match magicE,
			List<InteractiveLetter> UIletters, 
			Func<bool> shouldFlash){

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);

			InteractiveLetter innerVowel = magicELetters.ElementAt (SpellingRuleRegex.AnyVowel.Match (magicE.Value).Index);

			innerVowel.UpdateInputDerivedAndDisplayColor (innerVowelColor);
			//By definition, last letter of magic-e instance is e.
			InteractiveLetter silentE = magicELetters.Last ();
			silentE.UpdateInputDerivedAndDisplayColor (silentEColor);

			if (shouldFlash()) { 
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
	

