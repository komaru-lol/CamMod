using HarmonyLib;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace CamMod
{
	internal class NameTags
	{
		public static bool IsNameTags = false;
		public static bool IsFpsTags = false;

		private static Dictionary<VRRig, GameObject> nameTagObjects = new();
		private static Dictionary<VRRig, GameObject> fpsTagObjects = new();

		public static void EnableNameTags()
		{
			foreach (VRRig rig in GorillaParent.instance.vrrigs)
			{
				if (rig != null && !rig.isOfflineVRRig)
				{
					// --- NameTag ---
					if (IsNameTags)
					{
						if (!nameTagObjects.ContainsKey(rig))
						{
							GameObject obj = new GameObject("NameTags");
							obj.transform.SetParent(rig.transform);
							obj.transform.localPosition = Vector3.up * 0.5f;

							TextMeshPro textMeshPro = obj.AddComponent<TextMeshPro>();
							textMeshPro.text = RigManager.ReachForName(rig).NickName.ToUpper();
							textMeshPro.alignment = TextAlignmentOptions.Center;
							textMeshPro.color = GetNameColor(rig);
							textMeshPro.fontSize = 1.75f;
							textMeshPro.font = Plugin.NameTagFont;

							nameTagObjects[rig] = obj;
						}
						else
						{
							nameTagObjects[rig].transform.rotation = Quaternion.LookRotation(Plugin.Tpc.transform.forward);
						}
					}
					else if (nameTagObjects.ContainsKey(rig))
					{
						Object.Destroy(nameTagObjects[rig]);
						nameTagObjects.Remove(rig);
					}

					// --- FPS Tag ---
					if (IsFpsTags)
					{
						int fps = (int)Traverse.Create(rig).Field("fps").GetValue();
						Color color = GetFpsColor(ref fps);

						if (!fpsTagObjects.ContainsKey(rig))
						{
							GameObject obj2 = new GameObject("FpsTags");
							obj2.transform.SetParent(rig.transform);
							obj2.transform.localPosition = Vector3.up * 0.7f;

							TextMeshPro textMeshPro2 = obj2.AddComponent<TextMeshPro>();
							textMeshPro2.text = $"{fps} HZ";
							textMeshPro2.alignment = TextAlignmentOptions.Center;
							textMeshPro2.color = color;
							textMeshPro2.fontSize = 1.75f;
							textMeshPro2.font = Plugin.NameTagFont;

							fpsTagObjects[rig] = obj2;
						}
						else
						{
							TextMeshPro tmp = fpsTagObjects[rig].GetComponent<TextMeshPro>();
							tmp.text = $"{fps} HZ";
							tmp.color = color;
							fpsTagObjects[rig].transform.rotation = Quaternion.LookRotation(Plugin.Tpc.transform.forward);
						}
					}
					else if (fpsTagObjects.ContainsKey(rig))
					{
						Object.Destroy(fpsTagObjects[rig]);
						fpsTagObjects.Remove(rig);
					}
				}
			}
		}

		private static Color GetNameColor(VRRig rig)
		{
			if (rig.mainSkin.material.name.Contains("gorilla_body(Clone) (Instance)"))
				return rig.mainSkin.material.color;
			else
				return new Color(1f, 0.1f, 0f);
		}

		private static Color GetFpsColor(ref int fps)
		{
			if (fps > 58) return new Color(0f, 1f, 0f);
			if (fps > 49) return new Color(1f, 1f, 0f);
			if (fps > 45) return new Color(1f, 0.5f, 0f);
			return new Color(1f, 0f, 0f);
		}
	}
}
