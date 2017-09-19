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
	static string consonant = $"[^\\W,^{vowel}]";
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


	static string[] consonantDigraphs = consonantDigraphsEither.Concat(consonantDigraphsInitial).Concat(consonantDigraphsFinal).ToArray();
	static string consonantDigraph = MatchAnyOf (consonantDigraphs);
	static Regex consonantDigraphRegex = Make(consonantDigraph);
	public static Regex ConsonantDigraph{
		get{
			return consonantDigraphRegex;

		}

	}

		
	//Consonant Blend Regex
	//can only appear at the end of a syllable
	static string[] consonantBlendsFinal = new string[]{
		"ft","rn", "nd", "mp", "nt","thr", "nz","str"
	};


	//can only appear at the beginning of a syllable
	static string[] consonantBlendsInitial = new string[]{
		"sp", "sl", "spr", "scr", "spl", "squ", "shr", "bl", "gl", "pl", "cl", "fl", "cr", "tr", "dr"
	};

	static string[] consonantBlendsEither = new string[]{"sh", "sk", "st", "ll"};

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

	static string anyInitialConsonantUnit = MatchAnyOf(consonantBlendsEither.Concat(consonantBlendsInitial).Concat(consonantDigraphsEither).Concat(consonantDigraphsInitial).ToArray());
	static Regex anyInitialUnitRegex = Make($"{anyInitialConsonantUnit}|{vowelDigraph}");
	static string anyFinalConsonantUnit = MatchAnyOf(consonantBlendsEither.Concat(consonantBlendsFinal).Concat(consonantDigraphsEither).Concat(consonantDigraphsFinal).ToArray());
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
		
	static string consonantLeSyllable = $"({anyConsonant})le";
	static string rControlledVowelSyllable=$"({acceptableInitialConsonant})?({anyVowel})r";
	static string vowelYSyllable=$"({acceptableInitialConsonant})y";
	static string[] stableSyllables = new string[]{
		consonantLeSyllable, rControlledVowelSyllable, vowelYSyllable};

	static string stableSyllable = MatchAnyOf(stableSyllables);
	static Regex stableSyllableRegex = Make(stableSyllable);

	static Regex[] closedAndOpenSyllables = new Regex[]{ClosedSyllable,OpenSyllable};


	static Func<int,Match,bool> IsPartOfValidUnit = (int index, Match inMatch) => {
		Match validUnit = index <= inMatch.Index ? anyInitialUnitRegex.Match(inMatch.Value) : anyFinalUnitRegex.Match(inMatch.Value);
		return (Range.Includes(validUnit.Index, validUnit.Index+validUnit.Length, index-inMatch.Index));
	};		
	static Func<Match, Match, int, bool> unitsKeepTogetherLongerBeatShorter = (Match contender, Match currentWinner, int overlapAt) => {
		//if they're arguing over an index where
		//for one of the contenders, it's part of a valid multi letter unit,
		//then that contender wins.
		//otherwise, pick the longer one.
		return IsPartOfValidUnit(overlapAt, contender) ? true :  IsPartOfValidUnit(overlapAt, currentWinner) ? false : contender.Length > currentWinner.Length;
	};

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
		word = ExtractAll(closedAndOpenSyllables, word, syllables, unitsKeepTogetherLongerBeatShorter);
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


	/*
	 * decider: should return true if the left hand match would be chosen over the right hand match and false
	 * if the right hand match would return true over the left hand match. Cannot choose both. 
	 * 
	 * */

	static string ExtractAll(Regex[] rules, String word, List<Match> results, Func<Match,Match,int,bool> leftWins){
		while(true){
			var matches = rules.Select((Regex rule)=>rule.Match(word)).Where(match=>match.Success);
			if(matches.Count()==0) return word;
			for(int i=0;i<word.Length;i++){
				if(matches.Count() == 0) return word;
				if(word[i]==' ') continue;
				var contenders = matches.Where(match=>Range.Includes(match.Index, match.Index+match.Length, i));
				if(contenders.Count() == 0) continue;
					Match winner=null;
					foreach(Match contender in contenders){
					if(winner == null || 
						leftWins(contender, winner, 
							Range.IndexOfOverlap(contender.Index, contender.Index + contender.Length, winner.Index, winner.Index+winner.Length))) 
						winner = contender;
					}
					results.Add(winner);
					word=word.ReplaceRangeWith(' ',winner.Index, winner.Length);
					matches=matches.Except(contenders);
			}
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
		

	public static void Test(){

	}


	static void TestSyllabify(){
		Debug.Log($"expect wa ter {Syllabify("water").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect in put {Syllabify("input").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect rel ish {Syllabify("relish").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect po lite {Syllabify("polite").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect ca bin {Syllabify("cabin").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect ad mit {Syllabify("admit").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect jack et {Syllabify("jacket").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}"); 
		Debug.Log($"expect rock et {Syllabify("rocket").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect res pond {Syllabify("respond").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect banz ban an {Syllabify("banzwbanan").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect ma ple {Syllabify("maple").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect ter ror {Syllabify("terror").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");

		Debug.Log($"expect cree py {Syllabify("creepy").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect ca ble {Syllabify("cable").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect o ver {Syllabify("over").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect ang er {Syllabify("anger").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
		Debug.Log($"expect ri der {Syllabify("rider").Aggregate("", (string acc, Match m) => acc+" "+m.Value)}");
	}


	static void TestRControlledVowel(){
		Debug.Log("--------TEST R CONTROLLED VOWEL------");
		Debug.Log($"Matches ar: {rControlledVowelRegex.Match("ar").Value}");
		Debug.Log($"Matches arr: {rControlledVowelRegex.Match("arr").Value}");
		Debug.Log($"Matches ark: {rControlledVowelRegex.Match("ark").Value}");
		Debug.Log($"Matches arka: {rControlledVowelRegex.Match("arka").Value}");
		Debug.Log($"Matches ara: {rControlledVowelRegex.Match("ara").Value}");
		Debug.Log($"Matches bar: {rControlledVowelRegex.Match("bar").Value}");
		Debug.Log($"Matches bara: {rControlledVowelRegex.Match("bara").Value}");
		Debug.Log($"Matches ar  : {rControlledVowelRegex.Match("ar  ").Value}");
		Debug.Log($"Matches   ar: {rControlledVowelRegex.Match("  ar").Value}");
		Debug.Log($"Matches are: {rControlledVowelRegex.Match("are").Value}");
		Debug.Log($"Does not match r (without vowel): {!rControlledVowelRegex.IsMatch("r")}");

		Debug.Log($"Matches car: {rControlledVowelRegex.Match("car")}");
		Debug.Log($"Matches jar: {rControlledVowelRegex.Match("jar")}");
		Debug.Log($"Matches fir: {rControlledVowelRegex.Match("fir")}");

		Debug.Log($"Matches hurt; captures final cons. blend: {rControlledVowelRegex.Match("hurt").Value}");
		Debug.Log($"Matches horn; captures final cons. blend: {rControlledVowelRegex.Match("horn").Value}");
		Debug.Log($"Matches part; captures final cons. blend: {rControlledVowelRegex.Match("part").Value}");
		Debug.Log($"Matches intern; captures final cons. blend: {rControlledVowelRegex.Match("intern").Value}");
	

		Debug.Log($"Does not match ra: {!rControlledVowelRegex.IsMatch("ra")}");
		Debug.Log($"Does not match rad: {!rControlledVowelRegex.IsMatch("rad")}");
		Debug.Log($"Does not match r a: {!rControlledVowelRegex.IsMatch("r a")}");
	}



	static void TestAcceptableEndAndInitialConsonant(){
		Regex init = Make(acceptableInitialConsonant);
		Regex end = Make(acceptableFinalConsonant);
		foreach(string initBlend in consonantBlendsInitial){
			Match mn = init.Match(initBlend);
			Debug.Log($"expect match is true: {mn.Success} and matches blend {initBlend} = {mn.Value}? ");

		}
		foreach(string initDig in consonantDigraphsInitial){
			Match mn = init.Match(initDig);
			Debug.Log($"expect match is true: {mn.Success} and matches blend {initDig} = {mn.Value}? ");

		}

		foreach(string endBlend in consonantBlendsFinal){
			Match mn = end.Match(endBlend);
			Debug.Log($"expect match is true: {mn.Success} and matches blend {endBlend} = {mn.Value}? ");

		}
		foreach(string endDig in consonantDigraphsFinal){
			Match mn = end.Match(endDig);
			Debug.Log($"expect match is true: {mn.Success} and matches blend {endDig} = {mn.Value}? ");

		}
	}




	static void TestMatchMagicERule(){
		Debug.Log("--------TEST MAGIC E RULE------");
		Debug.Log ($"Matches game: {magicERule.IsMatch("game")}"); 
		//handles extra trailing space
		Debug.Log ($"Matches game   : {magicERule.IsMatch("game   ")}"); 
		//handles extra preceding space
		Debug.Log ($"Matches   game: {magicERule.IsMatch("  game")}"); 
		//handles charactes before
		Debug.Log ($"Matches nogame: {magicERule.IsMatch("nogame")}"); 
		Debug.Log ($"Matches shame: {magicERule.IsMatch("shame")}"); 
		Debug.Log ($"Matches ike: {magicERule.IsMatch("ike")}"); 
		Debug.Log ($"Does not match shanke: {!magicERule.IsMatch("shanke")}");
		Debug.Log ($"Does not match sa: {!magicERule.IsMatch("sa")}");
		Debug.Log ($"Does not match sas: {!magicERule.IsMatch("sas")}");
		Debug.Log ($"Does not match sass: {!magicERule.IsMatch("sass")}");
		Debug.Log ($"Does not match asp: {!magicERule.IsMatch("asp")}");
		Debug.Log ($"Does not match as: {!magicERule.IsMatch("as")}");
		Debug.Log ($"Does not match a: {!magicERule.IsMatch("as")}");
		Debug.Log ($"Does not match gamer: {!magicERule.IsMatch("gamer")}"); 
		Debug.Log ($"Does not match g a m e: {!magicERule.IsMatch("g a m e")}");
		//don't count r controlled vowels; different syllable.
		Debug.Log ($"Does not match lore: {!magicERule.IsMatch("lore")}");

	}

	static void TestClosedSyllable(){
		Debug.Log("--------TEST CLOSED SYLLABLE------");
		Debug.Log($"Matches cat: {ClosedSyllable.IsMatch("cat")}");
		Debug.Log($"Matches cat  : {ClosedSyllable.IsMatch("cat  ")}");
		Debug.Log($"Matches cat  : {ClosedSyllable.IsMatch("  cat")}");
		Debug.Log($"Matches acat  : {ClosedSyllable.IsMatch("acat")}");
		Debug.Log($"Matches acata: {ClosedSyllable.IsMatch("acata")}");
		Debug.Log($"Matches acatat: {ClosedSyllable.IsMatch("acatat")}");
		Debug.Log($"Matches catcat: {ClosedSyllable.IsMatch("catcat")}");
		Debug.Log($"Matches chat: {ClosedSyllable.IsMatch("chat")}");
		Debug.Log($"Matches cash: {ClosedSyllable.IsMatch("cash")}");
		Debug.Log($"Matches ash: {ClosedSyllable.IsMatch("ash")}");
		Debug.Log($"Does not match ca: {!ClosedSyllable.IsMatch("ca")}");
		Debug.Log($"Does not match car: {!ClosedSyllable.IsMatch("car")}");
		Debug.Log($"Does not match a: {!ClosedSyllable.IsMatch("a")}");
		Debug.Log($"Does not match c: {!ClosedSyllable.IsMatch("c")}");
		Debug.Log($"Does not match ch: {!ClosedSyllable.IsMatch("ch")}");
		Debug.Log($"Does not match game: {!ClosedSyllable.IsMatch("game")}");
		Debug.Log($"Does not match c: {!ClosedSyllable.IsMatch("c")}");
		Debug.Log($"Does not match ct: {!ClosedSyllable.IsMatch("ct")}");
		//don't count r controlled vowels; different syllable.
		Debug.Log($"Does not match car: {!ClosedSyllable.IsMatch("car")}");
		Debug.Log($"Does not match c a t: {!ClosedSyllable.IsMatch("c a t")}");
	}


	static void TestOpenSyllable(){
		Debug.Log("--------TEST OPEN SYLLABLE------");
		Debug.Log($"Matches a: {OpenSyllable.IsMatch("a")}");
		Debug.Log($"Matches aa: {OpenSyllable.IsMatch("aa")}");
		Debug.Log($"Matches ae: {OpenSyllable.IsMatch("ae")}");
		Debug.Log($"Matches ca: {OpenSyllable.IsMatch("ca")}");
		Debug.Log($"Matches caa: {OpenSyllable.IsMatch("caa")}");
		Debug.Log($"Matches    a: {OpenSyllable.IsMatch("   a")}");
		Debug.Log($"Matches a  : {OpenSyllable.IsMatch("a   ")}");
		Debug.Log($"Matches a a a: {OpenSyllable.IsMatch("a a a")}");
		Debug.Log($"Matches caca (second a): {OpenSyllable.IsMatch("caca")}");

		Debug.Log($"Does not match c: {!OpenSyllable.IsMatch("c")}");
		Debug.Log($"Does not match ad: {!OpenSyllable.IsMatch("ad")}");
		Debug.Log($"Does not match ack: {!OpenSyllable.IsMatch("ack")}");
		Debug.Log($"Does not match ace: {!OpenSyllable.IsMatch("ace")}"); //todo... this one fails
		Debug.Log($"Does not match ar: {!OpenSyllable.IsMatch("ar")}");

	}
		

}
