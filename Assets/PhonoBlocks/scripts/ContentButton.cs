using UnityEngine;
using System.Collections;

/* We will make this into a class that allows teachers to pick the prolbem type, versus the session*/
public class ContentButton : MonoBehaviour
{

		public GameObject sessionsDirectorOB;
		SessionsDirector sessionsDirector;
		public ProblemsRepository.ProblemType problemType;

		void Start ()
		{
				sessionsDirector = sessionsDirectorOB.GetComponent<SessionsDirector> ();

		}

		void OnPress (bool pressed)
		{
            
				if (pressed) {
					
						sessionsDirector.SetContentForTeacherMode (problemType);
				}
			



		}




		



}
