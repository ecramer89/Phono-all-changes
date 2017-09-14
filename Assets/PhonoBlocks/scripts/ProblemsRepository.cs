using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;


public class ProblemsRepository : MonoBehaviour
{   
		Problem[] problemsForSession; 
		int currProblem = 0;

		public int ProblemsCompleted {
				get{ return currProblem;}


		}
		

		public static ProblemsRepository instance = new ProblemsRepository ();


		public void Initialize (int sessionIndex)
		{
			problemsForSession = new Problem[Parameters.StudentMode.PROBLEMS_PER_SESSION];

			for (int problemNum=0; problemNum<problemsForSession.Length; problemNum++) {
				problemsForSession [problemNum] = new Problem (
					Parameters.StudentMode.PlaceholderLettersFor(sessionIndex, problemNum), 
					Parameters.StudentMode.TargetWordFor(sessionIndex, problemNum)
				);
			}
	
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
