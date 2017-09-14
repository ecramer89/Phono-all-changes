using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Uniduino;
using System.Text;

//receives inputs from the arduino.
//converts them into letters.
//sends the letter data to the arduinoLettersController.
//for now it will just print an array of letters.

//..receives new color data back from the controller and uses it to update the arduino letters.
//(implement this later)
public class ArduinoUnityInterface : MonoBehaviour
{
		private static ArduinoUnityInterface instance;
		public static ArduinoUnityInterface Instance{
			get {

				return instance;
			}
		}
		int CURR_MAX_NUM_PIN = 2;
		public float minimumSecondsBeforeNextLetterCheck = 1;
		float timeOfLastCheck;
		public Arduino arduino;
		public static bool communicationWithArduinoAchieved = false; //begin the main program because arduino has finished connecting/configuring.

		const int NUM_VALUES_PER_COLOR = 3;
		const int NUM_PINS_PER_LETTER_POSITION = 7;
		const int NUM_LETTER_POSITIONS = 7;

		const char BLANK = ' ';
		int[][] ledOutputPins = new int[][]{

            

        new int[]{27,25,23}, //inactive(?) first slot-- colour circuit hooked up but letter circuit not
        new int[]{17,18,19}, 
		new int[]{14,15,16}, 
		new int[]{10,9,8}, 
		new int[]{4,3,2}, 
		new int[]{11,12,13},
        new int[]{5,6,7}
    
	};
		static int eye = 49;
		int[][] platformInputPins = new int[][]{
        new int[]{7,7,7,7,7,7,7}, //inactive first slot (index 0)



        new int[]{24,36,34,32,30,28,26}, ///second slot (index 1)
        new int[]{40,52,50,48,46,44,42}, //third slot (index 2)
        new int[]{41,37,35,33,31,39,29}, // ... so on
    
        new int[]{61, 53, 51, 49, 47, 45, 43}, //fifth slot 
     
        new int[]{62, 68, 67, 66, 65, 64, 63},
     
        new int[]{54, 60, 59, 58, 57, 56, 55}        
	};
		int[][] stateOfPlatformInputPins = new int[][]{
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1},
		new int[]{1,1,1,1,1,1,1}
	};
		char letter;
		int[][] colorsAtEachPosition = new int[][]{
		new int[]{0,0,0},
		new int[]{0,0,0},
		new int[]{0,0,0},
		new int[]{0,0,0},
		new int[]{0,0,0},
		new int[]{0,0,0},
		new int[]{0,0,0}
	};
		
		struct ArduinoLetterData
		{
				public char letter;
				public int position;
	
		}
		const char SAME = '?';
		ArduinoLetterData change;

		public void Start(){
				instance = this;
				GameObject uniduino = GameObject.Find ("Uniduino");
				Events.Dispatcher.OnInputTypeSelected += (InputType type) => {
					if(type == InputType.KEYBOARD) {
						gameObject.SetActive(false);
						uniduino.SetActive(false);
				} else {
					
					uniduino.GetComponent<Uniduino.Arduino> ().Connect ();
					change = new ArduinoLetterData ();
					arduino = Arduino.global;
					arduino.Setup (ConfigurePins);

				}
			};

		}
		

		void ConfigurePins ()
		{
				//Initialize the input Pins

				for (int y=1; y<NUM_LETTER_POSITIONS; y++) {
						for (int i=0; i<NUM_PINS_PER_LETTER_POSITION; i++) {
       
								arduino.pinMode (platformInputPins [y] [i], PinMode.INPUT);
								arduino.digitalWrite (platformInputPins [y] [i], Arduino.HIGH);
           
								arduino.reportDigital ((byte)(platformInputPins [y] [i] / 8), (byte)1);
                                
						}
				}

			
		
				//Initialize the Led Pins
				for (int y=0; y<NUM_LETTER_POSITIONS; y++) {
						for (int i=0; i<NUM_VALUES_PER_COLOR; i++) {
								int pin = ledOutputPins [y] [i];
							
								arduino.pinMode (ledOutputPins [y] [i], PinMode.OUTPUT);
						}
				}
			
				Begin ();
				Debug.Log ("begin: " + communicationWithArduinoAchieved);
		}

		void Begin ()
		{
				communicationWithArduinoAchieved = true;
				timeOfLastCheck = Time.time;
				for (int i=0; i<ledOutputPins.Length; i++)
						ShutOffAt (i);



			
		}

		public void ShutOffAt (int pos)
		{
				pos = pos % ledOutputPins.Length;
		
				int[] pins = ledOutputPins [pos];
		
				foreach (int pin in pins)
						arduino.digitalWrite (pin, Arduino.LOW);
		
		
		
		
		}

		public void LightUpAllAt (int pos)
		{
				pos = pos % ledOutputPins.Length;
	
				int[] pins = ledOutputPins [pos];
			
				foreach (int pin in pins)
						arduino.digitalWrite (pin, Arduino.HIGH);




		}
	
		void Update ()
		{
				if (communicationWithArduinoAchieved) { //do not begin until after the connection with arduino is established.

						if (TimeToCheckForChangeHasElapsed ()) {

								SearchForAndSaveChangedLetterAndPosition ();
				if (WasAChange ())
					Events.Dispatcher.RecordNewUserInputLetter (
						change.letter, 
						AdjustArduinoPositionForScreen (change.position));
									
			
								timeOfLastCheck = Time.time;

				
						}
				}
		

		}

		int testPosition;

		bool ParseNumericKey ()
		{
				string s;
		
				for (int i=0; i<ledOutputPins.Length; i++) {
						s = "" + i;
						if (Input.GetKey (s)) {
								testPosition = i;
								return true;
						}
				}
				return false;
		}

		bool TimeToCheckForChangeHasElapsed ()
		{
				
				return Time.time - timeOfLastCheck > minimumSecondsBeforeNextLetterCheck;


		}

		public void ColorNthTangibleLetter (int position, Color color)
		{
      
				ApplyNewColorTo (position, color);
			

		}

		void ApplyNewColorTo (int position, Color color)
		{
                
				for (int colorChannel=0; colorChannel<NUM_VALUES_PER_COLOR; colorChannel++) {
						//int valueAtChannel = ScaledColorChannel (color, colorChannel);//CategoricalColorChannel (color, colorChannel);
						int valueAtChannel = CategoricalColorChannel (color, colorChannel);

						int rescaledChannel = (2 - colorChannel);
						int pin = 0;
						if (position < ledOutputPins.Length && position > -1) {
                
								pin = ledOutputPins [position] [rescaledChannel];

						} else
								return;

						
						arduino.digitalWrite (pin, CategoricalColorChannel (color, colorChannel));
						Debug.Log ("position " + position + " pin # " + ledOutputPins [position] [rescaledChannel] + " " + valueAtChannel + " " + ParseLetter (position));
				}
	
		}

		bool WasAChange ()
		{
				return change.letter != SAME;
		}



		int AdjustArduinoPositionForScreen (int arduinoPlatformPosition)
		{
				//starts counting at 1; screen starts counting at 0
				return arduinoPlatformPosition - 1;

		}

		void SearchForAndSaveChangedLetterAndPosition ()
		{

				change.letter = SAME;

  
				//for (int letterPlatformPosition=0; letterPlatformPosition<NUM_LETTER_POSITIONS; letterPlatformPosition++) {

				//skip 0th index. Min has not connected the letter inputs at the 0th position (the colour circuit is connected though)
				for (int letterPlatformPosition = 1; letterPlatformPosition < NUM_LETTER_POSITIONS; letterPlatformPosition++) {
						//for (int letterPlatformPosition = 0; letterPlatformPosition < NUM_LETTER_POSITIONS; letterPlatformPosition++)
						for (int pin = 0; pin < NUM_PINS_PER_LETTER_POSITION; pin++) {
								if (LetterAtPositionChanged (letterPlatformPosition, pin)) { //change of state
										RememberChangedPinStatesOfPositionThatExperiencedChange (letterPlatformPosition, pin);
										SaveChangeAsArduinoLetterData (letterPlatformPosition);
										return;
								}
						}
				}

		}

		void RememberChangedPinStatesOfPositionThatExperiencedChange (int position, int fromPin)
		{
				for (; fromPin<stateOfPlatformInputPins[position].Length; fromPin++)
						stateOfPlatformInputPins [position] [fromPin] = arduino.digitalRead (platformInputPins [position] [fromPin]);
		}
	    
		void SaveChangeAsArduinoLetterData (int position)
		{
				char newletter = ParseLetter (position);
				change.letter = newletter;
				change.position = position;
		}

		//the letter at a position has changed if any of its pins have changed state.
		bool LetterAtPositionChanged (int position, int pin)
		{
				return arduino.digitalRead (platformInputPins [position] [pin]) != stateOfPlatformInputPins [position] [pin];// && position <4;

		}
	 
		//assumes that we have updated the saved pin states at the given position after having discovered that there was a change here.
		//returns the char letter that corresponds to the new combination of active pins at position.
		char ParseLetter (int position)
		{      
				char letter = BLANK; //if NO letters are parsed, then the change was to remove a letter. 
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW) { 
						//if(position!=3)
						letter = 'E'; //hack because the pins are malfunctioning. april 21 2015.
				}
				if (stateOfPlatformInputPins [position] [1] == Arduino.LOW) {
						letter = 'S'; 
				}
				if (stateOfPlatformInputPins [position] [2] == Arduino.LOW) {
						//if(position!=3)  //hack because the pins are malfunctioning. april 21 2015.
						letter = 'I';
				}
			
				if (stateOfPlatformInputPins [position] [4] == Arduino.LOW) {
						letter = 'O';
				}
		
				if (stateOfPlatformInputPins [position] [5] == Arduino.LOW) {
						letter = 'A';
				}
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW) {
						letter = 'T';
				}

		
				//Detect the double pins 

				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW) 
						letter = 'U';
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW) 
						letter = 'C';

				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW) 
						letter = 'D';
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [5] == Arduino.LOW) 
						letter = 'R';
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [6] == Arduino.LOW) 
						letter = 'H'; 
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [5] == Arduino.LOW) 
						letter = 'M';   
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW) 
						letter = 'W';   
	  
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW) 
						letter = 'G';
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW) 
						letter = 'P';
				if (stateOfPlatformInputPins [position] [5] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW) 
						letter = 'Y'; 

				if (stateOfPlatformInputPins [position] [5] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW) 
						letter = 'V'; 
				if (stateOfPlatformInputPins [position] [5] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW) 
						letter = 'J'; 
			
				if (stateOfPlatformInputPins [position] [4] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW) 
						letter = 'X'; 
				if (stateOfPlatformInputPins [position] [4] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW) 
						letter = 'Q'; 


				if (stateOfPlatformInputPins [position] [2] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW) 
						letter = 'Z';  


				//Detect the triple pins 
		 
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW) 
						letter = 'K'; 
				if (stateOfPlatformInputPins [position] [0] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW && stateOfPlatformInputPins [position] [5] == Arduino.LOW) 
						letter = 'L';
				if (stateOfPlatformInputPins [position] [5] == Arduino.LOW && stateOfPlatformInputPins [position] [4] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW) 
						letter = 'B'; 
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [2] == Arduino.LOW && stateOfPlatformInputPins [position] [0] == Arduino.LOW) 
						letter = 'F'; 
				if (stateOfPlatformInputPins [position] [6] == Arduino.LOW && stateOfPlatformInputPins [position] [1] == Arduino.LOW && stateOfPlatformInputPins [position] [0] == Arduino.LOW) 
						letter = 'N'; 

				return letter;
		}





		


       
		//the word will have at most 6 letters.
		//we need to put these letters in the indexes 1 to 6
		/*public void UpdateColoursOfTangibleLetters (UserWord newWord)
		{
		
		
				for (int positionOfLetter=0; positionOfLetter<newWord.Count; positionOfLetter++) {
						LetterSoundComponent p = newWord.Get (positionOfLetter);
						Color color = p.GetColour();
                    

						// int positionOfLetter_ = NUM_LETTER_POSITIONS - positionOfLetter;
						positionOfLetter = positionOfLetter + 1;
						ApplyNewColorTo (positionOfLetter, color);

			
				}
			

			
			
		}*/

		int CategoricalColorChannel (Color c, int channel)
		{
				if (channel == 0)
						return (c.r >= .5 ? Arduino.HIGH : Arduino.LOW);
				if (channel == 1)
						return (c.g >= 0.5 ? Arduino.HIGH : Arduino.LOW);
				return (c.b >= .5 ? Arduino.HIGH : Arduino.LOW);

		}
	
		int ScaledColorChannel (Color c, int channel)
		{
				return (int)(255 * (channel == 0 ? c.r : (channel == 1 ? c.g : c.b)));
		
		
		}








}
