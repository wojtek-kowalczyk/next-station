using UnityEngine;

namespace NS.Core
{
    public abstract class LocalSingleton<T> : MonoBehaviour where T : LocalSingleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"An instance of {typeof(T).Name} already exists. This one will not be accessible.", gameObject);
                return;
            }

            Instance = (T)this;
        }
    }
}
