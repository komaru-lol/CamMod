/*using HarmonyLib;
using UnityEngine;

namespace CamMod.Patching.Patches;

[HarmonyPatch(typeof(VRMap), "MapOther")]
internal class RigSexPatch
{
    public static float lerper = 1.2f;

    private static bool Prefix(VRMap __instance, float lerpValue)
    {
        if (__instance == null || __instance.rigTarget == null || Time.deltaTime <= 0f)
            return true;
        Vector3 offset = __instance.syncPos - __instance.rigTarget.localPosition;
        Vector3 targetPosition = __instance.rigTarget.localPosition + offset * lerper;
        __instance.rigTarget.localPosition = Vector3.Lerp(__instance.rigTarget.localPosition, targetPosition, lerpValue);
        __instance.rigTarget.localRotation = Quaternion.Lerp(__instance.rigTarget.localRotation, __instance.syncRotation, lerpValue);
        return false;
    }
}*/