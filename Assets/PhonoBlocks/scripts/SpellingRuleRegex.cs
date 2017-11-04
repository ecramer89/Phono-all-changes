using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using UnityEngine;
using Extensions;

public static class SpellingRuleRegex  {

	//Vowel Regex
	static string[] vowels = new string[]{"a","e","i","o","u", "y"};
	static string vowel = MatchAnyOf (vowels);
	static Regex vowelRegex = Make(vowel);

	//Consonant Regex
	static string[] consonants = new string[]{"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","z"};
	static string consonant = MatchAnyOf(consonants);
	static Regex consonantRegex = Make(consonant);
	public static Regex Consonant{
		get {

			return consonantRegex;
		}

	}

	//Consonant Digraph regex
	//can only appear at the end of a syllable
	static string[] consonantDigraphsInitial = new string[]{
		"qu", "wh"
	};
	//can only appear at the beginning of a syllable
	static string[] consonantDigraphsFinal = new string[]{
		"ck", "ng","nk"
	};

	static string[] consonantDigraphsEither = new string[]{
		"th","ch","sh"
	};

	//Consonant Blend Regex
	//can only appear at the end of a syllable
	static string[] consonantBlendsFinal = new string[]{
		"ft","rn", "nd", "mp", "nt","thr", "nz","str","rt"
	};


	//can only appear at the beginning of a syllable
	static string[] consonantBlendsInitial = new string[]{
		"sl", "spr", "scr", "spl", "squ", "shr", 
		"bl", "gl", "pl", "cl", "fl", "cr", "tr", "dr"
	};

	static string[] consonantBlendsEither = new string[]{"sh", "sp", "sk", "st", "ll"};

	//I didn't want to use HashSet since it allocates much more space than is required.
	//just built out own d.s. that serves similar function.
	//can do fast lookup using the int version of the first letter characters to access the 
	static readonly int ASCI_a = 97;
	static bool[] consonantDigraphFirstLetters;
	static bool[] consonantBlendsFirstLetters;
	private static void AddFirstLetterOfUnitsToSet(string[] units, bool[] letterLookup){
		foreach(string unit in units){
			letterLookup[(int)(unit[0])-ASCI_a]=true;
		}
	}
	public static bool IsFirstLetterOfConsonantDigraph(string letter){
		//lazily initialize the set
		if(consonantDigraphFirstLetters==null){
			consonantDigraphFirstLetters=new bool[26];
			AddFirstLetterOfUnitsToSet(consonantDigraphsInitial, consonantDigraphFirstLetters);
			AddFirstLetterOfUnitsToSet(consonantDigraphsFinal, consonantDigraphFirstLetters);
			AddFirstLetterOfUnitsToSet(consonantDigraphsEither, consonantDigraphFirstLetters);
		}
		return consonantDigraphFirstLetters[(int)(letter[0])-ASCI_a];
	}

	public static bool IsFirstLetterOfConsonantBlend(string letter){
		//lazily initialize the set
		if(consonantBlendsFirstLetters==null){
			consonantBlendsFirstLetters=new bool[26];
			AddFirstLetterOfUnitsToSet(consonantBlendsFinal, consonantBlendsFirstLetters);
			AddFirstLetterOfUnitsToSet(consonantBlendsInitial, consonantBlendsFirstLetters);
			AddFirstLetterOfUnitsToSet(consonantBlendsEither, consonantBlendsFirstLetters);
		}
		return consonantBlendsFirstLetters[(int)(letter[0])-ASCI_a];
	}
		
	static string[] consonantDigraphs = consonantDigraphsEither.Concat(consonantDigraphsInitial).Concat(consonantDigraphsFinal).ToArray();
	static string consonantDigraph = MatchAnyOf (consonantDigraphs);
	static Regex consonantDigraphRegex = Make(consonantDigraph);
	public static Regex ConsonantDigraph{
		get{
			return consonantDigraphRegex;

		}

	}


	//those defined within initial literal can appear in either position within a syllable.
	//contenate to initial and final blends for all blends.
	static string[] consonantBlends = consonantBlendsEither.Concat(consonantBlendsFinal).Concat(consonantBlendsInitial).ToArray();


	static string consonantBlend = MatchAnyOf (consonantBlends);
	static Regex consonantBlendRegex = Make(consonantBlend);
	public static Regex ConsonantBlend{
		get {
			return consonantBlendRegex;
		}

	}

	static string[] acceptableInitialConsonantUnits=consonantBlendsEither.Concat(consonantBlendsInitial).Concat(consonantDigraphsEither).Concat(consonantDigraphsInitial).ToArray();
	static string[] acceptableFinalConsonantUnits =consonantBlendsEither.Concat(consonantBlendsFinal).Concat(consonantDigraphsEither).Concat(consonantDigraphsFinal).ToArray();

	static string anyInitialConsonantUnit = MatchAnyOf(acceptableInitialConsonantUnits);
	static string anyFinalConsonantUnit = MatchAnyOf(acceptableFinalConsonantUnits);


	static Regex anyInitialUnitRegex = Make($"{anyInitialConsonantUnit}|{vowelDigraph}");
	static Regex anyFinalUnitRegex = Make($"{anyFinalConsonantUnit}|{vowelDigraph}");


