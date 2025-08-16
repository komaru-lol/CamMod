using System.Reflection;
using HarmonyLib;

namespace CamMod.Patching {
    internal class HarmonyPatches
    {
        private static Harmony _instance = null;

        private static bool IsPatched { get; set; }

        internal static void ApplyPatches()
        {
            if (!IsPatched)
            {
                if (_instance == null)
                {
                    _instance = new Harmony(PluginInfo.Guid);
                }
                _instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void Unpatch()
        {
            if (IsPatched && _instance != null)
            {
                _instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}

