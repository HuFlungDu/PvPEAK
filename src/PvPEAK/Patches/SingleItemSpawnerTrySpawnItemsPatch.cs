using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace PvPEAK.Patches;

public class SingleItemSpawnerTrySpawnItemsPatch
{
    [HarmonyPatch(typeof(Spawner), nameof(Spawner.TrySpawnItems))]
    [HarmonyPrefix]
    static bool Prefix(ref List<PhotonView> __result,Spawner __instance)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        List<PhotonView> empty = new();
        if (__instance.gameObject.name is "CampfireFoodSpawner")
        {
            //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Skipping food");
            __result = empty;
            return false;
        }
        //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Spawner: {__instance.gameObject.name}; Parent: {__instance.gameObject.transform.parent.gameObject.name}");
        if (__instance.gameObject.name.ToLower().Contains("luggage"))
        {
            if (__instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("plane"))
            {
                //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Skipping plane luggage");
                PhotonNetwork.Destroy(__instance.photonView);
                __result = empty;
                return false;
            }
            if (__instance.gameObject.transform.parent.parent.gameObject.name.ToLower().Contains("campfire") &&
                !__instance.gameObject.transform.parent.parent.gameObject.name.ToLower().Contains("campfirespawner_volcano"))
            {
                //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Skipping campfire luggage");
                PhotonNetwork.Destroy(__instance.photonView);
                __result = empty;
                return false;
            }
        }
        //if (__instance)
        return true;
    }

    [HarmonyPatch(typeof(SingleItemSpawner), nameof(SingleItemSpawner.TrySpawnItems))]
    [HarmonyPrefix]
    static bool SinglePrefix(ref List<PhotonView> __result, SingleItemSpawner __instance)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        if (__instance.gameObject.name is "Backpack_Spawner" or "Backpack (2)_Spawner")
        {
            __result = new List<PhotonView>();
            return false;
        }
        //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"SingleInstanceSpawner: {__instance.gameObject.name}");
        return true;
    }
}