	static string[] vowelDigraphs = new string[] {
		"ea", "ai", "ae", "aa", "ee", "ie", "oe", "ue", "ou", "ay", "oa"
	};
	static string vowelDigraph = MatchAnyOf (vowelDigraphs);
	static Regex vowelDigraphRegex = Make(vowelDigraph);
	public static Regex VowelDigraph{
		get {
			return vowelDigraphRegex;
		}

	}

	static string[] anyRFinalBlendOrDigraph = acceptableFinalConsonantUnits.Where(blend=>blend[0]=='r').ToArray();


	static string anyConsonant = MatchAnyOf(new string[]{consonantDigraph, consonantBlend, consonant});
	static Regex anyConsonantRegex = Make (anyConsonant);
	public static Regex AnyConsonant{
		get {
			return anyConsonantRegex;
		}
	}


	//order matters here. 
	//syllable division- need to keep consonant blends/digraphs together (i.e. jacket -< jack and et not jac and ket).
	//as such, be sure to put digraphs and blends ahead of single consonants so that it matches the larger units first.
	public static string acceptableInitialConsonant = $"({anyInitialConsonantUnit}|({consonant}))";
	public static string acceptableFinalConsonant = $"({anyFinalConsonantUnit}|({consonant}))";


	static string anyVowel = MatchAnyOf (new string[]{vowelDigraph, vowel });
	static Regex anyVowelRegex = Make (anyVowel);
	public static Regex AnyVowel{
		get {
			return anyVowelRegex;
		}

	}

	static string rControlledVowel = $"({anyVowel})({MatchAnyOf (anyRFinalBlendOrDigraph)}|r)";
	static Regex rControlledVowelRegex = Make(rControlledVowel);
	public static Regex RControlledVowel{
		get{
			return rControlledVowelRegex;
		}

	}


	//Magic-E rule regex
	static Regex magicERule = Make($"({acceptableInitialConsonant})?({vowel})(?!r)({acceptableFinalConsonant})e(?!\\w)");
	public static Regex MagicERegex{
		get {
			return magicERule;
		}
	}

	//Open syllable regex
	static string openSyllable = $"({acceptableInitialConsonant})?({anyVowel})";
	static Regex openSyllableRegex = Make($"{openSyllable}(?!\\w)");
	public static Regex OpenSyllable{
		get {
			return openSyllableRegex;
		}

	}

	//Closed syllable regex
	static string closedSyllable = $"({acceptableInitialConsonant})?({anyVowel})(?!r)({acceptableFinalConsonant})";
	static Regex closedSyllableRegex = Make($"{closedSyllable}");
	public static Regex ClosedSyllable{
		get {
			return closedSyllableRegex;
		}
	}
		
	static string consonantLeSyllable = $"({anyConsonant})le$";


	static string rControlledVowelSyllable=$"{acceptableInitialConsonant}?{rControlledVowel}e?";
	static string vowelYSyllable=$"({acceptableInitialConsonant})y";

	static string[] stableSyllables = new string[]{
		consonantLeSyllable, rControlledVowelSyllable, vowelYSyllable};

	static string stableSyllable = MatchAnyOf(stableSyllables);
	static Regex stableSyllableRegex = Make(stableSyllable);

	static Regex[] closedAndOpenSyllables = new Regex[]{ClosedSyllable,OpenSyllable};


	/*
     * order is important. needs to find stable syllables first, then magic e, then closed, then open.
     * any letters that aren't included in syllables (e.g. randomly interjected consonants) won't be included
     * in anything in the returned list of matches.
     * each match has indices that correspond to the match's position relative to the entire input word.
     * e.g., input "maple" -> "ma" and "ple", index of "ma" =0 and index of "ple" = 2
     * 
     * */
	public static List<Match> Syllabify(String word){
		List<Match> syllables = new List<Match>();
		word = ExtractAll(stableSyllableRegex,word,syllables); 
		word = ExtractAll(MagicERegex, word, syllables);
		word = ExtractAll(ClosedSyllable, word, syllables);
		word = ExtractAll(OpenSyllable, word, syllables);
		syllables.Sort((Match x, Match y) => x.Index - y.Index);
		return syllables;
	}

	//tricky part is preserving the length and indices of the input word so that the resulting match objects have indices
	//that will be aligned with the input word. 
	//accomplish this by getting matches one at a time, each time get a match, replace those letters with blanks
	//in the inut word so that they won't be matched again
	//this way the colorers can use the match indices to determine which range of UI letters to color.
	static string ExtractAll(Regex rule,String word, List<Match> results){
		while(true){
			Match next = rule.Match(word);
			if(!next.Success) return word;
			results.Add(next);
			word = word.ReplaceRangeWith(' ', next.Index, next.Length);
		}
		return word;
	}

	static string MatchAnyOf(string[] patterns){
		return patterns.Aggregate((acc, nxt)=>$"{acc}|{nxt}");
	}
	//convenience 'factory-style' method; just saves me having to remember to add the case insensitive
	//modifer to each of the regexes.
	static Regex Make(string pattern){
		return new Regex (pattern, RegexOptions.IgnoreCase);
	}
			

}
