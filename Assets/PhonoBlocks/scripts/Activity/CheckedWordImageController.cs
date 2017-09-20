﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class CheckedWordImageController : MonoBehaviour
{
		GameObject checkedWordImage;
		UITexture img;
		BoxCollider clickTrigger;
		long showTime = -1;
		bool disableTextureOnPress;
		long defaultDisplayTime = 2000;
		bool caller_ends_display;


			void Start (){

				//remove the old image, if it's still there
				Dispatcher.Instance.NewProblemBegun.Subscribe((ProblemData problem) => {
							EndDisplay ();
				});
				//display the target word image
				Dispatcher.Instance.CurrentProblemCompleted.Subscribe(DisplayTargetWord);
				//level three hint; show the image of the target word.
				Dispatcher.Instance.HintRequested.Subscribe(() => {
						if(Dispatcher._State.CurrentHintNumber == Parameters.Hints.Descriptions.
							PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT){
								DisplayTargetWord();
							}
				});
		        //if the current state of user input letters corresponds to a saved image, then
				//display it.
				Dispatcher.Instance.UserAddedWordToHistory.Subscribe(DisplayCurrentInputWord);
				Dispatcher.Instance.ActivitySceneLoaded.Subscribe(() => {
						checkedWordImage = GameObject.Find("CheckedWordImage");
						img = checkedWordImage.GetComponent<UITexture> ();
						img.enabled = false;
						clickTrigger = checkedWordImage.GetComponent<BoxCollider> ();
						clickTrigger.enabled = false;
				});
				
		}

		void DisplayCurrentInputWord(){
			DisplayImageForWordIfAny (Dispatcher._State.UserInputLetters);
		}


		void DisplayTargetWord(){
			DisplayImageForWordIfAny (Dispatcher._State.TargetWord);
		}

		void DisplayImageForWordIfAny(string word){
			word = word.Trim ();
			Texture2D newimg = (Texture2D)Resources.Load ($"{Parameters.FILEPATHS.RESOURCES_WORD_IMAGE_PATH}{word}", typeof(Texture2D));
			if (!ReferenceEquals (newimg, null)) {
				ShowImage (newimg, false, true);
			}

		}

		public void ShowImage (Texture2D newimg, long showTime)
		{
				this.showTime = showTime;
				SetAndEnableTexture (newimg);


		}

		public void ShowImage (Texture2D newimg, bool disableTextureOnPress, bool caller_ends_display=false)
		{
				this.caller_ends_display = caller_ends_display;
				if (newimg != null) {
						if (disableTextureOnPress) {
								this.disableTextureOnPress = disableTextureOnPress;
								SetAndEnableTexture (newimg);
								clickTrigger.enabled = true;
						} else
								ShowImage (newimg, defaultDisplayTime);
				}
		}

		void SetAndEnableTexture (Texture2D newImg)
		{
				img.mainTexture = newImg;
				img.enabled = true;
		}

		void OnPress (bool isPressed)
		{
				if (isPressed && disableTextureOnPress) {
						EndDisplay ();
				}
		}

		public void EndDisplay ()
		{

		    
				img.enabled = false;
				caller_ends_display = false;
				if (disableTextureOnPress) {
						disableTextureOnPress = false;
						clickTrigger.enabled = false;
				}
				if (showTime > 0) {
						showTime = -1;
				}

		}

		void Update ()
		{
				if (!caller_ends_display) {
						if (showTime > 0)
								showTime--;
						if (showTime == 0) {
								EndDisplay ();
						}
				}
				
		}

		public bool WordImageIsOnDisplay ()
		{
				return img.enabled;


		}


		




}
