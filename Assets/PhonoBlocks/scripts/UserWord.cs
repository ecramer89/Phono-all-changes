using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserWord : PhonoBlocksController, IEnumerable
{

		List<LetterSoundComponent> letterSoundUnits;


		public void ApplyColoursToLetterSoundComponents ()
		{
				int idx = 0;
				foreach (LetterSoundComponent l in letterSoundUnits) {
						l.ApplyColor ();
			         
						idx++;

				}
		}

		public UserWord (List<LetterSoundComponent> letterSoundUnits)
		{
				this.letterSoundUnits = letterSoundUnits;

		}

		public int Count {
				get {
						return letterSoundUnits.Count;
				}
		}

		public void Add (LetterSoundComponent l)
		{
				letterSoundUnits.Add (l);

		}

		public LetterSoundComponent Get (int idx)
		{
				return letterSoundUnits [idx];
		
		}

		public IEnumerator GetEnumerator ()
		{
				return letterSoundUnits.GetEnumerator ();


		}
		

	public LetterSoundComponent GetLetterSoundComponentForIndexRelativeWholeWord(int index){
		int letterCount = 0;
		foreach(LetterSoundComponent lc in this){
			letterCount+=lc.Length;
			if(index < letterCount) return lc;
		}
		return null;

	}









}
