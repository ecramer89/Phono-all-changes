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
