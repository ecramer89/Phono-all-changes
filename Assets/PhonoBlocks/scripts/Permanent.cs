using UnityEngine;
using System.Collections;

public class Permanent : MonoBehaviour
{


		void Start ()
		{
				Object.DontDestroyOnLoad (gameObject);
		}



}
