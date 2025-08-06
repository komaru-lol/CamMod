using System;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace CamMod.Patching.Patches;

[HarmonyPatch(typeof(PhotonNetwork), "RaiseEvent", new Type[]
{
    typeof(byte),
    typeof(object),
    typeof(RaiseEventOptions),
    typeof(SendOptions)
})]
internal class EventPatch
{
    public static void Postfix(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
    {
        if (eventCode != 1 && eventCode != 2)
            return;

        if (eventContent is object[] contentArray && contentArray.Length >= 2)
        {
            int taggerActor = Convert.ToInt32(contentArray[0]);
            int taggedActor = Convert.ToInt32(contentArray[1]);

            Player tagger = PhotonNetwork.CurrentRoom?.GetPlayer(taggerActor);
            Player tagged = PhotonNetwork.CurrentRoom?.GetPlayer(taggedActor);

            if (tagger != null && tagged != null)
            {
                TagEventManager.TriggerEvent(tagger, tagged);
            }
        }
    }
}
