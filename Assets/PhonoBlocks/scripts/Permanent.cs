using UnityEngine;
using System.Collections;

public class Permanent : MonoBehaviour
{


		void Awake ()
		{
				Object.DontDestroyOnLoad (gameObject);
		}



}
