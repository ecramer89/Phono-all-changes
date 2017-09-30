using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Extensions;
	
public class Colorer : PhonoBlocksSubscriber   {
	
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
	static RuleBasedColorer syllableDivisionColorer = new SyllableDivisionColorer();

	static RuleBasedColorer ruleBasedColorer; 

	public override void SubscribeToAll(PhonoBlocksScene forScene){
		if(forScene == PhonoBlocksScene.MainMenu){
			Transaction.Instance.ActivitySelected.Subscribe(this,(Activity activity) => {
				InitializeRuleBasedColorer();
			});

		}

		if(forScene == PhonoBlocksScene.Activity){
			
			Transaction.Instance.SyllableDivisionShowStateToggled.Subscribe(this,ReColor);


			Transaction.Instance.NewProblemBegun.Subscribe(this,(ProblemData problem) => {

				TurnAllLettersOff();

				Transaction.Instance.TargetColorsSet.Fire(
					ruleBasedColorer.GetColorsOf(
						new Color[Parameters.UI.ONSCREEN_LETTER_SPACES], //note that the target colors array includes the 
						//"off" color for positions that aren't occupied by the target word. the different rule based colorers only overwrite
						//indieces that correspond to those of target word.
						problem.targetWord
					));
			});

		}
	}
	public void Start(){
		instance = this;
	}

	static Action InitializeRuleBasedColorer = () => {
		switch (Transaction.Instance.State.Activity) {
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
		case Activity.SYLLABLE_DIVISION:
			ruleBasedColorer = syllableDivisionColorer;
			break;
		}

	};

	static Action TurnAllLettersOff = () => {
		Transaction.Instance.State.UILetters.ForEach (UILetter => {
			
			UILetter.UpdateInputDerivedAndDisplayColor (offColor);
		});
	};


	static Action<string> ReapplyDefaultOrOff = (string updatedUserInputLetters) => {
		int index = 0;
		Transaction.Instance.State.UILetters.ForEach (UILetter => {
			UILetter.UpdateInputDerivedAndDisplayColor (updatedUserInputLetters [index] == ' ' ? offColor : onColor);
			index++;
		});
	};
		

	public void ReColor (){
		string updatedUserInputLetters = Transaction.Instance.State.UserInputLetters;
		string previousUserInputLetters = Transaction.Instance.State.PreviousUserInputLetters;
	
	
		ReapplyDefaultOrOff (
			updatedUserInputLetters
		);

		if (Transaction.Instance.State.Mode == Mode.TEACHER) {
			ruleBasedColorer.ColorAndConfigureFlashForTeacherMode (
				updatedUserInputLetters, 
				previousUserInputLetters
			);
		} else {
			string targetWord = Transaction.Instance.State.TargetWord;
			ruleBasedColorer.ColorAndConfigureFlashForStudentMode (
				updatedUserInputLetters.Substring(0,targetWord.Length), //only want the special rule-based colors
				//to apply to letters that the user placed in the spaces that match those of the target word.
				//letters placed outside of this range are incorrect by default since they don't exist in the target word,
				//and as such, should be turned off (not colored)
				previousUserInputLetters.Substring(0,targetWord.Length),
				targetWord
			);

			TurnOffAndConfigureFlashForErroneousLetters (
				updatedUserInputLetters,
				previousUserInputLetters,
				targetWord
			);
		}

		StartAllLetterFlashes();
	}

	static void StartAllLetterFlashes(){
		foreach(InteractiveLetter letter in Transaction.Instance.State.UILetters){
			letter.StartFlash();
		}
	}

	public static void ChangeDisplayColourOfASingleLetter(int at, Color newDisplayColor){
		if (at >= Transaction.Instance.State.UILetters.Count)
			return;
		Transaction.Instance.State.UILetters [at].UpdateDisplayColour (newDisplayColor);
	}
		
