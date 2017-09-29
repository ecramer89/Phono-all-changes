using UnityEngine;

public abstract class PhonoBlocksSubscriber : MonoBehaviour {

	public abstract void Subscribe();
	public abstract void Unsubscribe();
	public abstract bool IsSubscribed();
}
