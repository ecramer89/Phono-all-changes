using UnityEngine;
using System.Collections;
using System;

//container for the letter of this cell.
//stores data the image, the letter and the color
//but isn't responsible for using this data to do anything.
public class InteractiveLetter : MonoBehaviour
{
    //changed file!
		String letter;
		Color colorDerivedFromUserInput; //the 
		public Color DefaultColour {
				get {

						return colorDerivedFromUserInput;
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
		LetterSoundComponent lc;
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

		public LetterSoundComponent LetterSoundComponentIsPartOf {
				get {
						return lc;
				}

				set {
						lc = value;
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


		public String Letter ()
		{
				return letter;
		}
	
		public Texture2D Image ()
		{
				return (Texture2D)gameObject.GetComponent<UITexture> ().mainTexture;
		}

		public UnityEngine.Color CurrentColor ()
		{
				return gameObject.GetComponent<UITexture> ().color;
		}

		public bool IsBlank ()
		{
				return letter [0] == ' ';
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
		UpdateDisplayColour (colorDerivedFromUserInput);
		flashCounter = 0;
	}
		
		public void UpdateDisplayColour (UnityEngine.Color c_)
		{


				if (c_ == Color.clear)
						c_ = Color.white;
		
				GetComponent<UITexture> ().color = c_;
				//change colour of counterpart tangible letter
				ChangeColourOfTangibleCounterpartIfThereIsOne (c_);
			
		}

		public void RevertToDefaultColour ()
		{
				UpdateDisplayColour (colorDerivedFromUserInput);

		}

		public void UpdateDefaultColorAndDisplay(UnityEngine.Color c){
			UpdateDefaultColour (c);
			UpdateDisplayColour (c);
		}


		public void ChangeColourOfTangibleCounterpartIfThereIsOne (UnityEngine.Color c_)
		{
 
        //on the screen, blank letters are just clear.
        //but we issue the black (0,0,0) colour to the arduino.
        if (letter [0] == ' ')
						c_ = Color.black;
       

				if (UserInputRouter.instance != null)
				if (IdxAsArduinoControlledLetter != NOT_AN_ARDUINO_CONTROLLED_LETTER && UserInputRouter.instance.IsArduinoMode ()) 
						UserInputRouter.instance.arduinoLetterInterface.ColorNthTangibleLetter (IdxAsArduinoControlledLetter, c_);


		}

		public void UpdateLetter (String letter_, Texture2D img_, UnityEngine.Color c_)
		{
				UpdateLetter (letter_, img_);
				UpdateDefaultColour (c_);

			
		}


		//update the letter images; then after they make any change it will just update them all again
		public void UpdateLetterImage (Texture2D img_)
		{
				gameObject.GetComponent<UITexture> ().mainTexture = img_;


		}

		public void UpdateLetter (String letter_, Texture2D img_)
		{
			
				letter = letter_;

				//de-select this cell if it was selected
				if (isSelected)
						DeSelect ();
			
			
				gameObject.GetComponent<UITexture> ().mainTexture = img_;
			

		}



		public void UpdateDefaultColour (UnityEngine.Color c_)
		{
				if (colorDerivedFromUserInput == Color.clear)
						colorDerivedFromUserInput = Color.white;
				colorDerivedFromUserInput = c_;

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
								UpdateDisplayColour (colorDerivedFromUserInput);
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