	static void TurnOffAndConfigureFlashForErroneousLetters(string input, string previousInput,string target){
		List<InteractiveLetter> UILetters = Transaction.Instance.State.UILetters;
		for (int i = 0; i < UILetters.Count; i++) {
			InteractiveLetter UILetter = UILetters [i];
			//letter is incorrectly placed if it was placed outside the bounds or the target word OR
			//it doesn't match the letter at the corresponding position in target
			if (i>=target.Length || input [i] != target[i]){ 
				UILetter.UpdateInputDerivedAndDisplayColor (offColor);
				//only flash if it's a new error
				if(previousInput [i] != input [i]) {
					UILetter.ConfigureFlashParameters (
						offColor, onColor, 
						Parameters.Flash.Durations.ERROR_OFF, Parameters.Flash.Durations.ERROR_ON,
						Parameters.Flash.Times.TIMES_TO_FLASH_ERRORNEOUS_LETTER
					);
				}
		
			}

		}
	}

	static void ConfigureFlashOnCompletionOfTargetRule(InteractiveLetter letter, Color a, Color b){
		letter.ConfigureFlashParameters (a, b, 
			Parameters.Flash.Durations.HINT_TARGET_COLOR, Parameters.Flash.Durations.HINT_OFF, 
			Parameters.Flash.Times.TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME
		);
	}

	static Color[] GetColorsOfMultiLetterUnits(Color[] colors, string word, Regex unitRegex, Color unitColor){
		string unmatched = word;
		Match matchedUnit;
		while (true) {
	
			matchedUnit = unitRegex.Match (unmatched);
			if (!matchedUnit.Success)
				break;

		
			for (int i = 0; i < matchedUnit.Length; i++) {
				colors [matchedUnit.Index + i] = unitColor;
			}

			unmatched = unmatched.ReplaceRangeWith ('#', matchedUnit.Index, matchedUnit.Length);
		}

		for (int i = 0; i < unmatched.Length; i++) {
			if (unmatched [i] != '#')
				colors [i] = onColor;

		}

		return colors;

	}

