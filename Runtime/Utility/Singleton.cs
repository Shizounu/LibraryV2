using UnityEngine;

namespace Shizounu.Library.Utility
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T InternalInstance; 
        public static T Instance {
            get { 
                InternalInstance ??= new T();
                return InternalInstance;
            }
        }
    }
}
