using System.Collections;
using System.Collections.Generic;
using System;

public class DecodedWord  {

	string rawInput;
	public string RawInput{
		get {
			return rawInput;
		}

	}

	//KEYs: types of spelling rules (e.g., MagicE, ConsonantDigraph)
	//Values: List of integer index ranges of the substrings of raw input that match the given rule.
	Dictionary<Rule, List<int[]>> matchedRules;

	public DecodedWord(string rawInput){
		this.rawInput = rawInput;
		matchedRules = new Dictionary<Rule, List<int[]>>();
	}

	public void AddMatchedRule(Rule rule, int[] range){
		if (range.Length != 2)
			throw new Exception ("Invalid range");
		int start = range [0];
		int end = range [1];
		if(start < 0 || end > rawInput.Length - 1 || start - end >= 0) throw new Exception ("Invalid range: {start} {end}");

		List<int[]> matches;
		if(!matchedRules.TryGetValue(rule, out matches)){
			matches = new List<int[]>();
			matchedRules.Add (rule, matches);
		}
		matches.Add (range);
	}


}