	//return param is a string copy of input userInputLetters, with all letters that were colored replaced with blanks. 
	static string ColorAllInstancesOfMultiLetterUnit(
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		Regex spellingRuleRegex,
		Color multiLetterUnitColor){
		List<InteractiveLetter> UIletters = Transaction.Instance.State.UILetters;
		//find and match each successive blend.
		//because some strings could contain blends that cross boundaries (e.g., bll)
		//input to the regex match is a buffer from which we replace the 
		//blend letters with blanks on each iteration with blanks
		string unmatchedUserInputLetters=updatedUserInputLetters;
		Match multiLetterUnit;
		while(true){
			multiLetterUnit = spellingRuleRegex.Match (unmatchedUserInputLetters);
			if(!multiLetterUnit.Success) break;
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
		List<InteractiveLetter> UIletters = Transaction.Instance.State.UILetters;
		string unmatchedTargetInputLetters=targetWord;
		Match targetUnit;
		while(true){

			targetUnit = spellingRuleRegex.Match (unmatchedTargetInputLetters);

			if (!targetUnit.Success)
				break;
			
			unmatchedTargetInputLetters = unmatchedTargetInputLetters.ReplaceRangeWith(' ', targetUnit.Index, targetUnit.Length);
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
		//valid blended consonants get colored green
		//after removing all valid blended consonants; single consonants get colored blue.
		Color blendedColor = Parameters.Colors.ConsonantBlendColors.COMPLETED_BLEND_COLOR;
		Color singleConsonantColor = Parameters.Colors.ConsonantBlendColors.SINGLE_CONSONANT_COLOR;
		//apart from coloring single consonants of target blend; we don't otherwise hint them (i.e., 
		//no flashing for consonant blends)
		Action<InteractiveLetter> hintNewPartialMatches = (InteractiveLetter letter)=>{return;};


		public Color[] GetColorsOf(Color[] colors, string word){
			return GetColorsOfMultiLetterUnits (
				colors,
				word, 
				SpellingRuleRegex.ConsonantBlend, 
				blendedColor
			);
		}


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
				InteractiveLetter consonantLetter = Transaction.Instance.State.UILetters.ElementAt (consonant.Index);
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

		public Color[] GetColorsOf(Color[] colors, string word){
			return GetColorsOfMultiLetterUnits (
				colors,
				word, 
				SpellingRuleRegex.ConsonantDigraph, 
				digraphColor
			);
		}


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

		public Color[] GetColorsOf(Color[] colors, string word){
			return GetColorsOfMultiLetterUnits (
				colors,
				word, 
				SpellingRuleRegex.VowelDigraph, 
				digraphColor
			);
		}


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


		public Color[] GetColorsOf(Color[] colors, string word){
			return GetColorsOfMultiLetterUnits (
				colors,
				word, 
				SpellingRuleRegex.RControlledVowel, 
				rControlledVowelColor
			);
		}


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
	
	
		public Color[] GetColorsOf(Color[] colors, string word){
			string unmatched = word;

			Match matchedSyllable;

			while (true) {
				Match matchClosed = SpellingRuleRegex.ClosedSyllable.Match (unmatched);
				Match matchOpen = SpellingRuleRegex.OpenSyllable.Match (unmatched);

				matchedSyllable = matchClosed.Success ? matchClosed : matchOpen;

				if (!matchedSyllable.Success)
					break;

				MatchCollection consonants = SpellingRuleRegex.AnyConsonant.Matches (matchedSyllable.Value);
				foreach (Match consonant in consonants) {
					for (int i = 0; i < consonant.Value.Length; i++) {
						colors [matchedSyllable.Index + consonant.Index + i] = i % 2 == 0 ? consonantColorFirstAlternation : consonantColorSecondAlternation;
					}

				}

				Match vowel = SpellingRuleRegex.AnyVowel.Match (matchedSyllable.Value);
				for (int i = 0; i < vowel.Value.Length; i++) {
					colors [matchedSyllable.Index + vowel.Index + i] = matchClosed.Success ? shortVowelColor : longVowelColor;
				}
					
				unmatched = unmatched.ReplaceRangeWith ('#', matchedSyllable.Index, matchedSyllable.Length);
			}

			//any letters that weren't replaced with # were not matched.
			//so just let them be white.
			for (int i = 0; i < unmatched.Length; i++) {
				if (unmatched [i] != '#')
					colors [i] = onColor;
			}

			return colors;

		}


		public void ColorAndConfigureFlashForTeacherMode(
			string updatedUserInputLetters, 
			string previousUserInputLetters){

			//color consonants in alternating blue-green.
			ColorConsonants(updatedUserInputLetters);
			//color vowels by syllable type.
			ColorVowels(updatedUserInputLetters);

		}

		void ColorConsonants(string updatedUserInputLetters){
			List<InteractiveLetter> UIletters = Transaction.Instance.State.UILetters;
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
			List<InteractiveLetter> UIletters = Transaction.Instance.State.UILetters;
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


		public Color[] GetColorsOf(Color[] colors, string word){
			//assumes there would just be one instance of a magic e word in input.
			//generalizing to longer strings (i.e. sentences) requires same recursive trick as before

			Match magicE = SpellingRuleRegex.MagicERegex.Match (word);
			if (magicE.Success) {
				MatchCollection consonants = SpellingRuleRegex.AnyConsonant.Matches (magicE.Value);
				foreach (Match consonant in consonants) {
					for (int i = 0; i < consonant.Value.Length; i++) {
						colors [magicE.Index + consonant.Index + i] = onColor;
					}

				}

				Match innerVowel = SpellingRuleRegex.AnyVowel.Match (magicE.Value);
				for (int i = 0; i < innerVowel.Value.Length; i++) {
					colors [magicE.Index + innerVowel.Index + i] = innerVowelColor;
				}

				//assume that silent e corresponds to the last character in the magicE match
				colors [magicE.Index + magicE.Length - 1] = silentEColor;
			}

			//any indices in word that don't correspond to the magic e match become white
			for (int i = 0; i < colors.Length; i++) {
				if( i < magicE.Index && i >= magicE.Index + magicE.Length) colors [i] = Color.white;
			}


			return colors;

		}

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
			List<InteractiveLetter> UIletters = Transaction.Instance.State.UILetters;
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

	class SyllableDivisionColorer : RuleBasedColorer{
		static Color wholeWordColor = Parameters.Colors.SyllableDivisionColors.WHOLE_WORD_COLOR;
		static Color dividedFirstSyllableColor = Parameters.Colors.SyllableDivisionColors.DIVIDED_FIRST_SYLLABLE_COLOR;
		static Color dividedSecondSyllableColor = Parameters.Colors.SyllableDivisionColors.DIVIDED_SECOND_SYLLABLE_COLOR;

		static Func<int, Color> AlternateBy = (int seed) => seed%2==0 ? dividedFirstSyllableColor : dividedSecondSyllableColor;



		static Action<string> colorSyllables = (string updatedUserInputLetters)=>{
			List<Match> syllables = SpellingRuleRegex.Syllabify(updatedUserInputLetters);
			//note: we should take the trouble of parsing the input into sylables even if it's the "whole word" coloring code.
			//the rationale is that syllabify algorithm won't include phonotactically illegal letters 
			//(e.g., "banwnban", will return "ban ban") so these will be colored white so as to indicate that they
			//have no place in any phonotactically legal string.
			for(int i=0;i<syllables.Count;i++){
				Match syllable = syllables[i];
				List<InteractiveLetter> letters = Transaction.Instance.State.UILetters.GetRange(syllable.Index, syllable.Length);
			
				foreach(InteractiveLetter letter in letters){
					letter.UpdateInputDerivedAndDisplayColor(
						Transaction.Instance.State.SyllableDivisionShowState == SyllableDivisionShowStates.SHOW_WHOLE_WORD ? 
						wholeWordColor : AlternateBy(i));
				}
			}

		};


		public void ColorAndConfigureFlashForTeacherMode (
			string updatedUserInputLetters,
			string previousUserInputLetters
		){
			colorSyllables(updatedUserInputLetters);
		}

		public void ColorAndConfigureFlashForStudentMode( 
			string updatedUserInputLetters, 
			string previousUserInputLetters,
			string targetWord){

			//if it's whole word mode, color each letter pink provided it matches the target word.
			//if it's show division mode, then only if the user input letter currently matches target
			//should we show the divided colors.
			bool divided=Transaction.Instance.State.SyllableDivisionShowState == SyllableDivisionShowStates.SHOW_DIVISION;
			if(divided && targetWord == updatedUserInputLetters.Trim()){
				colorSyllables(updatedUserInputLetters);
				return;
			}

			//otherwise we just color the letters pink, provided they are correctly placed.
			List<InteractiveLetter> letters = Transaction.Instance.State.UILetters.GetRange(0, targetWord.Length);
			for(int i=0;i<letters.Count;i++){
				InteractiveLetter letter = letters[i];
				if(updatedUserInputLetters[i] == targetWord[i]){
					letter.UpdateInputDerivedAndDisplayColor(wholeWordColor);
				}
			}
		}

		//the target word colors (which are basically used only in hints anyway)
		//for syllable division mode are the divided colors, not the whole word, since the divided colors
		//serve as hints to some extent.
		public Color[] GetColorsOf (Color[] colors, string word){
			List<Match> syllables = SpellingRuleRegex.Syllabify(word);
			for(int syllableIndex=0;syllableIndex<syllables.Count;syllableIndex++){
				Match syllable = syllables[syllableIndex];
				for(int letterIndex=syllable.Index;letterIndex<syllable.Index+syllable.Length;letterIndex++){
					colors[letterIndex]=AlternateBy(syllableIndex);
				}
			}
			return colors;
		}

	}


}

//todo, these functions should all query state for necessary inputs. 
interface RuleBasedColorer{
	void ColorAndConfigureFlashForTeacherMode (
		string updatedUserInputLetters,
		string previousUserInputLetters
	);

	void ColorAndConfigureFlashForStudentMode( 
		string updatedUserInputLetters, 
		string previousUserInputLetters,
		string targetWord);

	Color[] GetColorsOf (Color[] colors, string word);


}
	

