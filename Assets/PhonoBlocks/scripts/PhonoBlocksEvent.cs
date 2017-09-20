using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface PhonoBlocksEvent  {

	List<Action> Subscribers();
	string Name();
}

public class ParameterlessEvent : PhonoBlocksEvent{
	private List<Action> subscribers = new List<Action>();
	private string name;

	public ParameterlessEvent(string name){
		this.name = name;
	}

	public string Name(){
		return name;
	}

	public void Subscribe(Action subscriber){
		subscribers.Add(subscriber);

	}

	public void Fire(){
		Debug.Log($"Firing event: {name}");
		Transaction.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return subscribers;
	}

}

public class UnaryParameterizedEvent<T> : PhonoBlocksEvent{
	private List<Action<T>> typedSubscribers;
	private List<Action> generifiedSubscribers;
	string name;

	public UnaryParameterizedEvent(string name){
		this.name = name;
		typedSubscribers = new List<Action<T>>();
	}

	public string Name(){
		return name;
	}

	public void Subscribe(Action<T> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Fire(T arg0){
		Debug.Log($"Firing event: {name}");
		generifiedSubscribers = new List<Action>();
		foreach(Action<T> subscriber in typedSubscribers){
			generifiedSubscribers.Add(()=>subscriber(arg0));
		}
		Transaction.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}

}

public class BinaryParameterizedEvent<T,V> : PhonoBlocksEvent{
	private List<Action<T,V>> typedSubscribers;
	private List<Action> generifiedSubscribers;
	private string name;


	public BinaryParameterizedEvent(string name){
		typedSubscribers = new List<Action<T,V>>();
		this.name = name;
	}

	public string Name(){
		return name;
	}

	public void Subscribe(Action<T,V> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Fire(T arg0, V arg1){
		Debug.Log($"Firing event: {name}");
		generifiedSubscribers = new List<Action>();
		foreach(Action<T,V> subscriber in typedSubscribers){
			generifiedSubscribers.Add(()=>subscriber(arg0, arg1));
		}
		Transaction.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}

}
	
 
