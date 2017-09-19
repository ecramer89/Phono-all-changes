using UnityEngine;
using System.Collections;
using System;


public class InteractiveLetter : MonoBehaviour{
	    
		int position; //position of this letter in the on screen word.
		public int Position{
			get {
				return Position;
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

		public delegate void SelectAction (bool wasSelected,GameObject o);

		public event SelectAction LetterSelectedDeSelected;
		public delegate void PressAction (GameObject o);

		public event PressAction LetterPressed;

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
				return (Texture2D)gameObject.GetComponent<UITexture> ().mainTexture;
		}

		public UnityEngine.Color CurrentDisplayColor ()
		{
				return gameObject.GetComponent<UITexture> ().color;
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
		}
		
		public void UpdateDisplayColour (Color c)
		{

				if (c == Color.clear)
						c = Color.white;
		
				GetComponent<UITexture> ().color = c;
				//change colour of counterpart tangible letter
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
				gameObject.GetComponent<UITexture> ().mainTexture = img;
		}
		



		public void UpdateDefaultColour (Color c)
		{
			colorFromInput = c == Color.clear ? Color.white : c;

		}
		
		public void Update ()
	{

						if (MouseIsOverSelectable ()) {
					
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.RIGHT) {
										Debug.Log("SWIPE RIGHT");		
										Select ();
								}
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.LEFT) {
										Debug.Log("SWIPE LEFT");
										DeSelect ();				
								}
					
						}
			
				
		}
	
		bool MouseIsOverSelectable ()
		{        
				Vector3 mouse = SwipeDetector.GetTransformedMouseCoordinates ();
		        
				return (Vector3.Distance (mouse, gameObject.transform.position) < .3);	
		}

		public void Select (bool notifyObservers=true)
		{
				if (!isSelected) {

						isSelected = true;
						if (selectColor == Color.clear)
								selectHighlight.enabled = true;
						else
								UpdateDisplayColour (selectColor);
						if (notifyObservers && LetterSelectedDeSelected != null)
								LetterSelectedDeSelected (true, gameObject);
				}
		
		}

		public void DeSelect (bool notifyObservers=true)
		{
				if (isSelected) {
						isSelected = false;
						if (selectColor == Color.clear) {
								if (selectHighlight)
										selectHighlight.enabled = false;
						} else
								UpdateDisplayColour (colorFromInput);
						if (notifyObservers && LetterSelectedDeSelected != null)
								LetterSelectedDeSelected (false, gameObject);
				}
		}

		public void OnPress (bool pressed)
		{
				if (pressed && LetterPressed != null)
						LetterPressed (gameObject);


		}

}
