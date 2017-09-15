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
	static string[] consonantDigraphs = new string[]{"th","ch","sh","qu","ck","ng","nk","wh"};
	static string consonantDigraph = MatchAnyOf (consonantDigraphs);
	static Regex consonantDigraphRegex = Make(consonantDigraph);
	public static Regex ConsonantDigraph{
		get{
			return consonantDigraphRegex;

		}

	}

		
	//Consonant Blend Regex
	static string[] consonantBlends = new string[]{
		"sp","sh", "sl", "sk", "str", "st", "spr", "scr", "spl", "squ", "shr",
		"ll", "bl", "gl", "cl", "pl", "fl", "cr", "tr", "dr", "ft", "nd", 
		"mp", "nt","thr"
	};
	static string consonantBlend = MatchAnyOf (consonantBlends);
	static Regex consonantBlendRegex = Make(consonantBlend);
	public static Regex ConsonantBlend{
		get {
			return consonantBlendRegex;
		}

	}

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

	static string[] rControlledVowels = new string[]{
		"er", "ur", "or","ir","ar"
	};
	static string rControlledVowel = MatchAnyOf (rControlledVowels);
	static Regex rControlledVowelRegex = Make(rControlledVowel);
	public static Regex RControlledVowel{
		get{
			return rControlledVowelRegex;
		}

	}




	static string anyConsonant = MatchAnyOf(new string[]{consonant, consonantDigraph, consonantBlend});
	static Regex anyConsonantRegex = Make (anyConsonant);
	public static Regex AnyConsonant{
		get {
			return anyConsonantRegex;
		}
	}
	static string anyVowel = MatchAnyOf (new string[]{ vowel, vowelDigraph });
	static Regex anyVowelRegex = Make (anyVowel);
	public static Regex AnyVowel{
		get {
			return anyVowelRegex;
		}

	}

	//Magic-E rule regex
	static Regex magicERule = Make($"({anyConsonant})?({vowel})(?!r)({consonant})e(?!\\w)");
	public static Regex MagicERegex{
		get {
			return magicERule;
		}
	}

	//Open syllable regex
	static string openSyllable = $"({anyConsonant})?({anyVowel})";
	static Regex openSyllableRegex = Make($"{openSyllable}(?!\\w)");//in open/closed syllable mode,
	//we disqualify strings that have characters after the vowel. 
	//but for definitional purposes (and for syllable division mode)
	//an open syllable can exist in words wherein they are followed by (independent) closed syllables
	public static Regex OpenSyllable{
		get {
			return openSyllableRegex;
		}

	}

	//Closed syllable regex
	static string closedSyllable = $"({anyConsonant})?({vowel})(?!r)({anyConsonant})";
	static Regex closedSyllableRegex = Make($"{closedSyllable}(?!e)");
	public static Regex ClosedSyllable{
		get {
			return closedSyllableRegex;
		}

	}

	static Regex any = new Regex(".*");
	static string[] stableSyllables = new string[]{
		$"({anyConsonant})le", $"({anyConsonant})({anyVowel})r"
	};
	static string stableSyllable = MatchAnyOf(stableSyllables);
	static Regex stableSyllableRegex = Make(stableSyllable);
		
	static Regex OneConsonantDivision = Make($"({anyVowel})({anyConsonant})({anyVowel})");
	static Func<int, string> Quantify = (int number) => "{"+number+"}";
	static Regex TwoConsonantDivision = Make($"({anyVowel})({anyConsonant}){Quantify(2)}({anyVowel})");

 
	public static List<Match> Syllabify(String word){
		List<Match> result = new List<Match>();
		Match oneCons = OneConsonantDivision.Match(word);
		if(oneCons.Success){
			
			Match consonant = AnyConsonant.Match(oneCons.Value);
			int startIndexOfConsonantInWord = oneCons.Index + consonant.Index;
			string firstHalf = word.Substring(0, startIndexOfConsonantInWord + consonant.Length);
			string secondHalf = word.Substring(startIndexOfConsonantInWord, word.Length - startIndexOfConsonantInWord);
			if(stableSyllableRegex.IsMatch(secondHalf)){
				result.Add(any.Match(secondHalf));
				result.Add(any.Match(firstHalf.Substring(0,firstHalf.Length-consonant.Length)));
				return result;
			}
			result.Add(any.Match(firstHalf));
			result.Add(any.Match(secondHalf.Substring(startIndexOfConsonantInWord + consonant.Length, word.Length - (startIndexOfConsonantInWord + consonant.Length))));
			return result;
		}

		Match twoCons = TwoConsonantDivision.Match(word);
		if(twoCons.Success){
			Match firstConsonant = AnyConsonant.Match(twoCons.Value);
			int startIndexOfConsonantInWord = twoCons.Index + firstConsonant.Index;
			string firstHalf = word.Substring(0, startIndexOfConsonantInWord + firstConsonant.Length);
			result.Add(any.Match(firstHalf));
			result.Add(any.Match(word.Substring(firstHalf.Length, word.Length-firstHalf.Length)));
			return result;
		}

		return result;
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
		TestSyllabify();

	}
	static Action<Regex, bool, string> testConsDiv= (Regex reg, bool expect, string input) => {
		Match m = reg.Match(input);
		Debug.Log($"Expect {m.Success} to be {expect}: given {input} matched {m.Value}");
	};

	static void TestOneConsonantDivision(){
		testConsDiv(OneConsonantDivision, false, "cat");
		testConsDiv(OneConsonantDivision, true, "water");
		testConsDiv(OneConsonantDivision, true, "wanky");
		testConsDiv(OneConsonantDivision, true, "anky");
		testConsDiv(OneConsonantDivision, true, "maple");
		testConsDiv(TwoConsonantDivision, false, "catnip");

	}

	static void TestTwoConsonantDivision(){
		testConsDiv(TwoConsonantDivision, false, "cat");
		testConsDiv(TwoConsonantDivision, false, "water");
		testConsDiv(TwoConsonantDivision, true, "wanky");
		testConsDiv(TwoConsonantDivision, true, "anky");
		testConsDiv(TwoConsonantDivision, true, "maple");
		testConsDiv(TwoConsonantDivision, true, "catnip");

	}

	static void TestSyllabify(){
		//given
		//relish -> rel ish
		//cabin -> cab in
		//polite -> pol ite
		//admit -> ad mit
		//mascot -> mas cot
		//jacket -> jack et
		//rocket -> rock et
		//respond -> res pond 
		//flan -> returns empty
		//banzwbanan -> returns ban an 
		//bananaz -> returns ban an
		//a -> returns empty
	

		Debug.Log(Syllabify("input").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("relish").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("polite").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("cabin").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("admit").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("jacket").Aggregate("", (string acc, Match m) => acc+" "+m.Value)); 
		Debug.Log(Syllabify("rocket").Aggregate("", (string acc, Match m) => acc+" "+m.Value));
		Debug.Log(Syllabify("respond").Aggregate("", (string acc, Match m) => acc+" "+m.Value));
		Debug.Log(Syllabify("banzwbanan").Aggregate("", (string acc, Match m) => acc+" "+m.Value));
		Debug.Log(Syllabify("maple").Aggregate("", (string acc, Match m) => acc+" "+m.Value));
		Debug.Log(Syllabify("terror").Aggregate("", (string acc, Match m) => acc+" "+m.Value));
	}

	/*
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
		Debug.Log($"Matches cat: {closedSyllable.IsMatch("cat")}");
		Debug.Log($"Matches cat  : {closedSyllable.IsMatch("cat  ")}");
		Debug.Log($"Matches cat  : {closedSyllable.IsMatch("  cat")}");
		Debug.Log($"Matches acat  : {closedSyllable.IsMatch("acat")}");
		Debug.Log($"Matches acata: {closedSyllable.IsMatch("acata")}");
		Debug.Log($"Matches acatat: {closedSyllable.IsMatch("acatat")}");
		Debug.Log($"Matches catcat: {closedSyllable.IsMatch("catcat")}");
		Debug.Log($"Matches chat: {closedSyllable.IsMatch("chat")}");
		Debug.Log($"Matches cash: {closedSyllable.IsMatch("cash")}");
		Debug.Log($"Matches ash: {closedSyllable.IsMatch("ash")}");
		Debug.Log($"Does not match ca: {!closedSyllable.IsMatch("ca")}");
		Debug.Log($"Does not match car: {!closedSyllable.IsMatch("car")}");
		Debug.Log($"Does not match a: {!closedSyllable.IsMatch("a")}");
		Debug.Log($"Does not match c: {!closedSyllable.IsMatch("c")}");
		Debug.Log($"Does not match ch: {!closedSyllable.IsMatch("ch")}");
		Debug.Log($"Does not match game: {!closedSyllable.IsMatch("game")}");
		//don't count r controlled vowels; different syllable.
		Debug.Log($"Does not match car: {!closedSyllable.IsMatch("car")}");
		Debug.Log($"Does not match c a t: {!closedSyllable.IsMatch("c a t")}");
	}


	static void TestOpenSyllable(){
		Debug.Log("--------TEST OPEN SYLLABLE------");
		Debug.Log($"Matches a: {openSyllable.IsMatch("a")}");
		Debug.Log($"Matches aa: {openSyllable.IsMatch("aa")}");
		Debug.Log($"Matches ae: {openSyllable.IsMatch("ae")}");
		Debug.Log($"Matches ca: {openSyllable.IsMatch("ca")}");
		Debug.Log($"Matches caa: {openSyllable.IsMatch("caa")}");
		Debug.Log($"Matches    a: {openSyllable.IsMatch("   a")}");
		Debug.Log($"Matches a  : {openSyllable.IsMatch("a   ")}");
		Debug.Log($"Matches a a a: {openSyllable.IsMatch("a a a")}");
		Debug.Log($"Matches caca (second a): {openSyllable.IsMatch("caca")}");

		Debug.Log($"Does not match c: {!openSyllable.IsMatch("c")}");
		Debug.Log($"Does not match ad: {!openSyllable.IsMatch("ad")}");
		Debug.Log($"Does not match ack: {!openSyllable.IsMatch("ack")}");
		Debug.Log($"Does not match ace: {!openSyllable.IsMatch("ace")}"); //todo... this one fails
		Debug.Log($"Does not match ar: {!openSyllable.IsMatch("ar")}");

	}

	static void TestRControlledVowel(){
		Debug.Log("--------TEST R CONTROLLED VOWEL------");
		Debug.Log($"Matches ar: {rControlledVowelRegex.IsMatch("ar")}");
		Debug.Log($"Matches arr: {rControlledVowelRegex.IsMatch("arr")}");
		Debug.Log($"Matches ark: {rControlledVowelRegex.IsMatch("ark")}");
		Debug.Log($"Matches arka: {rControlledVowelRegex.IsMatch("arka")}");
		Debug.Log($"Matches ara: {rControlledVowelRegex.IsMatch("ara")}");
		Debug.Log($"Matches bar: {rControlledVowelRegex.IsMatch("bar")}");
		Debug.Log($"Matches bara: {rControlledVowelRegex.IsMatch("bara")}");
		Debug.Log($"Matches ar  : {rControlledVowelRegex.IsMatch("ar  ")}");
		Debug.Log($"Matches   ar: {rControlledVowelRegex.IsMatch("  ar")}");
		Debug.Log($"Matches are: {rControlledVowelRegex.IsMatch("are")}");

		Debug.Log($"Does not match ra: {!rControlledVowelRegex.IsMatch("ra")}");
		Debug.Log($"Does not match rad: {!rControlledVowelRegex.IsMatch("rad")}");
		Debug.Log($"Does not match r a: {!rControlledVowelRegex.IsMatch("r a")}");
	}*/

}
