using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardLetterInput : MonoBehaviour {

	//all of this is for testing; simulates arduino functionality.
	static int testPosition = -1;

	public void SetTestPosition (int newPosition)
	{
		testPosition = newPosition;

		UpdateLetterBarIfPositionAndLetterSpecified ();
	}

	static string testLetter;

	public void SetTestLetter (string newLetter)
	{
		testLetter = newLetter;
		UpdateLetterBarIfPositionAndLetterSpecified ();

	}

	public void ClearTestLetter ()
	{
		testLetter = null;


	}

	public void ClearTestPosition ()
	{

		testPosition = -1;

	}


	void Update ()
	{
		if (Input.anyKeyDown) {
			if (Input.GetMouseButton (0) || Input.GetMouseButton (1) || Input.GetMouseButton (2))
				return;

			if (ParseNumericKey () || ParseLetterKey ())
				UpdateLetterBarIfPositionAndLetterSpecified ();




		}


	}

	bool ParseNumericKey ()
	{
		string s;

		for (int i=0; i<Parameters.UI.ONSCREEN_LETTER_SPACES; i++) {
			s = "" + i;
			if (Input.GetKey (s)) {
				//testPosition = i;
				SetTestPosition (i);
				return true;
			}
		}
		return false;
	}



	bool ParseLetterKey ()
	{       

		//deleting a character
		if (Input.GetKeyDown (KeyCode.Backspace)) {
			//testLetter = " ";
			SetTestLetter (" ");
			return true;
		}


		for (int i = (int)'a'; i <= (int)'z'; i++) {
			string s =""+(char)i;
			if (Input.GetKeyDown (s)) {
				SetTestLetter (s);
				return true;
			}

		}
		return false;

	}

	void UpdateLetterBarIfPositionAndLetterSpecified ()
	{
		if (testPosition != -1 && testLetter != null) {
			
			Dispatcher.Instance.RecordNewUserInputLetter(testLetter [0], testPosition);
			ClearTestPosition ();

		}
	}
}
