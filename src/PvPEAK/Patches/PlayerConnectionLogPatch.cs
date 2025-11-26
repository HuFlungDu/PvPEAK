using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PvPEAK.Patches;

public class PlayerConnectionLogPatch
{
    [HarmonyPatch(typeof(PlayerConnectionLog), "Awake")]
    [HarmonyPostfix]
    static void Postfix(PlayerConnectionLog __instance)
    {
        if (GameObject.Find("AirportGateKiosk") == null) return;
        if (Plugin.ItsPvPEAK)
        {
            __instance.AddMessage($"{__instance.GetColorTag(__instance.joinedColor)} It's PvPEAK!");
        }

    }
}