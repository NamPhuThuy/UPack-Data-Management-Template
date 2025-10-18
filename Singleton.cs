using UnityEngine;
namespace NamPhuThuy.Data
{
    /// <summary>
    /// Easy to use, auto-creates instances, and is suitable for simple cases.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        
        public static T Ins
        {
            get
            {
                if (_instance == null)
                {
                    T[] objs = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (objs.Length > 0)
                    {
                        T instance = objs[0];
                        _instance = instance;
                    }
                    else
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(T).Name;
                        _instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }

                }

                return _instance;
            }
        }
        
        protected void Awake()
        {
            if (!_instance)
            {
                // Debug.Log($"Singleton<{typeof(T).Name}> Awake");
                _instance = Object.FindFirstObjectByType<T>();
                OnInitialization();
            }
        }
        
        public virtual void OnDestroy()
        {
            _instance = null;
            OnExtinction();
        }
        
        public virtual void OnInitialization() { }
        public virtual void OnExtinction() { }
    }
}