using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
/*
 * session manager needs to instantiate and set up the variables of the SessionParameters component if the mode is student mode
 * 
 * */
public class SessionsDirector : MonoBehaviour
		
{
		[SerializeField] InputType inputType;
		public static SessionsDirector instance;
		


		public GameObject studentActivityControllerOB;
		public GameObject activitySelectionButtons;
		public GameObject sessionSelectionButtons;
		public GameObject teacherModeButton;
		public GameObject studentModeButton;
		public GameObject studentNameInputField;
		public GameObject dataTables;
		NameInputField studentName;
		public AudioClip noDataForStudentName;
		public AudioClip enterAgainToCreateNewFile;
		public static DateTime assessmentStartTime;
		

		void Start ()
		{     
		
				instance = this;
			
				studentName = studentNameInputField.GetComponent<NameInputField> ();
				SetupModeSelectionMenu ();
				Events.Dispatcher.RecordInputTypeSelected (inputType);
				Events.Dispatcher.OnActivitySelected += (Activity obj) => {
						Application.LoadLevel ("Activity");
				};
	}

		void SetupModeSelectionMenu ()
		{
				
				assessmentStartTime = DateTime.Now;
				//activitySelectionButtons.SetActive (false);
				//sessionSelectionButtons.SetActive (false);
				studentModeButton.SetActive (true);
				teacherModeButton.SetActive (true);
				//studentNameInputField.SetActive (false);

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

				Events.Dispatcher.RecordModeSelected (Mode.TEACHER);
		       
				activitySelectionButtons.SetActive (true);
				studentModeButton.SetActive (false);
				teacherModeButton.SetActive (false);
				studentNameInputField.SetActive (false);
				

		}


		public void LoadSessionSelectionScreen ()
		{
			sessionSelectionButtons.SetActive (true);
			studentModeButton.SetActive (false);
			teacherModeButton.SetActive (false);
			studentNameInputField.SetActive (false);
		}


		/*public void SelectStudentMode ()
	{       	


				if (studentNameInputField.activeSelf) {
						string nameEntered = studentName.Name.Trim ().ToLower ();
						if (nameEntered.Length > 0) {
		
								nameEntered = CreateNewFileIfNeeded (nameEntered);


								bool wasStoredDataForName = StudentsDataHandler.instance.LoadStudentData (nameEntered);
			
								if (wasStoredDataForName) {
										Events.Dispatcher.RecordModeSelected (Mode.STUDENT);
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
		}*/




}