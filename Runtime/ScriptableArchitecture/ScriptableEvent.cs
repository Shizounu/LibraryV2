using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Shizounu.Library.ScriptableArchitecture
{
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "Shizounu/ScriptableArchitecture/ScriptableEvent", order = -1)]
    public class ScriptableEvent : ScriptableObject 
    {
        private readonly List<Action> _eventListeners = new();

        public void Invoke()
        {
            for (int i = 0; i < _eventListeners.Count; i++)
            {
                _eventListeners[i].Invoke();
            }
        }

        public void RegisterListener(Action listener)
        {
            if (!_eventListeners.Contains(listener))
            {
                _eventListeners.Add(listener);
            }
        }
        
        public void RemoveListener(Action listener)
        {
            if (_eventListeners.Contains(listener))
            {
                _eventListeners.Remove(listener);
            }
        }

        public static ScriptableEvent operator +(ScriptableEvent thisEvent, Action listener)
        {
            thisEvent.RegisterListener(listener);
            return thisEvent;
        }
        
        public static ScriptableEvent operator -(ScriptableEvent thisEvent, Action listener)
        {
            thisEvent.RemoveListener(listener);
            return thisEvent;
        }
    }
}