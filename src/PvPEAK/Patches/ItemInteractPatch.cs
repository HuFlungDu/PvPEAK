using BepInEx.Logging;
using HarmonyLib;
using PvPEAK.Model;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PvPEAK.Patches;
public class ItemInteractPatch
{
    //[HarmonyPatch(typeof(Item), "Interact")]
    //[HarmonyPrefix]
    //static bool Prefix(Item __instance, Character interactor)
    //{
    //    if (!Plugin.itemTracker.ItemIsOwned(__instance))
    //    {
    //        return true;
    //    }
    //    Team team = Plugin.teamTracker.GetPlayerTeam(interactor.player);
    //    if (!Plugin.itemTracker.ItemBelongsToTeam(__instance, team))
    //    {
    //        return false;
    //    }
    //    return true;
    //}

    [HarmonyPatch(typeof(Item), nameof(Item.RequestPickup))]
    [HarmonyPrefix]
    static bool RequestPickupPrefix(Item __instance, PhotonView characterView)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        Character component = characterView.GetComponent<Character>();
        if (!Plugin.itemTracker.ItemIsOwned(__instance) || !Utility.ItemIsInSafeZone(__instance))
        {
            return true;
        }
        Team team = Plugin.teamTracker.GetPlayerTeam(component.player);
        if (!Plugin.itemTracker.ItemCanBeGrabbedByTeam(__instance, team))
        {
            __instance.view.RPC("DenyPickupRPC", component.player.photonView.Owner);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.AddItem))]
    [HarmonyPostfix]
    static bool Postfix(bool __response, Player __instance, ushort itemID, ItemInstanceData instanceData, ItemSlot slot)
    {
        //ItemInstanceDataHandler.TryGetInstanceData(instanceData.guid, out ItemInstanceData o);
        if (!Plugin.ItsPvPEAK) { return __response; }
        if (__response && Plugin.IsAfterAwake && instanceData != null)
        {
            Team team = Plugin.teamTracker.GetPlayerTeam(__instance);
            if (team == null) { return __response; }
            foreach (KeyValuePair<Campfire, List<Backpack>> entry in Plugin.campfireBackpackMapping)
            {
                // If we ever find a way to get from an instancedata to an actual instance, we can remove this iteration and just do a contains on the guid
                Backpack campfire_backpack = null;
                foreach (Backpack bp in entry.Value)
                {
                    if (bp.data.guid == instanceData.guid)
                    {
                        campfire_backpack = bp;
                    }
                }
                if (campfire_backpack != null)
                {
                    entry.Value.Remove(campfire_backpack);
                    foreach (KeyValuePair<Color, Team> team_entry in Plugin.teamTracker.teams)
                    {
                        Plugin.itemTracker.RemoveItemFromNotTeam(campfire_backpack, team_entry.Value);
                        Plugin.itemTracker.RemoveItemFromTeam(campfire_backpack, team_entry.Value);
                    }
                    foreach (Backpack bp in entry.Value)
                    {
                        Plugin.itemTracker.SetItemToNotTeam(bp, team);
                    }
                }
            }
        }
        return __response;
    }

    [HarmonyPatch(typeof(Backpack), nameof(Backpack.Interact))]
    [HarmonyPrefix]
    static bool BackpackInteractPrefix(Backpack __instance, Character interactor)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        if (!Plugin.itemTracker.ItemIsOwned(__instance) || (!Utility.ItemIsInSafeZone(__instance) && __instance.itemState == ItemState.Ground))
        {
            return true;
        }
        Team team = Plugin.teamTracker.GetPlayerTeam(interactor.player);
        if (!Plugin.itemTracker.ItemCanBeGrabbedByTeam(__instance, team))
        {
            return false;
        }
        return true;
    }

    // Backpack handling seems to be client side, so we need to have everyone install the mod
    //[HarmonyPatch(typeof(Backpack), nameof(Backpack.RPCAddItemToBackpack))]
    //[HarmonyPrefix]
    //static bool RPCAddItemToBackpackPrefix(Item __instance, PhotonView playerView, byte slotID, byte backpackSlotID)
    //{
    //    Character component = playerView.GetComponent<Player>() GetComponent<Character>();
    //    if (!Plugin.itemTracker.ItemIsOwned(__instance))
    //    {
    //        return true;
    //    }
    //    Team team = Plugin.teamTracker.GetPlayerTeam(component.player);
    //    if (!Plugin.itemTracker.ItemBelongsToTeam(__instance, team))
    //    {
    //        return false;
    //    }
    //    return true;
    //}


}
