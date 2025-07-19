using UnityEngine;

namespace CamMod
{
    internal class GUIUtils
    {
       public static void DrawTexture(
            Rect rect,
            Texture2D texture,
            int borderRadius,
            Vector4 borderRadius4 = default (Vector4))
        {
            if (borderRadius4 == Vector4.zero)
                borderRadius4 = new Vector4((float) borderRadius, (float) borderRadius, (float) borderRadius, (float) borderRadius);
            GUI.DrawTexture(rect, (Texture) texture, ScaleMode.StretchToFill, false, 0.0f, GUI.color, Vector4.zero, borderRadius4);
        }
    
        public static float RoundedSlider(
            float value,
            float min,
            float max,
            Texture2D backgroundTex,
            Texture2D fillTex,
            int radius = 6,
            Rect? manualRect = null,
            params GUILayoutOption[] options)
        {
            Rect rect = manualRect ?? GUILayoutUtility.GetRect(0.0f, 12f, options);
            DrawTexture(rect, backgroundTex, radius);
            float num = Mathf.InverseLerp(min, max, value);
            DrawTexture(new Rect(rect.x, rect.y, rect.width * num, rect.height), fillTex, radius);
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && rect.Contains(Event.current.mousePosition))
            {
                value = Mathf.Clamp(Mathf.Lerp(min, max, (Event.current.mousePosition.x - rect.x) / rect.width), min, max);
                Event.current.Use();
            }
            return value;
        }
    }
}