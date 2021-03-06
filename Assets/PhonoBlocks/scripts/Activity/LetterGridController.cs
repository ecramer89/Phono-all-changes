using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LetterGridController : MonoBehaviour
{       
		public GameObject letterGrid;
		public GameObject letterHighlightsGrid;
		public GameObject letterUnderlinesGrid;
		public int letterImageWidth;
		public int letterImageHeight;

		public int LetterImageHeight {
				get {
						return letterImageHeight;
				}
				set {
						letterImageHeight = value;
				}

		
		
		}

		public Texture2D blankLetter;
		LetterImageTable letterImageTable;

		void Start ()
		{      
				InitializeFieldsIfNecessary ();
		}

		void InitializeFieldsIfNecessary ()
		{
				if (letterGrid == null)
						letterGrid = gameObject;
				if (letterImageTable == null)
						letterImageTable = GameObject.Find ("DataTables").GetComponent<LetterImageTable> ();
				if (blankLetter == null)
						blankLetter = letterImageTable.GetBlankLetterImage ();
				if (letterImageWidth == 0 || letterImageHeight == 0) //you can specify dimensions for the image that are different from those of the grid.
						MatchLetterImageToGridCellDimensions (); //but if nothing is specified it defaults to make it the same size as the grid cells.

				if (letterHighlightsGrid) {
						UIGrid high = letterHighlightsGrid.GetComponent<UIGrid> ();
						UIGrid letters = letterGrid.GetComponent<UIGrid> ();
						high.cellWidth = letters.cellWidth;
						high.cellHeight = letters.cellHeight;
						letterHighlightsGrid.transform.position = letterGrid.transform.position;


				}

				if (letterHighlightsGrid) {
						UIGrid high = letterHighlightsGrid.GetComponent<UIGrid> ();
						UIGrid letters = letterGrid.GetComponent<UIGrid> ();
						high.cellWidth = letters.cellWidth;
						high.cellHeight = letters.cellHeight;
						letterHighlightsGrid.transform.position = letterGrid.transform.position;

				}
	
	
		}




		public void setNumVisibleLetterLines (int numVisibleLetterLines)
		{
				int i = 0;
				for (; i<letterUnderlinesGrid.transform.childCount; i++) {

						GameObject cell = GetCell (i, letterUnderlinesGrid);
						UITexture lineImage = cell.GetComponent<UITexture> ();


						if (i < numVisibleLetterLines) {
								lineImage.enabled = true;

						} else {
								lineImage.enabled = false;
		
						}
				}
	
		}

		

		void MatchLetterImageToGridCellDimensions ()
		{
			
				UIGrid grid = letterGrid.GetComponent<UIGrid> ();
				letterImageWidth = (int)grid.cellWidth;
				letterImageHeight = (int)grid.cellHeight;

		}
	
		public void InitializeBlankLetterSpaces (int numCells)
		{
				InitializeFieldsIfNecessary ();
    
				for (int i= 0; i<numCells; i++) {
				
						GameObject newLetter = CreateLetterBarCell (blankLetter, i + "", Color.white);
						InteractiveLetter iLetter = newLetter.GetComponent<InteractiveLetter>();
						iLetter.Position = i;
						//the word history controller also involves a letter grid... but doesn't highlight selected letters or show the lines.
						//consider changing this so that the arduino letter controller is what creates the underlines and highlights...
						//seems weird that this class would be making decisions about that
						if (letterHighlightsGrid) {
								UITexture letterHighlight = CreateLetterHighlightCell ();
								iLetter.SelectHighlight = letterHighlight;
						}
						if (letterUnderlinesGrid) {
								CreateLetterUnderlineCell (i);
						}
				}
				RepositionGrids ();
		}

		public void RepositionGrids ()
		{
				letterGrid.GetComponent<UIGrid> ().Reposition ();
				if (letterHighlightsGrid)
						letterHighlightsGrid.GetComponent<UIGrid> ().Reposition ();
				if (letterUnderlinesGrid)
						letterUnderlinesGrid.GetComponent<UIGrid> ().Reposition ();
			
		}
		

		public Texture2D GetAppropriatelyScaledImageForLetter(String letter){
			return letter == " " ? blankLetter : ConfigureTextureForLetterGrid (letterImageTable.GetLetterImageFromLetter (letter));

		}


		public UITexture CreateLetterHighlightCell ()
		{
				UITexture selectHighlight = NGUITools.AddChild<UITexture> (letterHighlightsGrid);
				selectHighlight.transform.localScale = new Vector2 (letterImageWidth, letterImageHeight);
				selectHighlight = SetShaders (selectHighlight);
				selectHighlight.mainTexture = ConfigureTextureForLetterGrid (LetterImageTable.SelectLetterImage);
				selectHighlight.enabled = false;
			
				return selectHighlight;

		}

		public void CreateLetterUnderlineCell (int index)
		{
	
				UITexture underline = NGUITools.AddChild<UITexture> (letterUnderlinesGrid);
				underline.transform.localScale = new Vector2 (letterImageWidth, letterImageHeight);
				underline = SetShaders (underline);
				underline.mainTexture = ConfigureTextureForLetterGrid (LetterImageTable.LetterUnderlineImage);
		        underline.gameObject.name = "" + index;
				
		}


		public InteractiveLetter GetInteractiveLetter (int position)
		{

				GameObject cell = GetLetterCell (position);
				return cell.GetComponent<InteractiveLetter> (); 
		}

		public GameObject GetLetterCell (int position)
		{     
			
				return GetCell (position, letterGrid);
		}

		public GameObject GetCell (int position, GameObject fromGrid)
		{     
				if (position < fromGrid.transform.childCount && position > -1) {
						return fromGrid.transform.Find (position + "").gameObject; //cell to update
				} else {

						return null;
			
				}
		}

		public GameObject CreateLetterBarCell (Texture2D tex2D, string position, Color c)
		{      
				Texture2D tex2dCopy = ConfigureTextureForLetterGrid (tex2D);
				UITexture ut = NGUITools.AddChild<UITexture> (letterGrid);
				ut.material = new Material (Shader.Find ("Unlit/Transparent Colored"));
				ut.shader = Shader.Find ("Unlit/Transparent Colored");
				ut.mainTexture = tex2dCopy;
				ut.color = c;
				BoxCollider b = ut.gameObject.AddComponent<BoxCollider> ();
				b.isTrigger = true;
				b.size = new Vector2 (.6f, .6f);

	

				InteractiveLetter l = ut.gameObject.AddComponent<InteractiveLetter> ();
				l.texture = ut;
				l.Trigger = b;
				l.UpdateLetterImageAndInputDerivedColor (tex2dCopy, c);
				
	
				ut.gameObject.name = position;
				ut.MakePixelPerfect ();

				return l.gameObject;

		}

		UITexture SetShaders (UITexture ut)
		{
				ut.material = new Material (Shader.Find ("Unlit/Transparent Colored"));
				ut.shader = Shader.Find ("Unlit/Transparent Colored");

				return ut;
		}
	
		public Texture2D ConfigureTextureForLetterGrid (Texture tex2D)
		{
				Texture2D tex2dCopy = Instantiate (tex2D) as Texture2D;
				TextureScale.Bilinear (tex2dCopy, (int)letterImageWidth, (int)letterImageHeight);
				return tex2dCopy;
		}

		public List<InteractiveLetter> GetLetters ()
	{       
				List<InteractiveLetter> list = new List<InteractiveLetter> ();
				foreach (Transform child in letterGrid.transform) {
						InteractiveLetter letter = child.GetComponent<InteractiveLetter> ();
								list.Add (letter);

				}
				return list;
		
		}
	  
	
}
	