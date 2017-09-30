using UnityEngine;
using System.Collections;
using System;


public class InteractiveLetter : MonoBehaviour{
	
		int position; //position of this letter in the on screen word.
		public int Position{
			get {
			 	return position;
			}
			set {
				position = value;
			}

		}
	    

		Color colorFromInput;  //color derived from the letterFromUserInput
		public Color ColorFromInput {
				get {

						return colorFromInput;
				}
		}

		bool isSelected = false;

		public bool Selected {
				get {
						return isSelected;
				}
		}

	   /*
	    * pattern for handling interactive letter UI events differs a little from others.
	    * basically, I don't want to even recognize swipes (selections) on the interactive letters in the word history panel
	    * conversely, I don't want to recognize press events on the interactive letters on the main letter grid.
	    * as such, I impose an additional layer between the interactive letter events and the main Event Transaction that
	    * runs all the rest of the application.
	    * the two classes (Arduino letter controller and word history controller) each subscribe directly to just the UI events they care about
	    * and only subscribe to the instances of InteractiveLetter that they manage. 
	    * these two controller classes are then responsible for dispatching the event (InteractiveLetterSelected/Deselected or just playing the word sound in case of W.H.controller)
	    * to the main event dispatcher. The ALC can also decide whether to dispatch this event according to the current state of the letter, i.e., whether it's blank.
	    * and of the app (i.e., ignore if all UI inputs blocked).
	    * the ALC is also responsible for deciding whether to respond to the swipe/select action by changing the letter's highlights.
	    * */

	    public event Action<bool, InteractiveLetter> OnInteractiveLetterSelectToggled = (bool selected, InteractiveLetter letter)=>{};

		public delegate void PressAction (GameObject o);

		public event PressAction LetterPressed;

	    public UITexture texture; //cache reference to UITexture component
		UITexture selectHighlight;
		Color selectColor = Color.clear;

		public void SetSelectColour (Color newColor)
		{
				selectColor = newColor;


		}

		public Color  SelectColour {
				get {
						return selectColor;
				}
		
		
		}

		BoxCollider trigger;

		int flashCounter = 0;
		Color[] flashColors = new Color[2];

		float[] flashDurations = new float[2];

		int numFlashCycles;


	public void ConfigureFlashParameters(
		Color a, Color b, float durationA, float durationB, int numCycles){
		flashColors [0] = a;
		flashColors [1] = b;

		flashDurations [0] = durationA;
		flashDurations [1] = durationB;

		this.numFlashCycles = numCycles;
	}



	public void ResetFlashParameters(){
		flashColors [0] = colorFromInput;
		flashColors [1] = colorFromInput;
		flashDurations [0] = 0f;
		flashDurations [1] = 0f;
		this.numFlashCycles = 0;
	}


		const int NOT_AN_ARDUINO_CONTROLLED_LETTER = -1;
		int idxAsArduinoControlledLetter = NOT_AN_ARDUINO_CONTROLLED_LETTER; //i.e., if it's a word history controlled letter. you have to "opt in" to be an arduino controlled letter.

		public int IdxAsArduinoControlledLetter {
				set {
						idxAsArduinoControlledLetter = value;
					

				}
				get {
						return idxAsArduinoControlledLetter;
				}
		}


		public BoxCollider Trigger {
				get {
						return trigger;
				}
				set {

						trigger = value;
				}


		}

		public UITexture SelectHighlight {
				get {
						return selectHighlight;
				}
				set {
						selectHighlight = value;
						selectHighlight.enabled = false;

				}


		}

	
		public Texture2D CurrentDisplayImage ()
		{
				return (Texture2D)texture.mainTexture;
		}

		public UnityEngine.Color CurrentDisplayColor ()
		{
				return texture.color;
		}


		public void StartFlash(){
			if (numFlashCycles == 0)
				return;

			IEnumerator coroutine = Flash();
			StartCoroutine (coroutine);
		}

		private IEnumerator Flash(){
			int timesToFlash = numFlashCycles * 2;//i.e. times to appear in the flash color. since switching back to default color requires another
			//invocation of the couroutine, must iterate twice as many times as requested times to flash
			float durationOfFlash;
			Color a = flashColors [0];
			Color b = flashColors [1];
			float durationA = flashDurations [0];
			float durationB = flashDurations [1];
			while (flashCounter<timesToFlash) {

				if (flashCounter % 2 == 0) {
					UpdateDisplayColour (a);
					durationOfFlash = durationA;
				} else {
					UpdateDisplayColour (b);
					durationOfFlash = durationB;
				}
				flashCounter++;

				yield return new WaitForSeconds (durationOfFlash);
			}
			//restore default color
			UpdateDisplayColour (colorFromInput);
			flashCounter = 0;
			ResetFlashParameters();
		}
		
		public void UpdateDisplayColour (Color c)
		{   
			
				if (c == Color.clear)
						c = Color.white;
		
				texture.color = c;
	
				ChangeColourOfTangibleCounterpartIfThereIsOne (c);
			
		}

		public void RevertToInputDerivedColor ()
		{
				UpdateDisplayColour (colorFromInput);

		}

	public void UpdateInputDerivedAndDisplayColor(Color c){
			UpdateDefaultColour (c);
			UpdateDisplayColour (c);
		}

	//todo, this should be responsibility of the colorer, not of the i letters sine i letters used to grids also
		public void ChangeColourOfTangibleCounterpartIfThereIsOne (Color c)
		{
 
        //on the screen, blank letters are just clear.
        //but we issue the black (0,0,0) colour to the arduino
		/*if (IdxAsArduinoControlledLetter != NOT_AN_ARDUINO_CONTROLLED_LETTER &&
		      State.Current.InputType == InputType.TUI) {
			ArduinoUnityInterface.Instance.ColorNthTangibleLetter (IdxAsArduinoControlledLetter, c);
		}*/


		}
		public void UpdateLetterImageAndInputDerivedColor (Texture2D img, Color c)
	{   	
			UpdateLetterImage (img);
			UpdateInputDerivedAndDisplayColor (c);
		}



		public void UpdateLetterImage (Texture2D img)
	{

				texture.mainTexture = img;

		}
		



		public void UpdateDefaultColour (Color c)
		{
			colorFromInput = c == Color.clear ? Color.white : c;

		}
		



		
		public void Update ()
	{

						if (MouseIsOverSelectable ()) {
					
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.RIGHT) {
										OnInteractiveLetterSelectToggled(true,this);
								}
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.LEFT) {

									OnInteractiveLetterSelectToggled(false, this);				
								}
					
						}
			
				
		}
	
		bool MouseIsOverSelectable ()
		{        
				Vector3 mouse = SwipeDetector.GetTransformedMouseCoordinates ();
		        
				return (Vector3.Distance (mouse, gameObject.transform.position) < .3);	
		}

		public void ToggleSelectHighlight (bool activate)
		{
				selectHighlight.enabled = activate;
		
		}
		

		public void OnPress (bool pressed)
		{
				if (pressed && LetterPressed != null)
						LetterPressed (gameObject);


		}

}
