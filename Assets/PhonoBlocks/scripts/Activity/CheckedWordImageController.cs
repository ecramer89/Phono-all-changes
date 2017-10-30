using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class CheckedWordImageController : PhonoBlocksSubscriber
{
		GameObject checkedWordImage;
		UITexture img;

	public override void SubscribeToAll(PhonoBlocksScene nextToLoad){
		if(nextToLoad == PhonoBlocksScene.MainMenu) return;
		if(nextToLoad == PhonoBlocksScene.Activity) {
			Transaction.Instance.ActivitySceneLoaded.Subscribe(this,() => {
				checkedWordImage = GameObject.Find("CheckedWordImage");
				img = checkedWordImage.GetComponent<UITexture> ();
				img.enabled = false;
			});
		//display the target word image
		Transaction.Instance.CurrentProblemCompleted.Subscribe(this,DisplayTargetWord);
		//level three hint; show the image of the target word.
		Transaction.Instance.HintRequested.Subscribe(this,() => {
			if(Transaction.Instance.State.CurrentHintNumber == Parameters.Hints.Descriptions.
				PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT){
				DisplayTargetWord();
			}
		});
		//if the current state of user input letters corresponds to a saved image, then
		//display it.
		Transaction.Instance.UserAddedWordToHistory.Subscribe(this,DisplayCurrentInputWord);

			/*
			 * end the display once all letters have been removed
			 * */
			Transaction.Instance.UserEnteredNewLetter.Subscribe(this, (char newLetter,int position)=>{
					if(
					img.enabled && 
					newLetter == ' ' && 
					Transaction.Instance.State.UserInputLetters.Trim()==""){
						EndDisplay();
					}
			});
		}

	}
		
		void DisplayCurrentInputWord(){
			DisplayImageForWordIfAny (Transaction.Instance.State.UserInputLetters);
		}


		void DisplayTargetWord(){
			DisplayImageForWordIfAny (Transaction.Instance.State.TargetWord);
		}

		void DisplayImageForWordIfAny(string word){
			word = word.Trim ();
			Texture2D newimg = (Texture2D)Resources.Load ($"{Parameters.FILEPATHS.RESOURCES_WORD_IMAGE_PATH}{word}", typeof(Texture2D));
			if (!ReferenceEquals (newimg, null)) {
				ShowImage (newimg);
			}

		}


		void ShowImage (Texture2D newImg)
		{   				
			img.mainTexture = newImg;
			img.enabled = true;
		}


		void EndDisplay ()
		{
				img.enabled = false;
			

		}


}
