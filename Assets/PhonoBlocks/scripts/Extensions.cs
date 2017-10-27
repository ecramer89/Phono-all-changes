namespace Extensions{
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


/*
 * c# doesn't currently support Static extension methods, but similar effect achieved by just defining these Static
 * _ classes that expose different utility-type functions that operate on instances of the class.
 * 
 * 
 * */
public static class _String{
	 //construct a new String consisting of 'with' the given number of times.
	public static String Fill(String with, int times){
		StringBuilder res = new StringBuilder();
		for(int i=0;i<times;i++){
			res.Append(""+with);
		}
		return res.ToString();
	}
}


		

	public static class Range{

	
		public static bool Overlap(int aStart, int aEnd, int bStart, int bEnd){
			if(aEnd <= aStart || bEnd<bStart) throw new Exception("Invalid range");
			int halfwaythroughA = (aEnd-aStart)/2;
			int halfwaythroughB = (bEnd-bStart)/2;
			return Math.Abs(halfwaythroughA - halfwaythroughB) < halfwaythroughA + halfwaythroughB;
		}

		public static bool Includes(int rangeStart, int rangeEnd, int value){
			if(rangeEnd <= rangeStart) throw new Exception("Invalid range");
			return value <= rangeEnd && value >= rangeStart;
		}

		public static int IndexOfOverlap(int aStart, int aEnd, int bStart, int bEnd){
			if(!Overlap(aStart, aEnd, bStart, bEnd)) throw new Exception("Ranges do not overlap.");
			return Math.Abs(aStart-bStart)+(aStart < bStart ? aStart : bStart);
		}

	}


	public static class PhonoBlocksExtensions {


		public static Match Longest(this MatchCollection matches){
			Match max = null;
			foreach(Match m in matches){
				if(max == null || max.Length < m.Length) max = m;
			}
			return max;
		}

		//returns a new string representing the union of this and other.
		//return string is as long as the longer of this and other
		//each position has the character at position of this or other
		//blanks are treated as false
		//non blank character of this has precedence.
		//e.g., given
		//"fl k" and "  ag" will return "flak".
		public static String Union(this String str, String other){
			if (other == null || other.Length == 0)
				return str;
			
			if (str.Length == 0)
				return other;
			
			StringBuilder builder = new StringBuilder ();
			int i = 0;
			for (; i < str.Length; i++) {
				if (i >= other.Length) {
					builder.Append (str [i]);
					continue;
				}
				char inThis = str [i];
				char inOther = other [i];
				builder.Append (inThis == ' ' ? inOther : inThis);
			}
			while (i < other.Length) {
				builder.Append (other [i]);
			}

			return builder.ToString ();
		}


		public static String ReplaceRangeWith(this String str, char with, int start, int length){
			StringBuilder buff = new StringBuilder ();
			buff.Append(str.Substring(0, start));
			buff.Append(_String.Fill($"{with}", length));
			if(start + length < str.Length) buff.Append (str.Substring (start + length, str.Length - start - length));
			return buff.ToString ();
		}


		public static String ReplaceAt(this String str, int at, char with){
				if(at < 0 || at > str.Length) throw new Exception($"{at} is out of bounds of {str}");
				char[] arr = str.ToCharArray();
				arr[at] = with;
				return new String(arr);
		}


		public static String Stringify<T>(this T[] arr){
			return arr.Aggregate("", (acc, nxt)=>$"{acc}, {nxt.ToString()}");
		}

		/*
		 * constructs a new string by aligning argument 'other' with this.
		 * the alignment of two strings is a string of equal length to this,
		 * where each character that this and other have in common and in the same order appear
		 * in the index at which they appear in this, and all remaining indices are blank.
		 * ASSUMES:
		 *  -other is not longer than this
		 *  -this and other have at least one character in common
		 *  -if this and other have multiple characters in common they appear in the same order in other and this
		 *   (see test cases in TestAlign function below for examples)
		 * used mostly to construct the initial placeholder string (which must include blanks in place of missing letters)
		 * given input letters (e.g., "pk" when target is "peak"; full placeholder string must be "p  k").
		 * */
		public static String Align(this String str, string other){
			if(other == null || other.Length == 0) return _String.Fill(" ", str.Length);
			if(other.Length == str.Length) {
				if(other != str) throw new Exception(
					$"Align argument exception: {str} and {other} have equal length but have uncommon characters." +
						"\nPlease check inputs.");
				return other;
			}
			if(other.Length > str.Length) {
				throw new Exception(
					$"Align argument exception: length of {other} exceeds length of {str}." +
					"\nPlease check inputs.");
			}
			StringBuilder result = new StringBuilder();
			int inOther = 0;
			for(int i=0;i<str.Length;i++){
				if(inOther == other.Length) {
					result.Append(_String.Fill(" ", str.Length - i));
					return result.ToString();
				}
				char nxt = other[inOther];
				if(nxt == str[i]){
					result.Append(nxt);
					inOther++;
					continue;
				}
				result.Append(" "); 
			}
			return result.ToString();
		}

		public static void TestAlign(){
			Action<string, string, string> test = (string a, string b, string expect) => {
				string actual = a.Align(b);
				Debug.Log($"Given {a} and {b} expect {expect}: {actual} {expect==actual ? "passed" : "failed"}");
			};

			test("game", "gm", "g m ");
			test("lunch", "lun", "lun  ");
			test("stop", "op", "  op");
			test("peak", "pk", "p  k");
			test("car", "c", "c  ");

		}




	}
}
