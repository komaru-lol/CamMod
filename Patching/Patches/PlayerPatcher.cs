/*using System;
using System.Reflection;
using BepInEx;
using GorillaGameModes;
using GorillaLocomotion;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;

namespace CamMod.Patching.Patches {
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    internal class PlayerPatcher : MonoBehaviour {
        public static bool TagAll;
        public static bool TagMe;
        public static VRRig TagHim;
        
        public static readonly quaternion DefaultRotation = quaternion.Euler(0f, 0f, 0f, math.RotationOrder.ZXY);

        [HarmonyPrefix]
        private static void Prefix() {
            if (!Plugin.IsSetup)
                return;
            
            // HandleTagAll();
           // HandleTagHim();
          //  HandleTagMe();
        }

        private static void HandleTagAll() {
            if (!TagAll) return;

            bool allInfected = true;

            foreach (var rig in GorillaParent.instance.vrrigs) {
                if (!rig.mainSkin.material.name.Contains("infected (Instance)")) {
                    allInfected = false;
                    break;
                }
            }

            if (allInfected) {
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                foreach (var rig in GorillaParent.instance.vrrigs) {
                    if (!rig.mainSkin.material.name.Contains("infected (Instance)")) {
                        var offset = rig.transform.position + Vector3.up * 3f;
                        GorillaTagger.Instance.offlineVRRig.transform.position = offset;
                        RigManager.GetVRRigFromPlayer(GorillaTagger.Instance.myVRRig.Owner).transform.position = offset;
                        GTPlayer.Instance.rightControllerTransform.position = rig.transform.position;
                    }
                }
            }
            
            if (!GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("infected (Instance)")) {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                TagAll = false;
            }
        }

        private static void HandleTagHim() {
            if (TagHim == null) return;

            bool canTag = !TagHim.mainSkin.material.name.Contains("infected (Instance)")
                          && TagHim != GorillaTagger.Instance.offlineVRRig
                          && GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("infected (Instance)");

            if (canTag) {
                var pos = TagHim.transform.position - new Vector3(0f, 3f, 0f);
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = pos;
                if (ValidateTag(TagHim)) {
                    ReportTag(TagHim);
                }
            } else {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                TagHim = null;
            }
        }
        
        private static float reportTagDelay;

        private static void ReportTag(VRRig rig)
        {
            if (Time.time > reportTagDelay)
            {
                reportTagDelay = Time.time + 0.1f;
                GameMode.ReportTag(RigManager.GetPlayerFromVRRig(rig));
            }
        }

        private static void HandleTagMe() {
            if (!TagMe) return;

            bool isInfected = GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("infected (Instance)");

            if (isInfected) {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                var closest = RigManager.GetClosestTagger(GorillaTagger.Instance.offlineVRRig);
                if (closest != null) {
                    GorillaTagger.Instance.offlineVRRig.transform.position =
                        closest.rightHandTransform.position;
                }
            } else {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                TagMe = false;
            }
        }
        
        public static bool ValidateTag(VRRig Rig) =>
            Vector3.Distance(ServerSyncPos, Rig.transform.position) < 6f;
        
        public static Vector3 ServerSyncPos;
        public static Vector3 ServerSyncLeftHandPos;
        public static Vector3 ServerSyncRightHandPos;

        public static Vector3 ServerPos;
        public static Vector3 ServerLeftHandPos;
        public static Vector3 ServerRightHandPos;

        public static void OnSerialize()
        {
            ServerSyncPos = VRRig.LocalRig?.transform.position ?? ServerSyncPos;
            ServerSyncLeftHandPos = VRRig.LocalRig?.leftHand.rigTarget.transform.position ?? ServerSyncLeftHandPos;
            ServerSyncRightHandPos = VRRig.LocalRig?.rightHand.rigTarget.transform.position ?? ServerSyncRightHandPos;
        }
    }
}*/