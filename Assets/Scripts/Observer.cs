using System;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    // Singleton instance
    public static Observer Instance { get; private set; }

    // Dictionary for events with one object parameter
    private Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Subscribe(string eventName, Action<object> listener)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            eventDictionary[eventName] += listener;
        }
    }

    public void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
        }
    }

    public void Notify(string eventName, object param = null)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName]?.Invoke(param);
        }
    }
}