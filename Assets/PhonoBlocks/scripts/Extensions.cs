namespace Extensions{
using System;
using System.Text;
using System.Linq;

public static class PhonoBlocksExtensions {

		public static String Fill(this String str, char with, int times=-1){
			times = times == -1 ? str.Length : times;
			StringBuilder res = new StringBuilder();
			for(int i=0;i<times;i++){
				res.Append(""+with);
			}
			return res.ToString();
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
			buff.Append("".Fill(with, length));
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


	}
}
