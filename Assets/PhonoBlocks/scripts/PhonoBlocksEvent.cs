using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface PhonoBlocksEvent  {

	IEnumerator<Action> Subscribers();
	string Name();
	void ClearSubscribers();
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
		subscribers[subscriber]=handler;

	}

	public void Unsubscribe(PhonoBlocksSubscriber subscriber){
		if(subscribers.ContainsKey(subscriber)){
			subscribers.Remove(subscriber);
		}
	}

	public void Fire(){
		Debug.Log($"Firing event: {name}");
		Transaction.Instance.EnqueueEvent(this);
	}

	public IEnumerator<Action> Subscribers(){
		return subscribers.Values.GetEnumerator();
	}


	public void ClearSubscribers(){
		subscribers.Clear();
	}

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
		subscribers[subscriber] = handler;
	}

	public void ClearSubscribers(){
		subscribers.Clear();
	}

	public void Unsubscribe(PhonoBlocksSubscriber subscriber){
		if(subscribers.ContainsKey(subscriber)){
			subscribers.Remove(subscriber);
		}
	}

	public void Fire(T arg0){
		Debug.Log($"Firing event: {name}");
	
		generifiedSubscribers = new List<Action>();
		foreach(Action<T> subscriber in subscribers.Values){
			generifiedSubscribers.Add(()=>subscriber(arg0));
		}
		Transaction.Instance.EnqueueEvent(this);
	}


	public IEnumerator<Action> Subscribers(){
		return generifiedSubscribers.GetEnumerator();
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
		subscribers[subscriber] = handler;

	}

	public void Unsubscribe(PhonoBlocksSubscriber subscriber){
		if(subscribers.ContainsKey(subscriber)){
			subscribers.Remove(subscriber);
		}
	}

	public void Fire(T arg0, V arg1){
		Debug.Log($"Firing event: {name}");
		generifiedSubscribers = new List<Action>();
		foreach(Action<T,V> subscriber in subscribers.Values){
			generifiedSubscribers.Add(()=>subscriber(arg0, arg1));
		}
		Transaction.Instance.EnqueueEvent(this);
	}

	public IEnumerator<Action> Subscribers(){
		return generifiedSubscribers.GetEnumerator();
	}

	public void ClearSubscribers(){
		subscribers.Clear();
		generifiedSubscribers.Clear();
	}

}
	
 
