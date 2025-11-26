using BepInEx.Logging;
using HarmonyLib;

namespace PvPEAK.Patches;

public class LeaveLobbyPatch
{
    [HarmonyPatch(typeof(SteamLobbyHandler), "LeaveLobby")]
    [HarmonyPostfix]
    static void Postfix(SteamLobbyHandler __instance)
    {
        //This is part of a gross way of testing if a user created a lobby, since PhotonNetwork.IsMasterClient doesn't seem to work in PlayerConnectionLog
        Plugin.Logger.LogInfo("Left Lobby");
        Plugin.leavingLobby = true;
        Plugin.Reset();
    }
}