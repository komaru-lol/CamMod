using GorillaNetworking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CamMod;

internal static class ToggleMic 
{
    private static bool ispttType = true;

    public static void SetupMicCanvas() {
        GorillaComputer.instance.pttType = "ALL CHAT";
        GameObject gameObject = new GameObject("Canvas");
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
        text = gameObject.AddComponent<Text>();
        text.text = "Mic active";
        text.fontSize = 15;
        text.color = Color.green;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.LowerLeft;
    }
    
    public static void SetupMicToggle() {
        if (Keyboard.current.tKey.wasPressedThisFrame) {
           OnVoiceToggle(); 
        }

        if (GorillaComputer.instance.pttType == "ALL CHAT") {
            text.text = "Mic active";
            text.color = Color.green;
        }
        else {
            text.text = "Mic Muted";
            text.color = Color.red;
        }
    }

    private static void OnVoiceToggle() {
        ispttType = !ispttType;
        GorillaComputer.instance.pttType = (ispttType ? "ALL CHAT" : "PUSH TO TALK");
        Debug.Log("Push to talk: " + (ispttType ? "ALL CHAT" : "PUSH TO TALK"));
        
        if (GorillaComputer.instance.pttType == "ALL CHAT") {
            text.text = "Mic active";
            text.color = Color.green;
        }
        else {
            text.text = "Mic Muted";
            text.color = Color.red;
        }
    }

    private static Text text;
}