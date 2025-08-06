using HarmonyLib;

namespace CamMod.Patching.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("ReportTag", MethodType.Normal)]
internal class ReportTagPatcher {
    public static void Prefix(NetPlayer taggedPlayer, NetPlayer taggingPlayer) {
        var tagger = taggingPlayer?.GetPlayerRef();
        var tagged = taggedPlayer?.GetPlayerRef();

        if (tagger != null && tagged != null) {
            TagEventManager.TriggerEvent(tagger, tagged);
        }
    }
}
