using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
public abstract class PhonoBlocksSubscriber : MonoBehaviour {


	[SerializeField] int priority;
	public int Priority{
		get {
			return priority;
		}
	}
	public abstract void SubscribeToAll(PhonoBlocksScene scene);



}
