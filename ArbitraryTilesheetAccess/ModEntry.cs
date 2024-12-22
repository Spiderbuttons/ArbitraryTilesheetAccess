using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Framework.ContentManagers;
using StardewValley.Extensions;

namespace ArbitraryTilesheetAccess
{
    internal sealed class ModEntry : Mod
    {
        private static IMonitor ModMonitor { get; set; } = null!;
        private static Harmony Harmony { get; set; } = null!;

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Harmony = new Harmony(ModManifest.UniqueID);

            Harmony.Patch(original: AccessTools.Method(typeof(ModContentManager), nameof(ModContentManager.GetContentKeyForTilesheetImageSource)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModContentManager_GetContentKeyForTilesheetImageSource_Prefix)));
        }
        
        private static bool ModContentManager_GetContentKeyForTilesheetImageSource_Prefix(string relativePath, ref string __result)
        {
            try
            {
                string key = relativePath;
                string[] segments = PathUtilities.GetSegments(key);
                if (segments.Length <= 1 || !segments[0].EqualsIgnoreCase("Content"))
                {
                    return true;
                }

                key = string.Join("/", segments, 1, segments.Length - 1);
                if (key.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    string text = key;
                    key = text.Substring(0, text.Length - 4);
                }

                __result = key;
                return false;
            }
            catch (Exception ex)
            {
                ModMonitor.Log($"Error loading arbitrary tilesheet ({relativePath}), reverting to default SMAPI behaviour. Ex: {ex}", LogLevel.Error);
                return true;
            }
        }
    }
}