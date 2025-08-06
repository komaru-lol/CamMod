using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace CamMod;

internal static class TagEventManager {
    public static event TagEventHandler OnTagEvent;

    public static void TriggerEvent(Player tagger, Player tagged) {
        if (ShouldLog()) {
            Debug.Log($"[TagEventManager] {tagger.NickName} tagged {tagged.NickName}");
        }
        OnTagEvent?.Invoke(tagger, tagged);
    }

    public static void OnEvent(EventData data) {
        if (data.Code == 1 || data.Code == 2) {
            if (data.Parameters.TryGetValue(ParameterCode.Data, out object rawContent)) {
                if (rawContent is object[] contentArray && contentArray.Length >= 2) {
                    int taggerActor = Convert.ToInt32(contentArray[0]);
                    int taggedActor = Convert.ToInt32(contentArray[1]);

                    Player tagger = PhotonNetwork.CurrentRoom?.GetPlayer(taggerActor);
                    Player tagged = PhotonNetwork.CurrentRoom?.GetPlayer(taggedActor);

                    if (tagger != null && tagged != null) {
                        TriggerEvent(tagger, tagged);
                    } else {
                        Debug.LogWarning($"[TagEventManager] Failed to resolve players from actor numbers: {taggerActor}, {taggedActor}");
                    }
                } else {
                    Debug.LogWarning("[TagEventManager] Invalid eventContent format (not object[] with 2 entries)");
                }
            } else {
                Debug.LogWarning("[TagEventManager] ParameterCode.Data not found in event");
            }
        }
    }

    public delegate void TagEventHandler(Player tagger, Player tagged);
    
    private static float lastLogTime = 0f;
    private const float logCooldown = 0.1f;

    private static bool ShouldLog()
    {
        float t = UnityEngine.Time.time;
        if (t - lastLogTime > logCooldown)
        {
            lastLogTime = t;
            return true;
        }
        return false;
    }
}
