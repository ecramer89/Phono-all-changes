using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;


public class ProblemsRepository : MonoBehaviour
{       static ProblemsRepository instance;
		public static ProblemsRepository Instance{
			get {
				return instance;
			}

		}
		ProblemData[] problemsForSession; 
		int currProblem = 0;

		public int ProblemsCompleted {
				get{ return currProblem;}


		}
		
		void Start(){
			instance = this;

		}


		public void Initialize (int sessionIndex)
		{
			problemsForSession = new ProblemData[Parameters.StudentMode.PROBLEMS_PER_SESSION];

			for (int problemNum=0; problemNum<problemsForSession.Length; problemNum++) {
				problemsForSession [problemNum] = Parameters.StudentMode.GetProblem (sessionIndex, problemNum);
			}
	
		}
		

		public ProblemData GetNextProblem ()
		{       
				if (!AllProblemsDone ()) {
						ProblemData result = problemsForSession [currProblem];
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
