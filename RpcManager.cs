using System;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Threading;
using Photon.Pun;
using UnityEngine;

namespace CamMod
{
    internal class RpcManager
    {
        private static Assembly rpcAssembly;
    
        public static void Init()
        {
            rpcAssembly = Assembly.Load(LoadEmbeddedResource("CamMod.Assets.DiscordRPC.dll"));
            Type type = rpcAssembly.GetType("DiscordRPC.DiscordRpcClient");
            object obj = Activator.CreateInstance(type, new object[] { "1345426711373807706" });
            type.GetMethod("Initialize").Invoke(obj, null);
            Type type2 = rpcAssembly.GetType("DiscordRPC.RichPresence");
            object obj2 = Activator.CreateInstance(type2);
            type2.GetProperty("Details").SetValue(obj2, "Using Serenity's Camera Mod");
            type2.GetProperty("State").SetValue(obj2, "https://discord.gg/SWzPcbFZKj");
            Type type3 = rpcAssembly.GetType("DiscordRPC.Assets");
            object obj3 = Activator.CreateInstance(type3);
            type3.GetProperty("LargeImageKey").SetValue(obj3, "embedded_cover");
            type3.GetProperty("LargeImageText").SetValue(obj3, "Serenity's Camera Mod");
            type2.GetProperty("Assets").SetValue(obj2, obj3);
            type.GetMethod("SetPresence").Invoke(obj, new object[] { obj2 });
        }

        private static byte[] LoadEmbeddedResource(string resourceName)
        {
            byte[] array;
            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (manifestResourceStream == null)
                {
                    throw new ArgumentException("Resource '" + resourceName + "' not found.");
                }
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    manifestResourceStream.CopyTo(memoryStream);
                    array = memoryStream.ToArray();
                }
            }
            return array;
        }
    }
}
