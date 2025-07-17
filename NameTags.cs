using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CamMod
{
	internal class NameTags
	{
		public static bool IsNameTags = false;
		public static bool IsFpsTags = false;

		public static void EnableNameTags()
		{
			if (IsNameTags)
			{
				foreach (VRRig rig in GorillaParent.instance.vrrigs)
				{
					if (rig != null && rig != rig.isOfflineVRRig)
					{
						Color color = rig.mainSkin.material.name.Contains("gorilla_body(Clone) (Instance)")
							? rig.mainSkin.material.color
							: new Color(1f, 0.1f, 0f);

						GameObject obj = new GameObject("NameTags");
						obj.transform.SetParent(rig.transform);
						obj.transform.localPosition = Vector3.up * 0.5f;
						TextMeshPro textMeshPro = obj.AddComponent<TextMeshPro>();
						textMeshPro.text = RigManager.ReachForName(rig).NickName.ToUpper();
						textMeshPro.alignment = TextAlignmentOptions.Center;
						textMeshPro.color = color;
						textMeshPro.fontSize = 1.75f;
						textMeshPro.font = Plugin.NameTagFont;
						obj.transform.rotation = UnityEngine.Quaternion.LookRotation(Plugin.Tpc.transform.forward);
						Object.Destroy(obj, Time.deltaTime);
						
						if (IsFpsTags)
						{
							int fps = (int)Traverse.Create(rig).Field("fps").GetValue();
							Color color2 = GetFpsColor(ref fps );

							GameObject obj2 = new GameObject("fpsTags");
							obj2.transform.SetParent(rig.transform);
							obj2.transform.localPosition = Vector3.up * 0.7f;
							TextMeshPro textMeshPro2 = obj2.AddComponent<TextMeshPro>();
							textMeshPro2.text = $"{fps.ToString()} HZ";
							textMeshPro2.alignment = TextAlignmentOptions.Center;
							textMeshPro2.color = color2;
							textMeshPro2.fontSize = 1.75f;
							textMeshPro2.font = Plugin.NameTagFont;
							obj2.transform.rotation = UnityEngine.Quaternion.LookRotation(Plugin.Tpc.transform.forward);
							Object.Destroy(obj2, Time.deltaTime);
						}
						else
						{
							IsFpsTags = false;
						}
					}
				}
			}
			else
			{
				IsNameTags = false;
			}
		}
		
		public static void EnableFpsTags()
		{
			
		}

		private static Color GetFpsColor(ref int fps)
		{
			Color fpsColor;
			if (fps > 58)
			{
				fpsColor = new Color(0f, 1f, 0f);
			}
			else if (fps > 49)
			{
				fpsColor = new Color(1f, 1f, 0f);
			}
			else if (fps > 45)
			{
				fpsColor = new Color(1f, 0.5f, 0f);
			}
			else
			{
				fpsColor = new Color(1f, 0f, 0f);
			}
			
			return fpsColor;
		}
	}
}

