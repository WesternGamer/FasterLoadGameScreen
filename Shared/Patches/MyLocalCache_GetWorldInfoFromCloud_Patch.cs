using HarmonyLib;
using Sandbox.Engine.Networking;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VRage.GameServices;
using VRage.Utils;

namespace Shared.Patches
{
    [HarmonyPatch(typeof(MyLocalCache))]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class MyLocalCache_GetWorldInfoFromCloud_Patch
    {
        private static MethodInfo loadWorldInfoFromCloudMethod = AccessTools.Method(typeof(MyLocalCache), "LoadWorldInfoFromCloud");

        [HarmonyPrefix]
        [HarmonyPatch("GetWorldInfoFromCloud")]
        private static bool GetWorldInfoFromCloudPrefix(string path, List<Tuple<string, MyWorldInfo>> result)
        {
            List<MyCloudFileInfo> cloudFiles = MyGameService.GetCloudFiles(MyLocalCache.GetSessionSavesPath(path, false, false, true));

            if (cloudFiles == null)
            {
                return false;
            }

            Parallel.ForEach(cloudFiles,
            delegate (MyCloudFileInfo fileInfo)
            {
                if (!fileInfo.Name.EndsWith("Sandbox.sbc", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string text = fileInfo.Name.Replace("Sandbox.sbc", "", StringComparison.OrdinalIgnoreCase);
                MyWorldInfo myWorldInfo = (MyWorldInfo)loadWorldInfoFromCloudMethod.Invoke(null, new object[] { text });

                if (myWorldInfo != null)
                {
                    if (string.IsNullOrEmpty(myWorldInfo.SessionName))
                    {
                        myWorldInfo.SessionName = Path.GetFileName(text);
                    }
                }
                
                result.Add(Tuple.Create(text, myWorldInfo));
            });

            return false;
        }
    }
}
