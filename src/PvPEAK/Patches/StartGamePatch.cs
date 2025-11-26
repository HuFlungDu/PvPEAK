using HarmonyLib;
using Photon.Pun;
using PvPEAK.Model;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace PvPEAK.Patches;

public class StartGamePatch
{
    //[HarmonyPatch(typeof(AirportCheckInKiosk), "StartGame")]
    //[HarmonyPrefix]
    //static void Prefix()
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //        Plugin.HasHostStarted = true;
    //}

    [HarmonyPatch(typeof(AirportCheckInKiosk), nameof(AirportCheckInKiosk.BeginIslandLoadRPC))]
    [HarmonyPostfix]
    static void BeginIslandLoadRPCPostfix()
    {
        if (!Plugin.ItsPvPEAK) { return; }

        Plugin.Reset();
        Plugin.teamTracker.CreateTeams();
        Plugin.teamTracker.GetOrCreateTeamByColor(Color.black);
        Plugin.teamTracker.GetOrCreateTeamByColor(Color.white);
    }
}