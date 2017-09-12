using UnityEngine;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * 
 * the main purpose of this class is to block user inputs when UI feedback not allowed (e.g., when in level 2 hint- showing the 
 * child all of the correct letters one at a time) and to decide which objects to delegate control to.
 * this class needs to be eliminated and replaced with global state/events.
 * 
 * */
public class UserInputRouter : MonoBehaviour
{
		static readonly string RESOURCES_WORD_IMAGE_PATH = "WordImages/";
		static readonly int DELAY_BEFORE_REGISTER_WHOLE_WORD_SELECTION = 30; //60/second
        static readonly int DELAY_BEFORE_REMOVE_HINT_LETTERS = 180; //60/second
        int hintLetterTimer = -1;
        public GameObject sessionParametersOB;
		bool screenMode;
		public static UserInputRouter instance;
		public static int totalLengthOfUserInputWord;
		public static int numOnscreenLetterSpaces = 6; 

		//the first slot in the physical prototype does not function.
		//it was easier to just let the system create that slot but fill it with a blank than to change everything else in the code
		//the problem class needs to know how many actual useable letter spaces there are for this to function... 
		//so we created this other method to distinguish between number of on screen spaxces and number of those spaces that are useable
		/*public static int NumOfActiveAndUseableArduinoControlledLetters(){
		return numOnscreenLetterSpaces - 1;

	}*/
		public GameObject studentActivityControllerGO;
		//public GameObject arduinoAndAffixLetterControllerGO;
		public GameObject arduinoLetterControllerGO;
		//public GameObject onscreenKeyboardControllerGO;
		public GameObject userStarControllerGO;
		public GameObject wordHistoryControllerGO;
		public GameObject arduinoLetterInterfaceG0;
		public GameObject uniduinoG0;
		public GameObject replayInstructionsButton;
		public GameObject checkedWordImageControllerGO;
		public GameObject hintButtonGO;
		public SessionsDirector sessionManager;
		public ArduinoLetterController arduinoLetterController;
		UserStarGridController userStarController;
		public WordHistoryController wordHistoryController;
		public ArduinoUnityInterface arduinoLetterInterface;
		public CheckedWordImageController checkedWordImageController;
		public StudentActivityController studentActivityController;


		bool currentWordViolatesPhonotactics = false; //either the arduino controlled letters or one of the affixes was placed incorrectly.
		//dont add to the history.
	    

		public AudioClip SAVED_DATA_CLIP;

	  
		// Use this for initialization
		void Start ()
		{
		      
				instance = this;
				sessionParametersOB = GameObject.Find ("SessionParameters");


             
				screenMode = sessionParametersOB.GetComponent<SessionsDirector> ().IsScreenMode ();

       

				if (screenMode) { 
						arduinoLetterInterfaceG0.SetActive (false);
						uniduinoG0.SetActive (false);


				} else { //tangible mode; activate the arduino unity interface and uniduino objects.
						arduinoLetterInterface = arduinoLetterInterfaceG0.GetComponent<ArduinoUnityInterface> ();
						arduinoLetterInterfaceG0.SetActive (true);
						arduinoLetterInterface.Initialize ();
						uniduinoG0.SetActive (true);
						uniduinoG0.GetComponent<Uniduino.Arduino> ().Connect ();


				}



				totalLengthOfUserInputWord = numOnscreenLetterSpaces;
	
				arduinoLetterController = arduinoLetterControllerGO.GetComponent<ArduinoLetterController> ();
				arduinoLetterController.Initialize (arduinoLetterInterface);
				wordHistoryController = wordHistoryControllerGO.GetComponent<WordHistoryController> ();
				wordHistoryController.Initialize (totalLengthOfUserInputWord);
			
				checkedWordImageController = checkedWordImageControllerGO.GetComponent<CheckedWordImageController> ();

				hintButtonGO.SetActive (false);
			



				if (sessionParametersOB != null) {
					
						sessionManager = sessionParametersOB.GetComponent<SessionsDirector> ();
						if (SessionsDirector.IsStudentMode) {
							      
								studentActivityControllerGO = sessionManager.studentActivityControllerOB;
								studentActivityController = studentActivityControllerGO.GetComponent<StudentActivityController> ();
				            
								studentActivityController.Initialize (hintButtonGO, arduinoLetterController);
								userStarControllerGO.SetActive (true);
								userStarController = userStarControllerGO.GetComponent<UserStarGridController> ();
								userStarController.Initialize ();
			
						} else {
								replayInstructionsButton.SetActive (false);
								userStarControllerGO.SetActive (false);
						}
		        
				}
			
		}

		public bool IsScreenMode ()
		{

				return screenMode;


		}

