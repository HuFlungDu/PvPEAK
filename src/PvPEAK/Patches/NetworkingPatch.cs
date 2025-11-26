using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvPEAK.Patches;

// PUN server doesn't let us make new RPCs, but Player.SyncInventoryRPC lets us send generic bytes, which we can hijack and filter for our commands.
internal class NetworkingPatch
{
    //[HarmonyPatch(typeof(Player), nameof(Player.SyncInventoryRPC))]
    //[HarmonyPrefix]
    //static bool Prefix(Player __instance, byte[] data, bool forceSync)
    //{
    //    if (!Plugin.ItsPvPEAK) { return true; }
    //    if (Plugin.networking.RoutePvPEAKRPC(data)) { return false; }
    //    return true;
    //}
}
