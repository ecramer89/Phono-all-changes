using UnityEngine;
using System.Collections;

public class SessionButton : MonoBehaviour {
	public int session_num;
	public GameObject sessionsDirectorOB;
	SessionsDirector sessionsDirector;


	
	void Start ()
	{
		sessionsDirector = sessionsDirectorOB.GetComponent<SessionsDirector> ();


		
	}
	
	void OnPress (bool pressed)
	{
		
		if (pressed) {
		
			sessionsDirector.SetSessionForPracticeMode(session_num);
		}
		
		
		
		
	}
}
