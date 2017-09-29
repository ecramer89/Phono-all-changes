using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubscriptionManager : MonoBehaviour {

	void Awake(){
		SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => {
			//safe casting with generics:
			//as will check whether a cast can be safely achieved
			//if it can, then it will return the value with the desired cast
			//otherwise, it returns null
			PhonoBlocksSubscriber[] activeSubscribers = FindObjectsOfType(typeof(PhonoBlocksSubscriber)) as PhonoBlocksSubscriber[];
			if(activeSubscribers == null) return;  //check safety of cast
				foreach(PhonoBlocksSubscriber subscriber in activeSubscribers){
					subscriber.Subscribe();
					subscriber.IsSubscribed();
					subscriber.Unsubscribe();
			
			}
		
		
		};

	}

}
