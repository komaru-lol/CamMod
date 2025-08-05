using UnityEngine;

namespace CamMod
{
    public class RectMat 
    {
        public static void DrawParallelogram(float width, float height, float skewOffset, Texture2D bg) {
            if (bg == null)
                return;
            Rect layoutRect = GUILayoutUtility.GetRect(width, height);
            Matrix4x4 originalMatrix = GUI.matrix;
            Matrix4x4 skewMatrix = Matrix4x4.identity;
            skewMatrix.m00 = 1;
            skewMatrix.m01 = skewOffset / height;
            skewMatrix.m11 = 1;
            GUI.matrix = Matrix4x4.TRS(new Vector3(layoutRect.x, layoutRect.y, 0), Quaternion.identity, Vector3.one) * skewMatrix;
            GUI.DrawTexture(new Rect(0, 0, width, height), bg, ScaleMode.StretchToFill);
            GUI.matrix = originalMatrix;
        }
        
        public static Texture2D MakeColorFromTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }
    }
}
