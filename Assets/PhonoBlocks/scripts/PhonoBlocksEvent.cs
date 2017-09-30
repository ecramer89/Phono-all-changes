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



//dictionaries shold be turned back into lists asap
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
		//Debug.Log($"Firing event: {name}");
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
	
 
