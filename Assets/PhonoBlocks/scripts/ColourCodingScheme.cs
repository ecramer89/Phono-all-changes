using UnityEngine;
using System.Collections;

public abstract class ColourCodingScheme
{     

		//todo refactor rename to 'ActivityColorRule'
		//requirement for all activities:
		//establish colours for all generally relevant LetterSoundComponents.
		//we can do so by adjusting the singleton (LetterSoundComponent Color Map)

		protected Color green;
		protected Color blue;
		protected Color red;
		protected Color pink;
		protected Color yellow;
		protected Color gray;
		public string label;
		protected Color[] errorColors;
		protected int alternate = 0;
	  

		public ColourCodingScheme ()
		{
				green = Color.green;
				green.r = (float)95 / (float)255;
				green.b = (float)127 / (float)255;
				green.g = (float)180 / (float)255;
				blue = Color.blue;
				blue.r = (float)105 / (float)255;
				blue.g = (float)210 / (float)255;
				blue.b = (float)231 / (float)255;
				red = Color.red;
				red.g = (float)58 / (float)255;
				red.b = (float)68 / (float)255;
				pink = Color.red;
				pink.r = (float)247 / (float)255;
				pink.g = (float)98 / (float)255;
				pink.b = (float)162 / (float)255;
				yellow = Color.yellow;
				yellow.r = (float)249 / (float)255;
				yellow.g = (float)249 / (float)255;
				yellow.b = (float)98 / (float)255;
				gray = Color.gray;
				gray.r = (float)(gray.r * 1.2);
				gray.g = gray.r;
				gray.b = gray.r;

		        errorColors = new Color[]{gray, Color.white};
		}

		public virtual Color GetColorsForOff ()
		{
				return gray;
		
		}
		

		public virtual Color[] GetErrorColors(){
			return errorColors;
		}


		public virtual Color GetColorsForWholeWord ()
		{
				return pink;
		
		}
	
		public virtual Color GetColorsForLongVowel ()
		{
				return Color.white;
		
		}

		public virtual Color GetColorsForConsonant ()
		{
				return Color.white;
		}

		public virtual Color GetColourForSilent ()
		{
				return Color.gray;
		}
	
		public virtual Color GetColorsForShortVowel ()
		{
				return Color.white;
		}
	
		public virtual Color GetColorsForConsonantBlends ()
		{
				return Color.white;
		}
	
		public virtual Color GetColoursForSyllables ()
		{
				return Color.white;
		}
	

		public virtual Color GetColorsForConsonantDigraphs ()
		{
				return Color.white;
		}
	
		public virtual Color GetColorsForVowelDigraphs ()
		{
				return Color.white;
		}
	
		public virtual Color GetColorsForRControlledVowel ()
		{
		
				return Color.white;
		}

		public virtual Color GetColorForPortionOfTargetComposite(){
				return Color.gray;
		}

	
}


//Colour coding schemes for Min's Study:
//also consonant digraphs
class ConsonantBlends : NoColour
{

		public ConsonantBlends (): base()
		{

				label = "Blends";

		}



		//todo initially thought we would distinguish between different blend positions.
		//no longer the case. should collapse into one function.
		public override Color GetColorsForConsonantBlends ()
		{
				return green;
		}

		public override Color GetColorForPortionOfTargetComposite(){
				return blue;
		}

}

class OpenClosedVowel : NoColour
{

		public OpenClosedVowel () : base()
		{
				label = "openClosedVowel";
		}

		public override Color GetColorsForShortVowel ()
		{
				return yellow;
		}

		public override Color GetColorsForLongVowel ()
		{
				return red;
		}
		


}

//changes to parent
//colour silent e black
class VowelInfluenceERule : NoColour
{
	
		public VowelInfluenceERule () : base()
		{
				label = "vowelInfluenceE";
		}

		public override Color GetColorsForLongVowel ()
		{
				return red;
		}

	    public override Color GetColorsForShortVowel()
	    {
	        return yellow;
	    }

   

	
}


//changes to parent
//colour consonant digraphs green
class ConsonantDigraphs : NoColour
{
	
		public ConsonantDigraphs () : base()
		{
				label = "consonantDigraphs";
		}

		public override Color GetColorsForConsonantDigraphs ()
		{
				return green;
		}
		
}


//changes to parent
//colour r controlled vowels purple
class RControlledVowel: VowelDigraphs
{
		public RControlledVowel () : base()
		{
				label = "rControlledVowel";
		}

		public override Color GetColorsForRControlledVowel ()
		{   
				return red;
		}

		public override Color GetColorsForVowelDigraphs ()
		{
				return gray;
		}


}

//changes to parent
//colour vowel digraphs orange
class VowelDigraphs : NoColour

{
		public VowelDigraphs () : base()
		{
		   label = "vowel Digraphs";
		}

		public override Color GetColorsForVowelDigraphs ()
		{
				return red;
		}


}

class SyllableDivision : NoColour
{
		public SyllableDivision () : base()
		{
				label = "syllableDivision";
		}
	//not sure if this is being used. check with min
}




//different activities can "opt out" of certain distinctions, and add new ones.
public class NoColour : ColourCodingScheme
{

		




}