		public bool IsArduinoMode ()
		{

				return !screenMode;


		}

		public void RequestReplayInstruction ()
		{
				if (SessionsDirector.IsStudentMode)
						studentActivityController.PlayInstructions ();

		}

		public bool TeacherMode ()
		{
				return sessionParametersOB == null || SessionsDirector.IsTeacherMode;

		}

		public void RequestHint ()
		{//and if the UI is not on lockdown.
		if (!TeacherMode () && !State.Current.UIInputLocked)
						studentActivityController.UserRequestsHint ();


		}


		//touch the desk to deselect any selected selectables.
		public void DeskTouched ()
		{
				if (checkedWordImageController != null)
						checkedWordImageController.EndDisplay ();
	

		}

		public void RequestTurnOffImage ()
		{
				checkedWordImageController.EndDisplay ();
		}

		public void DisplayLettersOf (string word, bool ignoreBlanks=false)
		{
        //hintLetterTimer = DELAY_BEFORE_REMOVE_HINT_LETTERS;

			arduinoLetterController.DisplayWordInLetterGrid (word,ignoreBlanks);

		}

		//
		public void RequestDisplayImage (string word, bool disableTextureOnPress, bool indefinite=false)
		{
	
				StringBuilder path = new StringBuilder (RESOURCES_WORD_IMAGE_PATH);
				path.Append (word);
				Texture2D newimg = (Texture2D)Resources.Load (path.ToString (), typeof(Texture2D));
				if (!ReferenceEquals (newimg, null)) {
						checkedWordImageController.ShowImage (newimg, disableTextureOnPress, indefinite);
				}
		}

		
		//called by the check word button.
		public void RequestCheckWord ()
	{     
		if (!State.Current.UIInputLocked) {
				
						if (TeacherMode ()) {
								AddCurrentWordToHistory (true);
			} else {
				
							studentActivityController.HandleSubmittedAnswer ();
				}
		}
	}

		public void AddCurrentWordToHistory (bool playSoundsAndShowImage)
		{

		wordHistoryController.AddCurrentWordToHistory (State.Current.UILetters, playSoundsAndShowImage);

		}
		
		public void ClearWordHistory ()
		{
				wordHistoryController.ClearWordHistory ();


		}

		//show stars acquired during a session (but not yet stored in player prefs and not yet displayed initially) during the activity.
		public void DisplayNewStarOnScreen (int at)
		{
				userStarController.AddNewUserStar (true, at);
		}
	                                              

		/* if it is activity mode, then we delegate control of the new letter to the student activity controller. otherwise just update all of the letters*/
		public void HandleNewUserInputLetter (char newLetter, int atPosition, ArduinoLetterController alc)
	{       if (State.Current.UIInputLocked)
			return;

				if (sessionManager != null && SessionsDirector.IsStudentMode)
					studentActivityController.HandleNewArduinoLetter (newLetter, atPosition);


		}


		//called by the button that treats all words as selected and by the selection/deselection swipe action received by the arduinoletter controller.
		//
		int selectTimer = -1;

		void Update ()
		{
				if (selectTimer > 0)
						selectTimer--;
				if (selectTimer == 0) {
						if (!wholeWordWasSelected) {
								if (!SessionsDirector.IsStudentMode || studentActivityController.StringMatchesTarget (selectedLetters)) {
										if (AudioSourceController.PushClip (AudioSourceController.GetWordFromResources (selectedLetters))) {					
												if (!SessionsDirector.IsSyllableDivisionActivity)
														arduinoLetterController.ChangeDisplayColourOfCells (SessionsDirector.colourCodingScheme.GetColorsForWholeWord (), true);
												wholeWordWasSelected = true;
												
										}
				
								}

						} else {
								arduinoLetterController.RevertLettersToDefaultColour ();
								//and if there is a sounded out word, then play it
								//AudioSourceController.PushClip (AudioSourceController.GetSoundedOutWordFromResources (arduinoLetterController.CurrentUserControlledLettersAsString.Trim ()));
								wholeWordWasSelected = false;


						}
						selectTimer = -1;

				}

		}

		string selectedLetters = "";
		bool wholeWordWasSelected = false;

		public void HandleLetterSelection (string selectedLetters)
		{ 
				
				this.selectedLetters = selectedLetters.Trim ();
				selectTimer = DELAY_BEFORE_REGISTER_WHOLE_WORD_SELECTION;

		}

		public void RequestReturnToMainMenu ()
		{
				SessionsDirector.instance.ReturnToMainMenu ();

		}

		public void TellUserToPlaceInitialLetters ()
		{
				studentActivityController.PlayInstructions ();

		}

		

}
