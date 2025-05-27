using UnityEngine;

namespace CommonUnity
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _inst;
        
        public static T Inst
        {
            get
            {
                if (_inst == null)
                {
                    GameObject obj = new GameObject();
                    T t = obj.AddComponent<T>();
                    t.name = typeof(T).ToString();
                    _inst = t;
                    DontDestroyOnLoad(t);
                }

                return _inst;
            }
        }
    }
}