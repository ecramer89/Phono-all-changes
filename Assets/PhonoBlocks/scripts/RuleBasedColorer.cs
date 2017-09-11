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
	static RuleBasedColorer consonantDigraphsColorer = new ConsonantDigraphsColorer();
	static RuleBasedColorer consonantBlendsColorer = new ConsonantBlendsColorer();
	static RuleBasedColorer magicEColorer = new MagicEColorer();
	static RuleBasedColorer openClosedVowelColorer = new OpenClosedVowelColorer();

	static RuleBasedColorer ruleBasedColorer = consonantBlendsColorer; //todo, make a GO, start, subscribes to event; color rule selected;
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
	
		ReapplyDefaultOrOff (
			updatedUserInputLetters, 
			UILetters
		);

		//todo; switch according to whether current mode is teacher or student
		ruleBasedColorer.ColorAndConfigureFlashForStudentMode (
			updatedUserInputLetters, 
			previousUserInputLetters,
			UILetters,
			targetWord
		);

		TurnOffAndConfigureFlashForErroneousLetters (
			updatedUserInputLetters,
			previousUserInputLetters,
			UILetters,
			targetWord
		);


		StartAllInteractiveLetterFlashes ();

	
	}

	//todo: color schemes that return a reduced or otherwise modified list of UI letters can't do that here sicne the indices need to stay aligned.
	static void TurnOffAndConfigureFlashForErroneousLetters(string input, string previousInput, List<InteractiveLetter> UILetters, string target){
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

	static void ConfigureFlashOnCompletionOfTargetRule(InteractiveLetter letter, Color a, Color b){
		letter.ConfigureFlashParameters (a, b, 
			Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
			Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
		);
	}

	//return param is a string copy of input userInputLetters, with all letters that were colored replaced with blanks. 
	static string ColorAllInstancesOfMultiLetterUnit(
		string updatedUserInputLetters, 
		string previousUserInputLetters, 
		List<InteractiveLetter> UIletters,
		Regex spellingRuleRegex,
		Color multiLetterUnitColor){
		//find and match each successive blend.
		//because some strings could contain blends that cross boundaries (e.g., bll)
		//input to the regex match is a buffer from which we replace the 
		//blend letters with blanks on each iteration with blanks
		string unmatchedUserInputLetters=updatedUserInputLetters;
		Match multiLetterUnit = null;
		while(multiLetterUnit==null || multiLetterUnit.Success){
			multiLetterUnit = spellingRuleRegex.Match (unmatchedUserInputLetters);
			List<InteractiveLetter> unitLetters = UIletters.GetRange (multiLetterUnit.Index, multiLetterUnit.Length);
			//remove the blended letters from unmatchedUserInputLetters
			//replace with blanks (don't use substring) to account for:
			//"a dr" (example) and to keep the indices between the match data and previous input string aligned.
			unmatchedUserInputLetters = unmatchedUserInputLetters.ReplaceRangeWith(' ', multiLetterUnit.Index, multiLetterUnit.Length);
			Match previous = spellingRuleRegex.Match(previousUserInputLetters.Substring(multiLetterUnit.Index, multiLetterUnit.Length));
			//color each single consonant within the blend.
			foreach (InteractiveLetter letter in unitLetters) {
				letter.UpdateInputDerivedAndDisplayColor (multiLetterUnitColor);
				//if the user produced a new blend
				if(!previous.Success || previous.Value != multiLetterUnit.Value){
					//flash to indicate instantiation of spelling rule
					ConfigureFlashOnCompletionOfTargetRule (letter,multiLetterUnitColor, onColor);
				}
			}

		}


		return unmatchedUserInputLetters;

	}

	//return param is a string copy of input userInputLetters, with all letters that were colored replaced with blanks. 
	static void ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit(
		string updatedUserInputLetters,
		string previousUserInputLetters,  
		List<InteractiveLetter> UIletters, 
		string targetWord,
		Regex spellingRuleRegex,
		Color multiLetterUnitColor,
		Color singleLetterOfTargetUnitColor,
		Action<InteractiveLetter> hintPartialMatches
	){
		string unmatchedTargetInputLetters=targetWord;
		Match targetUnit = null;
		while(targetUnit==null || targetUnit.Success){

			targetUnit = spellingRuleRegex.Match (unmatchedTargetInputLetters);
			unmatchedTargetInputLetters = unmatchedTargetInputLetters.ReplaceRangeWith(' ', targetUnit.Index, targetUnit.Length);
			string correspondingUserInput = updatedUserInputLetters.Substring (targetUnit.Index, targetUnit.Length);
			Match userUnit = spellingRuleRegex.Match(correspondingUserInput);

			//update colors and flash if user is newly successful.
			if (userUnit.Success && userUnit.Value == targetUnit.Value) {
				List<InteractiveLetter> unitLetters = UIletters.GetRange (userUnit.Index, userUnit.Length);
				Match previous = spellingRuleRegex.Match (previousUserInputLetters.Substring (targetUnit.Index, targetUnit.Length));

				foreach (InteractiveLetter letter in unitLetters) {
					//for simplicity's sake, we always re-color the matching letters (otherwise they would be re-colored white by the entry point function)
					letter.UpdateInputDerivedAndDisplayColor (multiLetterUnitColor);
					if (!previous.Success || previous.Value != targetUnit.Value) {
						//but only flash if the child just completed the blend on this letter placement
						ConfigureFlashOnCompletionOfTargetRule (letter, multiLetterUnitColor, onColor);
					}
				}
			} else {
				//check whether the user successfully matched either any of the consonants in this blend.
				//if so, then color that letter as a single consonant.
				//(we don't bother coloring other consonants in student blends mode, even if correctly placed.
				//e.g. the "p" in "drop" would be white here, though would be single consonant color inteacher mode blends.
				for (int i = 0; i < targetUnit.Length; i++) {
					int indexOfLetterInInput = i + targetUnit.Index;
					if (updatedUserInputLetters [indexOfLetterInInput] == targetUnit.Value [i]) {
						InteractiveLetter asInteractiveLetter = UIletters.ElementAt (indexOfLetterInInput);
						asInteractiveLetter.UpdateInputDerivedAndDisplayColor (singleLetterOfTargetUnitColor);
						if (previousUserInputLetters [indexOfLetterInInput] != targetUnit.Value [i]) {
							hintPartialMatches (asInteractiveLetter);
						}
					}

				}

			}
		}

	}


	class ConsonantBlendsColorer : RuleBasedColorer{
		//valid blended consonants get colored green
		//after removing all valid blended consonants; single consonants get colored blue.
		Color blendedColor = Parameters.Colors.ConsonantBlendColors.COMPLETED_BLEND_COLOR;
		Color singleConsonantColor = Parameters.Colors.ConsonantBlendColors.SINGLE_CONSONANT_COLOR;
		//apart from coloring single consonants of target blend; we don't otherwise hint them (i.e., 
		//no flashing for consonant blends)
		Action<InteractiveLetter> hintNewPartialMatches = (InteractiveLetter letter)=>{return;};

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			//find and match each successive blend.
			//because some strings could contain blends that cross boundaries (e.g., bll)
			//input to the regex match is a buffer from which we replace the 
			//blend letters with blanks on each iteration with blanks
			string unmatchedUserInputLetters = ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				UIletters,
				SpellingRuleRegex.ConsonantBlend,
				blendedColor
            );


			//after identifying/coloring the blends; color all remaining singleton consonants 
		    //in the single consonant color.
			MatchCollection singleConsonants = SpellingRuleRegex.Consonant.Matches(unmatchedUserInputLetters);
			foreach (Match consonant in singleConsonants) {
				InteractiveLetter consonantLetter = UIletters.ElementAt (consonant.Index);
				consonantLetter.UpdateInputDerivedAndDisplayColor (singleConsonantColor);

			}
				
		}



		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){
		

			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
				UIletters, 
				targetWord,
				SpellingRuleRegex.ConsonantBlend,
				blendedColor,
				singleConsonantColor,
				hintNewPartialMatches
			);


		}

	}

	static Action<InteractiveLetter, Color, Color> flashCorrectLettersOfTargetMultiLetterUnit = 
		(InteractiveLetter letter, Color targetUnitColor, Color singleLetterColor) => {
		letter.ConfigureFlashParameters (targetUnitColor, singleLetterColor, 
			Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
			Parameters.Flash.Times.TIMES_TO_FLASH_CORRECT_PORTION_OF_FINAL_GRAPHEME
		);
   };


	class ConsonantDigraphsColorer : RuleBasedColorer{

		static Color digraphColor = Parameters.Colors.ConsonantDigraphColors.COMPLETED_DIGRAPH_COLOR;
		static Color singleMemberOfTargetDigraphColor = Parameters.Colors.ConsonantDigraphColors.SINGLE_MEMBER_OF_TARGET_DIGRAPH_COLOR;
		static Action<InteractiveLetter> hintPartialMatch = (InteractiveLetter letter) => flashCorrectLettersOfTargetMultiLetterUnit (letter, digraphColor, singleMemberOfTargetDigraphColor);

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				UIletters,
				SpellingRuleRegex.ConsonantDigraph,
				digraphColor
			);
		
		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
				UIletters, 
				targetWord,
				SpellingRuleRegex.ConsonantDigraph,
				digraphColor,
				singleMemberOfTargetDigraphColor,
				hintPartialMatch
			);
				
		}

	}

	class VowelDigraphsColorer : RuleBasedColorer{

		static Color digraphColor = Parameters.Colors.VowelDigraphColors.COMPLETED_DIGRAPH_COLOR;
		static Color singleMemberOfTargetDigraphColor = Parameters.Colors.VowelDigraphColors.SINGLE_MEMBER_OF_TARGET_DIGRAPH_COLOR;
		static Action<InteractiveLetter> hintPartialMatch = (InteractiveLetter letter) => flashCorrectLettersOfTargetMultiLetterUnit (letter, digraphColor, singleMemberOfTargetDigraphColor);

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				UIletters,
				SpellingRuleRegex.VowelDigraph,
				digraphColor
			);

		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
				UIletters, 
				targetWord,
				SpellingRuleRegex.VowelDigraph,
				digraphColor,
				singleMemberOfTargetDigraphColor,
				hintPartialMatch
			);

		}

	}

	class RControlledVowelsColorer : RuleBasedColorer{

		static Color rControlledVowelColor = Parameters.Colors.RControlledVowelColors.R_CONTROLLED_VOWEL_COLOR;
		static Color singleMemberOfRControlledVowelColor = Parameters.Colors.RControlledVowelColors.SINGLE_MEMBER_OF_TARGET_R_CONTROLLED_VOWEL_COLOR;
		static Action<InteractiveLetter> hintPartialMatch = (InteractiveLetter letter) => flashCorrectLettersOfTargetMultiLetterUnit (letter, rControlledVowelColor, singleMemberOfRControlledVowelColor);

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				UIletters,
				SpellingRuleRegex.RControlledVowel,
				rControlledVowelColor
			);

		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
				UIletters, 
				targetWord,
				SpellingRuleRegex.RControlledVowel,
				rControlledVowelColor,
				singleMemberOfRControlledVowelColor,
				hintPartialMatch
			);

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
				unMatchedUserInputLetters = unMatchedUserInputLetters.ReplaceRangeWith(' ', closedSyllable.Index, closedSyllable.Length);

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

			ColorAndConfigureFlash (updatedUserInputLetters, previousUserInputLetters, UIletters,
				(Match magicE) => {
					Match previousInstantiationOfRule = SpellingRuleRegex.MagicERegex.Match (previousUserInputLetters);
					return previousInstantiationOfRule.Value != magicE.Value;
				}
			);
		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			List<InteractiveLetter> UIletters, 
			string targetWord){


			ColorAndConfigureFlash (updatedUserInputLetters, previousUserInputLetters, UIletters, 
				(Match magicE) => {
					Match previousInstantiationOfRule = SpellingRuleRegex.MagicERegex.Match (previousUserInputLetters);
					Match targetMatch = SpellingRuleRegex.MagicERegex.Match (targetWord);

					return targetMatch.Value == magicE.Value && 
						(!previousInstantiationOfRule.Success || targetMatch.Value != previousInstantiationOfRule.Value);
				}
			);

		}


		void ColorAndConfigureFlash(
			string updatedUserInputLetters, 
			string previousUserInputLetters, 
			List<InteractiveLetter> UIletters, 
			Func<Match, bool> shouldFlash
		){

			Match magicE = SpellingRuleRegex.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				OpenClosedVowelColorer openClosed = (OpenClosedVowelColorer)openClosedVowelColorer;
				openClosed.ColorVowels (
					updatedUserInputLetters, 
					UIletters
				);
				return;
			}

			var magicELetters = UIletters.Skip(magicE.Index).Take(magicE.Length);

			InteractiveLetter innerVowel = magicELetters.ElementAt (SpellingRuleRegex.AnyVowel.Match (magicE.Value).Index);

			innerVowel.UpdateInputDerivedAndDisplayColor (innerVowelColor);
			//By definition, last letter of magic-e instance is e.
			InteractiveLetter silentE = magicELetters.Last ();
			silentE.UpdateInputDerivedAndDisplayColor (silentEColor);

			if (shouldFlash(magicE)) { 
				ConfigureFlashOnCompletionOfTargetRule (innerVowel,innerVowelColor, onColor);
				ConfigureFlashOnCompletionOfTargetRule (silentE, silentEColor, onColor);
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
	

