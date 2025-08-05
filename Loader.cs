using CamMod;
using UnityEngine;

namespace LoadThing
{
    public class Loader
    { 
        private static GameObject _loaderObject;
        
        public static void Load()
        {
            _loaderObject = new GameObject();
            _loaderObject.AddComponent<Plugin>();
            Object.DontDestroyOnLoad(_loaderObject);            
        }
    }
}