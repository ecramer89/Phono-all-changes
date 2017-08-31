using UnityEngine;
using System.Collections;

public class Permanent : MonoBehaviour
{

		public int active_level = -1;


		// Use this for initialization
		void Start ()
		{
				Object.DontDestroyOnLoad (gameObject);
		}

		void OnLevelWasLoaded (int level)
		{
				if (active_level > -1) {
						if (level == active_level) {
								gameObject.SetActive (true);


						} else {

								gameObject.SetActive (false);
						}

				}
		}

}
