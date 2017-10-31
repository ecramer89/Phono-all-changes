using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface PhonoBlocksEvent  {

	IEnumerator<Action> SubscriptionHandlers();
	IEnumerable<PhonoBlocksSubscriber> Subscribers();
	string Name();
	void ClearSubscribers();

}


public class UnaryParameterizedEvent<T> : PhonoBlocksEvent{
	private Dictionary<PhonoBlocksSubscriber, Action<T>> subscribers = new Dictionary<PhonoBlocksSubscriber, Action<T>>();
	private List<Action> generifiedSubscribers;
	string name;

	public UnaryParameterizedEvent(string name){
		this.name = name;
	}



	public string Name(){
		return name;
	}

	public void Subscribe(PhonoBlocksSubscriber subscriber, Action<T> handler){
		if(subscribers.ContainsKey(subscriber)) throw new Exception($"Warning!!! Subscriber: {subscriber.GetType()} is attempting to subscribe multiple handlers to event {Name()}. Please check the subscribe to all method of this subscriber and combine the multiple event handlers into one.");
		subscribers[subscriber] = handler;
	}

	public void ClearSubscribers(){
		subscribers = new Dictionary<PhonoBlocksSubscriber, Action<T>>();
		generifiedSubscribers = null;
	}


	public void Fire(T arg0){
		Debug.Log($"Firing event: {name}");

		generifiedSubscribers = new List<Action>();
		foreach(Action<T> subscriber in subscribers.Values){
			generifiedSubscribers.Add(()=>subscriber(arg0));
		}
		Transaction.Instance.EnqueueEvent(this);
	}

	public IEnumerable<PhonoBlocksSubscriber> Subscribers(){
		return subscribers.Keys;
	}


	public IEnumerator<Action> SubscriptionHandlers(){
		return generifiedSubscribers.GetEnumerator();
	}

}


public class ParameterlessEvent : PhonoBlocksEvent{
	Dictionary<PhonoBlocksSubscriber, Action> subscribers = new Dictionary<PhonoBlocksSubscriber, Action>();
	private string name;


	public ParameterlessEvent(string name){
		this.name = name;
	}

	public string Name(){
		return name;
	}

	public void Subscribe(PhonoBlocksSubscriber subscriber, Action handler){
		if(subscribers.ContainsKey(subscriber)) throw new Exception($"Warning!!! Subscriber: {subscriber.GetType()} is attempting to subscribe multiple handlers to event {Name()}. Please check the subscribe to all method of this subscriber and combine the multiple event handlers into one.");
		subscribers[subscriber]=handler;

	}


	public void Fire(){
		Debug.Log($"Firing event: {name}");
		Transaction.Instance.EnqueueEvent(this);
	}

	public IEnumerator<Action> SubscriptionHandlers(){
		return subscribers.Values.GetEnumerator();
	}

	public IEnumerable<PhonoBlocksSubscriber> Subscribers(){
		return subscribers.Keys;
	}


	public void ClearSubscribers(){
		subscribers= new Dictionary<PhonoBlocksSubscriber, Action>();

	}

}


public class BinaryParameterizedEvent<T,V> : PhonoBlocksEvent{
	private Dictionary<PhonoBlocksSubscriber, Action<T,V>> subscribers = new Dictionary<PhonoBlocksSubscriber, Action<T,V>>();
	private List<Action> generifiedSubscribers;
	private string name;


	public BinaryParameterizedEvent(string name){
		this.name = name;
	}

	public string Name(){
		return name;
	}

	public void Subscribe(PhonoBlocksSubscriber subscriber, Action<T,V> handler){
		if(subscribers.ContainsKey(subscriber)) throw new Exception($"Warning!!! Subscriber: {subscriber.GetType()} is attempting to subscribe multiple handlers to event {Name()}. Please check the subscribe to all method of this subscriber and combine the multiple event handlers into one.");
		subscribers[subscriber] = handler;

	}


	public void Fire(T arg0, V arg1){
		Debug.Log($"Firing event: {name}");
		generifiedSubscribers = new List<Action>();
		foreach(Action<T,V> subscriber in subscribers.Values){
			generifiedSubscribers.Add(()=>subscriber(arg0, arg1));
		}
		Transaction.Instance.EnqueueEvent(this);
	}

	public IEnumerator<Action> SubscriptionHandlers(){
		return generifiedSubscribers.GetEnumerator();
	}

	public IEnumerable<PhonoBlocksSubscriber> Subscribers(){
		return subscribers.Keys;
	}

	public void ClearSubscribers(){
		subscribers = new Dictionary<PhonoBlocksSubscriber, Action<T,V>>();
		generifiedSubscribers = null;
	}

}

//UndoablebinaryParam...
//UndoableParameterless...
//this is a classic case for decorator but it is hard to wrap the subscribe method of the two different wrapped classes
//becuase they differ wrt the NUBER of generic parameters that are neded for the argument to subscribe
	
/*mightbe an interesting problem for c# user group-
	composition
	have events, they difer wrt the type and number of params
	wnted to add in undo funcitonality
	some events are undoable
	wanted to force subscribers of undoable events to have to provide a
	handler for 'undo'
		the undoable 'wrpaper; would impelemtn the phone blocks even tinterface'
		and basiclaly delegate to it, it would wrap the subscribe method
		and force clients to produce the arguments expected of the wrapped evnet
		and also an extra onewhich is an undi handler
		then it would delegate to the wrappee
		and also save the undo handler in its own list.
		it exoses a public undo method
		when called
		it invokes the undo handler of each subscriber.*/
 
