using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Extensions;
	
public class Colorer : MonoBehaviour   {

	private static Colorer instance;
	public static Colorer Instance{
		get {
			return instance;
		}
	}


	static Color onColor = Parameters.Colors.DEFAULT_ON_COLOR;
	static Color offColor = Parameters.Colors.DEFAULT_OFF_COLOR;
	static RuleBasedColorer consonantDigraphsColorer = new ConsonantDigraphsColorer();
	static RuleBasedColorer consonantBlendsColorer = new ConsonantBlendsColorer();
	static RuleBasedColorer magicEColorer = new MagicEColorer();
	static RuleBasedColorer openClosedVowelColorer = new OpenClosedVowelColorer();
	static RuleBasedColorer rControlledVowelsColorer = new RControlledVowelsColorer();
	static RuleBasedColorer vowelDigraphsColorer = new VowelDigraphsColorer();


	static RuleBasedColorer ruleBasedColorer; 


	public void Start(){
		instance = this;

		Events.Dispatcher.OnActivitySelected += (Activity activity) => {

			switch(activity){
			case Activity.CONSONANT_BLENDS:
				ruleBasedColorer = consonantBlendsColorer;
				break;
			case Activity.CONSONANT_DIGRAPHS:
				ruleBasedColorer = consonantDigraphsColorer;
				break;
			case Activity.MAGIC_E:
				ruleBasedColorer = magicEColorer;
				break;
			case Activity.R_CONTROLLED_VOWELS:
				ruleBasedColorer = rControlledVowelsColorer;
				break;
			case Activity.VOWEL_DIGRAPHS:
				ruleBasedColorer = vowelDigraphsColorer;
				break;
			case Activity.OPEN_CLOSED_SYLLABLE:
				ruleBasedColorer = openClosedVowelColorer;
				break;

			}

		};


		Events.Dispatcher.OnUILettersCreated += (List<InteractiveLetter> letters) => {
			RegisterLettersToColorer(letters);
		};
	}


