using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CamMod
{
    internal class NameTags
    {
        private static Dictionary<VRRig, GameObject> nameTags = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, GameObject> fpsTags = new Dictionary<VRRig, GameObject>();

        public static void UpdateNameTags()
        {
            CleanupMissingRigs(nameTags);
            CleanupMissingRigs(fpsTags);

            if (GorillaParent.instance == null) return;

            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig) continue;

                if (!nameTags.ContainsKey(rig))
                    CreateNameTag(rig);
                else
                    UpdateNameTag(rig);

                if (Plugin.IsFpsTags)
                {
                    if (!fpsTags.ContainsKey(rig))
                        CreateFPSTag(rig);
                    else
                        UpdateFPSTag(rig);
                }
                else
                {
                    if (fpsTags.TryGetValue(rig, out var fpsObj))
                    {
                        Object.Destroy(fpsObj);
                        fpsTags.Remove(rig);
                    }
                }
            }
        }

        private static void CreateNameTag(VRRig rig)
        {
            Color nameColor = rig.mainSkin.material.name.Contains("gorilla_body(Clone) (Instance)")
                ? rig.mainSkin.material.color
                : new Color(1f, 0.1f, 0f);

            GameObject nameTagObj = new GameObject("NameTag");
            nameTagObj.transform.SetParent(rig.transform);
            nameTagObj.transform.localPosition = Vector3.up * 0.5f;
            nameTagObj.transform.localScale = Vector3.one;

            TextMeshPro tmp = nameTagObj.AddComponent<TextMeshPro>();
            tmp.enableAutoSizing = false;
            tmp.fontSize = 1.75f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = nameColor;
            tmp.font = Plugin.NameTagFont;
            tmp.text = rig.playerNameVisible;

            nameTags[rig] = nameTagObj;

            UpdateNameTag(rig);
        }

        private static void UpdateNameTag(VRRig rig)
        {
            if (!nameTags.TryGetValue(rig, out GameObject nameTagObj) || nameTagObj == null) return;

            TextMeshPro tmp = nameTagObj.GetComponent<TextMeshPro>();
            if (tmp == null) return;

            tmp.color = rig.mainSkin.material.name.Contains("gorilla_body(Clone) (Instance)")
                ? rig.mainSkin.material.color
                : new Color(1f, 0.1f, 0f);

            tmp.text = rig.playerNameVisible;

            if (Plugin.TpcObject != null && Plugin.TpcObject.transform != null)
            {
                Quaternion.LookRotation(Plugin.Tpc.transform.forward);
            }
        }

        private static void CreateFPSTag(VRRig rig)
        {
            GameObject fpsTagObj = new GameObject("FpsTag");
            fpsTagObj.transform.SetParent(rig.transform);
            fpsTagObj.transform.localPosition = Vector3.up * 0.7f;
            fpsTagObj.transform.localScale = Vector3.one;

            TextMeshPro tmp = fpsTagObj.AddComponent<TextMeshPro>();
            tmp.fontSize = 1.75f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = Plugin.NameTagFont;

            fpsTags[rig] = fpsTagObj;

            UpdateFPSTag(rig);
        }

        private static void UpdateFPSTag(VRRig rig)
        {
            if (!fpsTags.TryGetValue(rig, out GameObject fpsTagObj) || fpsTagObj == null) return;

            TextMeshPro tmp = fpsTagObj.GetComponent<TextMeshPro>();
            if (tmp == null) return;

            int fps = (int)Traverse.Create(rig).Field("fps").GetValue();
            tmp.text = $"{fps} HZ";
            tmp.color = GetFpsColor(fps);

            if (Plugin.TpcObject != null && Plugin.TpcObject.transform != null)
            {
                Quaternion.LookRotation(Plugin.Tpc.transform.forward);
            }
        }

        private static void CleanupMissingRigs<T>(Dictionary<VRRig, T> dict) where T : Object
        {
            if (GorillaParent.instance == null) return;

            List<VRRig> toRemove = new List<VRRig>();
            foreach (var kvp in dict)
            {
                if (kvp.Key == null || !GorillaParent.instance.vrrigs.Contains(kvp.Key))
                {
                    if (kvp.Value != null)
                        Object.Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in toRemove)
                dict.Remove(rig);
        }

        private static Color GetFpsColor(int fps)
        {
            if (fps > 58) return Color.green;
            if (fps > 49) return Color.yellow;
            if (fps > 45) return new Color(1f, 0.5f, 0f);
            return Color.red;
        }
    }
}
