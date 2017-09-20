using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public interface PhonoBlocksEvent  {

	List<Action> Subscribers();
}

public class ParameterlessEvent : PhonoBlocksEvent{
	private List<Action> subscribers;


	public void SubscribeWith(Action subscriber){
		subscribers.Add(subscriber);

	}

	public void Dispatch(){
		Dispatcher.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return subscribers;
	}

}

public class UnaryParameterizedEvent<T> : PhonoBlocksEvent{
	private List<Action<T>> typedSubscribers;
	private List<Action> generifiedSubscribers;

	public UnaryParameterizedEvent(){
		typedSubscribers = new List<Action<T>>();
	}

	public void SubscribeWith(Action<T> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Dispatch(T arg0){
		generifiedSubscribers = new List<Action>();
		foreach(Action<T> subscriber in typedSubscribers){
			generifiedSubscribers.Add(()=>subscriber(arg0));
		}
		Dispatcher.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}

}

public class BinaryParameterizedEvent<T,V> : PhonoBlocksEvent{
	private List<Action<T,V>> typedSubscribers;
	private List<Action> generifiedSubscribers;

	public BinaryParameterizedEvent(){
		typedSubscribers = new List<Action<T,V>>();
	}

	public void SubscribeWith(Action<T,V> subscriber){
		typedSubscribers.Add(subscriber);

	}

	public void Dispatch(T arg0, V arg1){
		generifiedSubscribers = new List<Action>();
		foreach(Action<T,V> subscriber in typedSubscribers){
			generifiedSubscribers.Add(()=>subscriber(arg0, arg1));
		}
		Dispatcher.Instance.EnqueueEvent(this);
	}

	public List<Action> Subscribers(){
		return generifiedSubscribers;
	}

}
	
 
