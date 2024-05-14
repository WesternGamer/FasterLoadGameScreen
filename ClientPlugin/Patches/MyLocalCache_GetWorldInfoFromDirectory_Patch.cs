using System.Collections.Generic;
using System;
using HarmonyLib;
using Sandbox.Engine.Networking;
using Sandbox.Game.World;
using System.IO;
using System.Threading.Tasks;

namespace ClientPlugin
{
    [HarmonyPatch(typeof(MyLocalCache))]
    public static class MyLocalCache_GetWorldInfoFromDirectory_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetWorldInfoFromDirectory")]
        private static bool GetWorldInfoFromDirectoryPrefix(string path, List<Tuple<string, MyWorldInfo>> result)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            Parallel.ForEach(new DirectoryInfo(path).GetDirectories(),
            delegate (DirectoryInfo d)
            {
                MyWorldInfo worldInfo = MyLocalCache.LoadWorldInfoFromFile(d.FullName);

                string sessionPath = d.FullName;
                if (worldInfo != null)
                {
                    if (string.IsNullOrEmpty(worldInfo.SessionName))
                    {
                        worldInfo.SessionName = Path.GetFileName(d.FullName);
                    }

                    
                    if (!string.IsNullOrEmpty(worldInfo.SessionPath))
                    {
                        sessionPath = worldInfo.SessionPath;
                    }
                }

                result.Add(Tuple.Create(sessionPath, worldInfo));
            });

            return false;
        }
    }
}
