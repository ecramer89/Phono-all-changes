using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class LetterSoundComponent
{
	


		protected int placeInWord;
	
	   
		protected int soundType;

		public int SoundType {
				get {
						return soundType;
				}
		}
	
		public virtual string Sound ()
		{
				switch (soundType) {
				case SILENT:
						return "silent";
				case R_CONTROLLED:
						return "r controlled";
			
				}
				return "none";
		
		}
	
		protected const int SILENT = -1;
		protected const int R_CONTROLLED = SILENT + 3;
	
		public void MakeRControlled ()
		{
				soundType = R_CONTROLLED;
		}
	
		public void Silence ()
		{
				soundType = SILENT;
		}

	   public abstract bool IsComposite();
	
		public bool IsConsonantConsonantDigraphOrBlend {
		
				get {
						return this is Consonant || this is ConsonantDigraph || this is Blend;
				}

		}

		public override bool Equals(object other){
			if (other == null)
				return false;
			if (other == this)
				return true;
			if(!other.GetType().Equals(this.GetType())) return false;

			LetterSoundComponent oLSC = (LetterSoundComponent)other;
			return AsString.Equals (oLSC.AsString);
		}

		public virtual bool IsBlank(){
			return false;
		}
		
		public bool LettersMatch (LetterSoundComponent other)
		{
				return LettersAre (other.asString);
		
		
		}
	
		public bool IsVowelOrVowelDigraph {
		
				get {
						return this is Vowel || this is VowelDigraph;
				}
		
		
		
		}
	
		public LetterSoundComponent (string asString)
		{
				this.asString = asString;
		
			
		}
	
		string asString;
	
		public virtual string AsString {
				get {
						return asString;
				}
		
		
		}

		public bool LettersAre (string test)
		{
				return asString.Equals (test);


		}
	
		public char LetterAt (int c)
		{
				return AsString [c];
		}
	
		public int Length {
				get{ return asString.Length;}
		
		}
	
		protected Color color = Color.gray;
	
		public virtual Color GetColour ()
		{       
			
				return color;
				
		}

	
		public abstract void ApplyColor ();
	
		public bool TryApplyColor (Color suggestion)
		{
				color = suggestion;
				ModifyColorBySound ();

				return true;
		
		}
	


	
		protected virtual void ModifyColorBySound ()
		{
				if (soundType == SILENT)
						color = SessionsDirector.colourCodingScheme.GetColourForSilent ();
		
		}

	
	      		
		

}

public abstract class LetterSoundComposite : LetterSoundComponent
{
		protected List<LetterSoundComponent> children;
	
		public List<LetterSoundComponent> Children {
				get {
						return children;
				}
		}
	
		public LetterSoundComposite (string asString) : base(asString)
		{
				children = new List<LetterSoundComponent> ();
		}
	
		public void AddChild (LetterSoundComponent child)
		{
				if (children == null)
						children = new List<LetterSoundComponent> ();
				children.Add (child);
		
		}

		public override bool IsComposite(){
			return true;
		}
	
		public override void ApplyColor ()
		{
				ApplyColorToComposite ();
				foreach (LetterSoundComponent child in children)
						child.TryApplyColor (GetColour ());
		
		}
	

		protected abstract void ApplyColorToComposite ();

		
}

public abstract class Letter : LetterSoundComponent
{
		public Letter (string asString): base(asString)
		{
		}

		public override bool IsComposite(){
			return false;
		}
	
}

public class Consonant : Letter
{
		const int HARD = SILENT + 1;
		const int SOFT = SILENT + 2;


		//note: as of Min's study (fall 2015) we aren't distinguishing between hard and soft consonants. also note that
		//at the moment, effects on small components of digraphs (e.g., consonant letters) are overriding colours that are assigned to
		//larger units. for example having the "s" default to soft consonant colour was overriding the special colouration that s would have when it
		//appears in a consonant digraph. so in the future you will need to change how colour pritory works if you want to incoporate this
		public override string Sound ()
		{
				switch (soundType) {
			
				case HARD:
						return "hard";
				case SOFT:
						return "soft";
				}
				return base.Sound ();
		
		
		}
	
		public void MakeSoft ()
		{
				soundType = SOFT;
		}
	
		public void MakeHard ()
		{
				soundType = HARD;
		}
	
		public Consonant (string asString) : base(asString)
		{
				
		}
	
		public Consonant (char asString) : base(asString+"")
		{
				
		}
	
	    public override void ApplyColor ()
		{
		  color = SessionsDirector.colourCodingScheme.GetColorsForConsonant ();
		}

}

public class Vowel : Letter
{
		const int LONG = SILENT + 1;
		const int SHORT = SILENT + 2;


		public override string Sound ()
		{
				switch (soundType) {
			
				case LONG:
						return "long";
				case SHORT:
						return "short";
				}
				return base.Sound ();
		
	
		}
	
		public void MakeLong ()
		{
				soundType = LONG;
		
		}
	
		public void MakeShort ()
		{
				soundType = SHORT;
		
		}
	
			public Vowel (string asString) : base(asString)
			{

					MakeLong ();
			}

			public Vowel (char asString) : this(""+asString){}

	
	     public override void ApplyColor ()
	{       


		switch (soundType) {
				
			case SILENT:
				color = SessionsDirector.colourCodingScheme.GetColourForSilent ();
				break;
			case SHORT:
				color = SessionsDirector.colourCodingScheme.GetColorsForShortVowel ();
				break;
			default: 
				color = SessionsDirector.colourCodingScheme.GetColorsForLongVowel ();
				break;
		     }
		}
	
		protected override void ModifyColorBySound ()
		{
				base.ModifyColorBySound ();
				if (soundType == SHORT) {
						color = SessionsDirector.colourCodingScheme.GetColorsForShortVowel ();
				

				}
	
		}
}

public class Blank : Letter
{
	
	
		public Blank () : base(" ")
		{
		}

	    public override void ApplyColor ()
		{
		
		     color = SessionsDirector.colourCodingScheme.GetColorsForOff ();
		
		}

		public override bool IsBlank(){
			return true;
		}
}

public class Blend : LetterSoundComposite
{

		Color[] colors = new Color[2];
		int colorIdx = 1;

		public Blend (string asString) : base(asString)
		{
				
		}
	
		protected override void ApplyColorToComposite ()
	{
			color = SessionsDirector.colourCodingScheme.GetColorsForConsonantBlends ();
	
	}
	
	
	
}

public class VowelR: LetterSoundComposite
{
		public VowelR (string asString) : base(asString)
		{
		}
	
		protected override void ApplyColorToComposite ()
		{
				color = SessionsDirector.colourCodingScheme.GetColorsForRControlledVowel ();
		
		}

	
	
}

public class ConsonantDigraph : LetterSoundComposite
{
		public ConsonantDigraph (string asString) : base(asString)
		{
		}
	
		protected override void ApplyColorToComposite ()
		{
				color = SessionsDirector.colourCodingScheme.GetColorsForConsonantDigraphs ();
		
		}

}

public class VowelDigraph : LetterSoundComposite
{
		public VowelDigraph (string asString) : base(asString)
		{
		}
	
		protected override void ApplyColorToComposite ()
		{
				color = SessionsDirector.colourCodingScheme.GetColorsForVowelDigraphs (); 
			
		}

	
}

public class StableSyllable : LetterSoundComposite
{
		public StableSyllable (string asString) : base(asString)
		{
		}
	
		protected override void ApplyColorToComposite ()
		{
		
				color = SessionsDirector.colourCodingScheme.GetColoursForSyllables ();
		
		}

	
	
}


	
	


