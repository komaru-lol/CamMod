using System;
using System.Threading;
using Discord;
using Photon.Pun;
using UnityEngine;

namespace CamMod;

internal class RpcManager : MonoBehaviour
{
    private void Start()
    {
        Thread.Sleep(5000);
        discord = new global::Discord.Discord(applicationID, (ulong)Discord.CreateFlags.Default);
        time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private void Update()
    {
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void LateUpdate()
    {
        Discord.ActivityManager activityManager = discord.GetActivityManager();
       
        activity = new Activity()
        {
            Details = "Using Serenity's Camera Mod",
            State = "https://discord.gg/SWzPcbFZKj"
        };
        activity.Assets = new ActivityAssets()
        {
            LargeImage = "embedded_cover",
            LargeText = "https://discord.gg/SWzPcbFZKj",
            SmallImage = "embedded_cover",
            SmallText = "https://discord.gg/SWzPcbFZKj",
        };
        activity.Timestamps = new ActivityTimestamps()  
        {
            Start = time
        };
        activityManager.UpdateActivity(activity, delegate(Result result){});
    }
    
    internal long applicationID = 1345426711373807706L;
    
    internal string details;

    internal string largeImage;
    
    internal string smallImage;
    
    internal long time;
    
    internal global::Discord.Discord discord;
    
    internal Discord.Activity activity;
    
    public static int PlayerCount;
}