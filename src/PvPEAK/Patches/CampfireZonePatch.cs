using BepInEx.Logging;
using HarmonyLib;
using Peak.Afflictions;
using PvPEAK.Model;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using static CharacterAfflictions;

namespace PvPEAK.Patches;
public class CampfireZonePatch
{
    //[HarmonyPatch(typeof(Campfire), nameof(RespawnChest.Interact_CastFinished))]
    //[HarmonyPrefix]
    //static bool Interact_CastFinishedPrefix(RespawnChest __instance, Character interactor)
    //{
    //    Plugin.touchedStatueTeam = Plugin.teamTracker.GetPlayerTeam(interactor.player);
    //    Plugin.StatueList.Add(__instance);
    //    return true;
    //}

    [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
    [HarmonyPrefix]
    static bool AddStatusPrefix(CharacterAfflictions __instance, STATUSTYPE statusType, float amount, bool fromRPC = false, bool playEffects = true)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        if (!Plugin.IsAfterAwake) { return true; }
        Character character = __instance.character;
        Team team = Plugin.teamTracker.GetPlayerTeam(character.player);
        if ( team == null ) { return true; }
        float distance_to_fire = Utility.DistanceToCampfire(character.Center, out Campfire reference_fire);
        if (distance_to_fire > 15 || reference_fire.state != Campfire.FireState.Off) { return true; }
        foreach (string userId in team.players)
        {
            Photon.Realtime.Player photon_player = null;
            foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value.UserId == userId)
                {
                    photon_player = player.Value;
                    break;
                }
            }
            if (photon_player == null) { continue; }
            Character other_character = PlayerHandler.GetPlayerCharacter(photon_player);
            float other_distance_to_fire = Utility.DistanceToCampfire(other_character.Center, out Campfire other_fire);
            if (other_distance_to_fire > 15 || other_fire != reference_fire) {
                return true; }
        }
        return false;
    }

    [HarmonyPatch(typeof(Campfire), nameof(Campfire.Light_Rpc))]
    [HarmonyPostfix]
    static void LightRPCPostfix(Campfire __instance)
    {
        if (!Plugin.ItsPvPEAK) { return; }
        if (!PhotonNetwork.IsMasterClient) { return; }
            foreach (Character character in Character.AllCharacters)
        {
            if (character.data.dead || character.data.fullyPassedOut)
            {
                character.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, __instance.transform.position + __instance.transform.up * 8f, true);
            }
        }
    }

}
