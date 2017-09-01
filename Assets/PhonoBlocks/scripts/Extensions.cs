namespace Extensions{
using System;
using System.Text;

public static class PhonoBlocksExtensions {

	public static String Fill(this String str, string with, int times=-1){
		times = times == -1 ? str.Length : times;
		StringBuilder res = new StringBuilder();
		for(int i=0;i<times;i++){
			res.Append(with);
		}
		return res.ToString();
	}


	public static String ReplaceAt(this String str, int at, char with){
			if(at < 0 || at > str.Length) throw new Exception($"{at} is out of bounds of {str}");
			char[] arr = str.ToCharArray();
			arr[at] = with;
			return new String(arr);
	}
}
}
