using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/*
 * session manager needs to instantiate and set up the variables of the SessionParameters component if the mode is student mode
 * 
 * */
public class SessionsDirector : MonoBehaviour
		//change this between students
{

		public static SessionsDirector instance;
		public static ColourCodingScheme colourCodingScheme = new RControlledVowel ();

		public bool IsMagicERule {
				get {
						return colourCodingScheme.label.Equals ("vowelInfluenceE");
				}
		}


	public ColourCodingScheme CurrentActivityColorRules{
		get {
						return colourCodingScheme;
				}
	}

	public string GetCurrentRule{
		get {

			return colourCodingScheme.label;
				}

	}

		public static bool IsSyllableDivisionActivity {
				get {
						return colourCodingScheme.label.Equals ("syllableDivision");

				}

		}

	public bool IsConsonantBlends {
		get {
			return colourCodingScheme.label.Equals ("Blends");
				}
	}

		public INTERFACE_TYPE INTERFACE;

 

		public enum INTERFACE_TYPE
		{
				TANGIBLE, 
				SCREEN_KEYBOARD
    }
		;

		public bool IsScreenMode ()
		{
				return INTERFACE == INTERFACE_TYPE.SCREEN_KEYBOARD;

		}

		public static int currentUserSession; //will obtain from player prefs
		public static int numStarsOfCurrentUser; //will obtain from player prefs
	
	
		public static bool IsTheFirstTutorLedSession ()
		{

				return  currentUserSession == 0;

		}

		static Mode mode; //testing mode. can be student driven (usual phonoblocks practice session, phono reads words), test (assessment) or sandbox
	
			
		/* also more like a "sandbox" mode; teacher can create whatever words they want */
		public static bool IsTeacherMode {
				get {
						return mode == Mode.TEACHER;
				}
		
		
		}

		public static bool IsStudentMode {
				get {
						return mode == Mode.STUDENT;
				}
		
		
		}

		public GameObject studentActivityControllerOB;
		public GameObject activitySelectionButtons;
		public GameObject sessionSelectionButtons;
		public GameObject teacherModeButton;
		public GameObject studentModeButton;
		public GameObject studentNameInputField;
		public GameObject dataTables;
		InputField studentName;
		public AudioClip noDataForStudentName;
		public AudioClip enterAgainToCreateNewFile;
		public static DateTime assessmentStartTime;
		



		public enum Mode
		{
				TEACHER,
				STUDENT
			
		}



		void Start ()
		{     
				instance = this;
				SpeechSoundReference.Initialize ();
				studentName = studentNameInputField.GetComponent<InputField> ();
				SetupModeSelectionMenu ();
				LetterSoundComponentRegex.Test ();

		}

		void SetupModeSelectionMenu ()
		{
				
				assessmentStartTime = DateTime.Now;
				activitySelectionButtons.SetActive (false);
				sessionSelectionButtons.SetActive (false);
				studentModeButton.SetActive (true);
				teacherModeButton.SetActive (true);
				studentNameInputField.SetActive (false);

		}

		public void ReturnToMainMenu ()
		{
				
				if (!Application.loadedLevelName.Equals ("MainMenu"))
						Application.LoadLevel ("MainMenu");
				SetupModeSelectionMenu ();

		}
				
		//Teacher mode is the current "sandbox" mode, which just defaults to rthe colour scheme chosen at the head of this file.
		public void SelectTeacherMode ()
		{
				mode = Mode.TEACHER;
				activitySelectionButtons.SetActive (true);
				studentModeButton.SetActive (false);
				teacherModeButton.SetActive (false);
				studentNameInputField.SetActive (false);
				

		}

	 //1. 
		public void SetContentForTeacherMode (ProblemsRepository.ProblemType problemType)
		{

				colourCodingScheme = ProblemsRepository.instance.GetColourCodingSchemeGivenProblemType (problemType);
			

				Application.LoadLevel ("Activity");
		}

		public void SetSessionForPracticeMode (int session)
		{

				currentUserSession = session;
				SetParametersForStudentMode (studentActivityControllerOB);
				Application.LoadLevel ("Activity");

		}

		public void LoadSessionSelectionScreen ()
		{
			sessionSelectionButtons.SetActive (true);
			studentModeButton.SetActive (false);
			teacherModeButton.SetActive (false);
			studentNameInputField.SetActive (false);
		}


		public void SelectStudentMode ()
		{
				if (studentNameInputField.activeSelf) {
						string nameEntered = studentName.stringToEdit.Trim ().ToLower ();
						if (nameEntered.Length > 0) {
		
								nameEntered = CreateNewFileIfNeeded (nameEntered);


								bool wasStoredDataForName = StudentsDataHandler.instance.LoadStudentData (nameEntered);

		
								if (wasStoredDataForName) {
										mode = Mode.STUDENT;
										studentActivityControllerOB = (GameObject)GameObject.Instantiate (studentActivityControllerOB);
			
										
										UnityEngine.Object.DontDestroyOnLoad (studentActivityControllerOB);
									
										LoadSessionSelectionScreen ();
								
								} else {
										AudioSourceController.PushClip (noDataForStudentName);
			
								}
						}
				} else
						studentNameInputField.SetActive (true);
		
		}

		string CreateNewFileIfNeeded (string nameEntered)
		{     
				bool createNewFile = nameEntered [nameEntered.Length - 1] == '*'; //mark new file with asterik
				
				if (createNewFile) {
				
						
						nameEntered = nameEntered.Substring (0, nameEntered.Length - 1);
						
						
						StudentsDataHandler.instance.CreateNewStudentFile (nameEntered);
		
		
		
				}

				return nameEntered;
		}

		public void SetParametersForStudentMode (GameObject studentActivityController)
		{
			StudentsDataHandler.instance.UpdateUsersSession (currentUserSession);

			numStarsOfCurrentUser = StudentsDataHandler.instance.GetUsersNumStars ();
			                                
			ProblemsRepository.instance.Initialize (currentUserSession);
			
			colourCodingScheme = ProblemsRepository.instance.ActiveColourScheme;
			
			StudentActivityController sc = studentActivityControllerOB.GetComponent<StudentActivityController> ();

		}




}