using UnityEngine;
using System.Collections;
using System;

//container for the letter of this cell.
//stores data the image, the letter and the color
//but isn't responsible for using this data to do anything.
public class InteractiveLetter : MonoBehaviour{

		String inputLetter;
		Color colorDerivedFromInput; //the 
		public Color ColorDerivedFromInput {
				get {

						return colorDerivedFromInput;
				}
		}

		bool isSelected = false;

		public bool Selected {
				get {
						return isSelected;
				}
		}

		public delegate void SelectAction (bool wasSelected,GameObject o);

		public static event SelectAction LetterSelectedDeSelected;
		public delegate void PressAction (GameObject o);

		public static event PressAction LetterPressed;

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
		public void SetFlashColors(Color a, Color b){
			flashColors [0] = a;
			flashColors [1] = b;
		}
		float[] flashDurations = new float[2];
		public void SetFlashDurations(float a, float b){
			flashDurations [0] = a;
			flashDurations [1] = b;
		}
		int numFlashCycles;
		public void SetNumFlashCycles(int numFlashCycles){
			this.numFlashCycles = numFlashCycles;
		}


	public void ConfigureFlashParameters(
		Color a, Color b, float durationA, float durationB, int numCycles){
		SetFlashColors (a, b);
		SetFlashDurations (durationA, durationB);
		SetNumFlashCycles (numCycles);
	}

	public void ResetFlashParameters(){
		SetFlashColors (colorDerivedFromInput, colorDerivedFromInput);
		SetFlashDurations (0f, 0f);
		SetNumFlashCycles (0);
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


		public String InputLetter ()
		{
				return inputLetter;
		}
	
		public Texture2D CurrentDisplayImage ()
		{
				return (Texture2D)gameObject.GetComponent<UITexture> ().mainTexture;
		}

		public UnityEngine.Color CurrentDisplayColor ()
		{
				return gameObject.GetComponent<UITexture> ().color;
		}

		public bool IsBlank ()
		{
				return inputLetter [0] == ' ';
		}

	public void StartFlash(){
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
		UpdateDisplayColour (colorDerivedFromInput);
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
				UpdateDisplayColour (colorDerivedFromInput);

		}

	public void UpdateInputDerivedAndDisplayColor(Color c){
			UpdateDefaultColour (c);
			UpdateDisplayColour (c);
		}


		public void ChangeColourOfTangibleCounterpartIfThereIsOne (Color c)
		{
 
        //on the screen, blank letters are just clear.
        //but we issue the black (0,0,0) colour to the arduino.
		if (IsBlank())
						c = Color.black;
		if (IdxAsArduinoControlledLetter != NOT_AN_ARDUINO_CONTROLLED_LETTER &&
		      State.Current.InputMode == InputMode.TUI) {
			ArduinoUnityInterface.Instance.ColorNthTangibleLetter (IdxAsArduinoControlledLetter, c);
		}


		}
		public void UpdateInputLetterAndInputDerivedColor (String letter, Texture2D img, Color c)
		{
			UpdateInputLetterButNotInputDerivedColor (letter, img);
			UpdateInputDerivedAndDisplayColor (c);
		}


		
		public void UpdateDisplayImageButNotInputLetter (Texture2D img)
		{
				gameObject.GetComponent<UITexture> ().mainTexture = img;
		}

		public void UpdateInputLetterButNotInputDerivedColor (String letter, Texture2D img)
		{
			
				inputLetter = letter;

				//de-select this cell if it was selected
				if (isSelected)
						DeSelect ();
			
			
				gameObject.GetComponent<UITexture> ().mainTexture = img;
			

		}



		public void UpdateDefaultColour (Color c)
		{
			colorDerivedFromInput = c == Color.clear ? Color.white : c;

		}

		public void  SwitchImageTo (Texture2D img)
		{
				
		        
				GetComponent<UITexture> ().mainTexture = img;


		}

		void Update ()
		{
	
				if (!IsBlank ()) { //don't select blank letters
						
						if (MouseIsOverSelectable ()) {
					
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.RIGHT) {
												
										Select ();
								}
								if (SwipeDetector.swipeDirection == SwipeDetector.Swipe.LEFT) {
										DeSelect ();				
								}
					
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
				if (!isSelected && !IsBlank ()) {

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
								UpdateDisplayColour (colorDerivedFromInput);
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
