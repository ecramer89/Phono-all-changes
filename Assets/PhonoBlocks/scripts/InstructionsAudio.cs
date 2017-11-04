using UnityEngine;
using System.Collections;

/*
 * singleton pattern. use the global instance to access the public audio clips.
 * 
 * */
public class InstructionsAudio : MonoBehaviour
{
		public AudioClip yourWordIsntQuiteRightYet;
		public AudioClip makeTheWord;
		public AudioClip soundOutTheWord;
		public AudioClip excellent;
		public AudioClip incorrectSoundEffect;
		public AudioClip notQuiteIt;
		public AudioClip offerHint;
		public AudioClip youDidIt;
		public AudioClip correctSoundEffect;
		public AudioClip removeAllLetters;
		public AudioClip allDoneSession;
		public AudioClip trySoundingOutWordYourself;
		public AudioClip tryReadingWholeWordYourself;


		public static InstructionsAudio instance;


		void Awake ()
		{
				instance = this;
		}















}
