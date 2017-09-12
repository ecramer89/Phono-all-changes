using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class Problem : MonoBehaviour
{

		public const int TO_MAKE_THE_WORD = 0;
		public const int TARGET_WORD = 1;
		protected AudioClip[] instructions;
		public AudioClip[] Instructions{
			get {
				return instructions;
			}

		}
		protected string initialWord;
		protected AudioClip sounded_out_word;

		protected static string emptyWord = "";
		protected int timesAttempted;

		public int TimesAttempted {
				get {
						return timesAttempted;
				}
		}

		public void IncrementTimesAttempted ()
		{
				timesAttempted++;


		}


		public string InitialWord {
				get {
						return initialWord;
				}
				set {
						initialWord = value;
			
				}


		}

		protected string targetWord;
		protected readonly string cachedTargetWord;

		public string TargetWord (bool skipBlanks)
		{
				if (skipBlanks)
						return cachedTrimmedTargetWord;
				return targetWord;

		}

		protected readonly string cachedTrimmedTargetWord;



		public Problem (string initialWord, string targetWord)
		{
				initialWord = Clean (initialWord); 
				targetWord = Clean (targetWord);


				CacheFixedInstructions (initialWord, targetWord);

				cachedTrimmedTargetWord = targetWord;
				this.targetWord = AppendBlanksToEnd (targetWord, UserInputRouter.numOnscreenLetterSpaces);
				this.initialWord = AppendBlanksToFrontOrEnd (initialWord, this.targetWord);
		
				cachedTargetWord = this.targetWord;
		
		}

		string Clean (string word)
		{
				return word.Trim ().ToLower ();
		}

		static void CacheEmptyWord (int numArduinoControlledLetters)
		{      
				StringBuilder s = new StringBuilder ();
				for (int i=0; i<numArduinoControlledLetters; i++) {
						s.Append (' ');

				}

				emptyWord = s.ToString ();
		}


		

		protected virtual void CacheFixedInstructions (string initialWord, string targetWord)
		{       
				
				instructions = new AudioClip[2];
				instructions [TO_MAKE_THE_WORD] = InstructionsAudio.instance.makeTheWord;
				instructions [TARGET_WORD] = AudioSourceController.GetWordFromResources (targetWord);
		        
				sounded_out_word = AudioSourceController.GetSoundedOutWordFromResources (targetWord);
		}
		

		/* return a new string that is identical to targetWord except that apppended to the end are numArduinoControlledLetters-targetWord.length blanks*/
		protected string AppendBlanksToEnd (string targetWord, int numArduinoControlledLetters)
		{
				StringBuilder s = new StringBuilder (targetWord);

				for (int i=targetWord.Length; i<numArduinoControlledLetters; i++)
						s.Append (' ');

				string afterAppendBlank = s.ToString ();
	
				return afterAppendBlank;


		}

		public void PlayCurrentInstruction ()
		{
			
				for (int i=TO_MAKE_THE_WORD; i<TARGET_WORD+1; i++) {
							
						if (instructions [i] != null)
								AudioSourceController.PushClip (instructions [i]);
				}

				
		}

		public void PlayTargetWord ()
		{

				AudioSourceController.PushClip (instructions [TARGET_WORD]);
		}

		public void PlaySoundedOutWord ()
		{

				AudioSourceController.PushClip (sounded_out_word);

		}

		public void PlayAnswer ()
		{
				PlayTargetWord ();
			


		}
		
		protected string AppendBlanksToFrontOrEnd (string initialWord, string targetWord)
		{
			
				StringBuilder s = new StringBuilder (initialWord);

				int numBlanksToAppendToFront = FindDifferenceInIndexesOfFirstMatchingLetter (initialWord, targetWord);
				for (int i=0; i<numBlanksToAppendToFront; i++) {
						s.Insert (0, ' ');
		
				}
				int numBlanksToAppendToEnd = targetWord.Length - s.Length;
				for (int i=0; i<numBlanksToAppendToEnd; i++)
						s.Append (' ');

				string afterAppendBlank = s.ToString ();
	
				return afterAppendBlank;


		}

		protected int FindDifferenceInIndexesOfFirstMatchingLetter (string initialWord, string targetWord)
		{
				//D=idx 1 in itial; D idx 2 in target. if we were to ADD ONE MORE CHATR the indexes would match. so add that many blanks
				for (int i=0; i<initialWord.Length; i++) {
						char a = initialWord [i];
						for (int j=0; j<targetWord.Length; j++) {
								char b = targetWord [j];
								if (a == b) 
										return j - i;
					 		
						}
				}
				return targetWord.Length - initialWord.Length;
		}

	   
}

