using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;

//Problem Manager-- not the student data manager-- should have the information about what colour coding scheme to use (since now that is associated with the problem type)
public class ProblemsRepository : MonoBehaviour
{   
		Problem[] problemsForSession; //I think if we do go this way, we should change NGram class so that problems aere associated directly with them
		int currProblem = 0;

		public int ProblemsCompleted {
				get{ return currProblem;}


		}

		public readonly int PROBLEMS_PER_SESSION = 3;


		public static ProblemsRepository instance = new ProblemsRepository ();


		static readonly int INITIAL_WORD_IDX = 1;
		static readonly int TARGET_WORD_IDX = 0;
		static string[][][] activity_word_sets = {


		new string[][]{
			new string[]{"bet","dad","tin"}, //target words
			new string[]{"b t","d d","t n"} //initial versions of target words
		},


		new string[][]{
			new string[]{"pup","hit","web"},
			new string[]{"p p","h t","w b"}
		},
		
		new string[][]{
			new string[]{"flag","skin","stop"},
			new string[]{"ag","in","op"}
		},
		
		new string[][]{
			new string[]{"trip","drop","crab"},
			new string[]{"ip","op","ab"}
		},

		new string[][]{
			new string[]{"thin","shop","chip"},
			new string[]{"in","op","ip"}
		},


		new string[][]{
			new string[]{"path","wish","lunch",},
			new string[]{"pa","wi","lun"}
		},

		new string[][]{
			new string[]{"game","tape","cake"},
			new string[]{"g m","t p","c k"}
		},

		new string[][]{
			new string[]{"side","wide","late"},
			new string[]{"s d","w d","l t"}
		},

		new string[][]{
			new string[]{"eat","boat","paid"},
			new string[]{"t","b  t","p  d"}
		},


		new string[][]{
			new string[]{"seat","coat","bait"},
			new string[]{"s  t","c  t","b  t"}
		}, 

		new string[][]{
			new string[]{"car","jar","fir"},
			new string[]{"c","j","f"}
		},
		
		new string[][]{
			new string[]{"hurt","horn","part"},
			new string[]{"h  t","h  n","p  t"}
		}


	};
		static int numSessions = activity_word_sets.Length;

		public int NumSessions {
				get {
						return numSessions;
				}
		}


		public void Initialize (int sessionIndex)
		{
			string[][] wordsForSessionProblems = activity_word_sets [sessionIndex % activity_word_sets.Length];
			problemsForSession = new Problem[PROBLEMS_PER_SESSION];


			for (int i=0; i<PROBLEMS_PER_SESSION; i++) {

				problemsForSession [i] = new Problem (wordsForSessionProblems [INITIAL_WORD_IDX] [i], wordsForSessionProblems [TARGET_WORD_IDX] [i]);
			}
	
		}


	public Activity ActivityForSession(int session){
		switch (session) {
		case 0:
		case 1:
			return Activity.OPEN_CLOSED_SYLLABLE;
		
		case 2:
		case 3:
			return Activity.CONSONANT_BLENDS;

		case 4:
		case 5:
			return Activity.CONSONANT_DIGRAPHS;
		

		case 6:
		case 7:
			return Activity.MAGIC_E;
		
		case 8:
		case 9:
			return Activity.VOWEL_DIGRAPHS;
		case 10:
		case 11:
			
			return Activity.R_CONTROLLED_VOWELS;

		}

		throw new Exception ($"Invalid session: {session}");

	}
		

		public Problem GetNextProblem ()
		{       
				if (!AllProblemsDone ()) {
						Problem result = problemsForSession [currProblem];
						currProblem++;
						return result;
				} else
						return problemsForSession [0];
		}

		public bool AllProblemsDone ()
		{
				return currProblem == problemsForSession.Length;

		}



		


}