	static Action<string> ReapplyDefaultOrOff = (string updatedUserInputLetters) => {
		int index = 0;
		State.Current.UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters [index] == ' ' ? offColor : onColor);
			index++;
		});
	};
		

	static event Action ResetAllInteractiveLetterFlashConfigurations = () => {};
	static event Action StartAllInteractiveLetterFlashes = ()=>{};

	public static void RegisterLettersToColorer(List<InteractiveLetter> letters){
		State.Current.UILetters.ForEach (UILetter => {
			StartAllInteractiveLetterFlashes += UILetter.StartFlash;
			ResetAllInteractiveLetterFlashConfigurations += UILetter.ResetFlashParameters;
		});
	}


	public void ReColor (string updatedUserInputLetters,string previousUserInputLetters, string targetWord){

		ResetAllInteractiveLetterFlashConfigurations ();
	
		ReapplyDefaultOrOff (
			updatedUserInputLetters
		);

		if (State.Current.Mode == Mode.TEACHER) {
			ruleBasedColorer.ColorAndConfigureFlashForTeacherMode (
				updatedUserInputLetters, 
				previousUserInputLetters
			);
		} else {
			ruleBasedColorer.ColorAndConfigureFlashForStudentMode (
				updatedUserInputLetters, 
				previousUserInputLetters,
				targetWord
			);

			TurnOffAndConfigureFlashForErroneousLetters (
				updatedUserInputLetters,
				previousUserInputLetters,
				targetWord
			);
		}
			
		StartAllInteractiveLetterFlashes ();

	
	}

	//todo: color schemes that return a reduced or otherwise modified list of UI letters can't do that here sicne the indices need to stay aligned.
	static void TurnOffAndConfigureFlashForErroneousLetters(string input, string previousInput,string target){
		List<InteractiveLetter> UILetters = State.Current.UILetters;
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
		Regex spellingRuleRegex,
		Color multiLetterUnitColor){
		List<InteractiveLetter> UIletters = State.Current.UILetters;
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
		string targetWord,
		Regex spellingRuleRegex,
		Color multiLetterUnitColor,
		Color singleLetterOfTargetUnitColor,
		Action<InteractiveLetter> hintPartialMatches
	){
		List<InteractiveLetter> UIletters = State.Current.UILetters;
		string unmatchedTargetInputLetters=targetWord;
		Match targetUnit = null;
		while(targetUnit==null || targetUnit.Success){

			targetUnit = spellingRuleRegex.Match (unmatchedTargetInputLetters);
			unmatchedTargetInputLetters = unmatchedTargetInputLetters.ReplaceRangeWith(' ', targetUnit.Index, targetUnit.Length);
			//string correspondingUserInput = updatedUserInputLetters.Substring (targetUnit.Index, targetUnit.Length);
			Match userUnit = spellingRuleRegex.Match(updatedUserInputLetters);

			//if a corresponding match was found in the updated user input
			if (userUnit.Success && 
				userUnit.Index == targetUnit.Index && 
				userUnit.Length == targetUnit.Length && 
				userUnit.Value == targetUnit.Value) {
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
		List<InteractiveLetter> UIletters = State.Current.UILetters;
		//valid blended consonants get colored green
		//after removing all valid blended consonants; single consonants get colored blue.
		Color blendedColor = Parameters.Colors.ConsonantBlendColors.COMPLETED_BLEND_COLOR;
		Color singleConsonantColor = Parameters.Colors.ConsonantBlendColors.SINGLE_CONSONANT_COLOR;
		//apart from coloring single consonants of target blend; we don't otherwise hint them (i.e., 
		//no flashing for consonant blends)
		Action<InteractiveLetter> hintNewPartialMatches = (InteractiveLetter letter)=>{return;};

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters){

			//find and match each successive blend.
			//because some strings could contain blends that cross boundaries (e.g., bll)
			//input to the regex match is a buffer from which we replace the 
			//blend letters with blanks on each iteration with blanks
			string unmatchedUserInputLetters = ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
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
			string targetWord){
		

			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
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
			string previousUserInputLetters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				SpellingRuleRegex.ConsonantDigraph,
				digraphColor
			);
		
		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
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
			string previousUserInputLetters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				SpellingRuleRegex.VowelDigraph,
				digraphColor
			);

		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
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
			string previousUserInputLetters){

			ColorAllInstancesOfMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,
				SpellingRuleRegex.RControlledVowel,
				rControlledVowelColor
			);

		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters,  
			string targetWord){


			ColorCompleteAndHintPartialInstancesOfAllTargetMultiLetterUnit (
				updatedUserInputLetters,
				previousUserInputLetters,  
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
			string previousUserInputLetters){

			//color consonants in alternating blue-green.
			ColorConsonants(updatedUserInputLetters);
			//color vowels by syllable type.
			ColorVowels(updatedUserInputLetters);

		}

		void ColorConsonants(string updatedUserInputLetters){
			List<InteractiveLetter> UIletters = State.Current.UILetters;
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

		public void ColorVowels(string updatedUserInputLetters){
			List<InteractiveLetter> UIletters = State.Current.UILetters;
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
			string targetWord){

			ColorAndConfigureFlashForTeacherMode (
				updatedUserInputLetters,
				previousUserInputLetters);

		}

	}



	class MagicEColorer : RuleBasedColorer {
		static Color innerVowelColor = Parameters.Colors.MagicEColors.INNER_VOWEL;
		static Color silentEColor = Parameters.Colors.MagicEColors.SILENT_E;

		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters){

			ColorAndConfigureFlash (updatedUserInputLetters, previousUserInputLetters,
				(Match magicE) => {
					Match previousInstantiationOfRule = SpellingRuleRegex.MagicERegex.Match (previousUserInputLetters);
					return previousInstantiationOfRule.Value != magicE.Value;
				}
			);
		}

		public void ColorAndConfigureFlashForStudentMode(
			string updatedUserInputLetters,
			string previousUserInputLetters, 
			string targetWord){


			ColorAndConfigureFlash (updatedUserInputLetters, previousUserInputLetters, 
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
			Func<Match, bool> shouldFlash
		){
			List<InteractiveLetter> UIletters = State.Current.UILetters;
			Match magicE = SpellingRuleRegex.MagicERegex.Match (updatedUserInputLetters);
			if (!magicE.Success){
				//no match found; switch to open/closed vowel coloring rules.
				OpenClosedVowelColorer openClosed = (OpenClosedVowelColorer)openClosedVowelColorer;
				openClosed.ColorVowels (
					updatedUserInputLetters
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
		string previousUserInputLetters
	);

	void ColorAndConfigureFlashForStudentMode( 
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		string targetWord);


}
	

