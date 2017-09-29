using UnityEngine;
using UnityEngine.SceneManagement;
public abstract class PhonoBlocksSubscriber : MonoBehaviour {

	public abstract void SubscribeToAll(PhonoBlocksScene scene);


	//implement on destroy. unsubscribe. 
	//but from what?
	//from all the events that we carre about... oi.
}
