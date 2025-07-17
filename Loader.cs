using CamMod;
using TMPro;
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
            Plugin.EnsureDefaultConfig();
            Plugin.LoadSettings();
            Plugin.NameTagFont = TMP_FontAsset.CreateFontAsset(Plugin.CreateFont("CamMod.Assets.nametagfont.ttf"));
        }
    }
}