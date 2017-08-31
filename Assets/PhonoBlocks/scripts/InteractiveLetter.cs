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
		Color defaultColour;

		public Color DefaultColour {
				get {

						return defaultColour;
				}
		}

		bool isLocked = false;

		public bool IsLocked {
				get {
						return isLocked;
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
		const int defaultTimesToFlash = 1;

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

	private IEnumerator Flash(Color flashColor, int timesToFlash = defaultTimesToFlash){
		timesToFlash *= 2;//i.e. times to appear in the flash color. since switching back to default color requires another
		//invocation of the couroutine, must iterate twice as many times as requested times to flash
		int default_color_mod = ((timesToFlash-1) % 2); //ensure that we always end on the new default color.
		
		while (flashCounter<timesToFlash) {
			bool showingDefaultColor = flashCounter % 2 == default_color_mod;
				if (showingDefaultColor) {
				UpdateDisplayColour (defaultColour);
			} else {
				UpdateDisplayColour (flashColor);
			}
			flashCounter++;
			
			yield return new WaitForSeconds (showingDefaultColor ? 
				TimingParameters.FLASH_DURATION_OF_DEFAULT_COLOR : 
				TimingParameters.FLASH_DURATION_OF_FLASH_COLOR);
		}
		
		flashCounter = 0;

		}

	//todo use this version after refactoring how the color cues work
	private IEnumerator Flash(Color a, Color b, float durationA, float durationB, int numCycles){
		int timesToFlash = numCycles * 2;//i.e. times to appear in the flash color. since switching back to default color requires another
		//invocation of the couroutine, must iterate twice as many times as requested times to flash
		float durationOfFlash;

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
		UpdateDisplayColour (defaultColour);
		flashCounter = 0;
	}

		public void StartFlash (Color flashOff, int timesToFlash = defaultTimesToFlash)
		{      
			IEnumerator coroutine = Flash (flashOff, timesToFlash);
			StartCoroutine (coroutine);
			
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
				UpdateDisplayColour (defaultColour);

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
				if (defaultColour == Color.clear)
						defaultColour = Color.white;
				defaultColour = c_;

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
								UpdateDisplayColour (defaultColour);
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
