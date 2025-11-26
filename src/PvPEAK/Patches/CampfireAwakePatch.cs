using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using PvPEAK.GameObjects;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;
using static Zorro.ControllerSupport.Rumble.RumbleClip;
using Random = UnityEngine.Random;

namespace PvPEAK.Patches;

public class CampfireAwakePatch
{
    [HarmonyPatch(typeof(Campfire), "Awake")]
    [HarmonyPostfix]
    static void Postfix(Campfire __instance)
    {

        // Newly added clients need to know where the campfires are, but they haven't learned that ItsPvPEAK by the time this runs, so add them anyway
        if (__instance.nameOverride == "NAME_PORTABLE STOVE")
            return;
        Plugin.CampfireList.Add(__instance);
        Plugin.IsAfterAwake = true;
        if (!Plugin.ItsPvPEAK) { return; }
        if (PhotonNetwork.IsMasterClient && __instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
        {
            SpawnBeachItems(__instance);
        }
        else
        {
            Utility.AddCampfireSpawners(__instance);
        }
    }

    static void SpawnBeachItems(Campfire campfire)
    {
        int backpack_count = Plugin.teamTracker.Count;
        Plugin.campfireBackpackMapping[campfire] = new();
        Item testItem = null;
        //Debug
        testItem = SingletonAsset<ItemDatabase>.Instance.itemLookup[30];

        // Starting fire

        List<Backpack> backpacks = Utility.SpawnBackpacks(backpack_count, campfire.gameObject.transform.position, campfire.gameObject.transform.eulerAngles, campfire.advanceToSegment, new Vector3(0, 10f, 0));
            int i = 0;
            foreach (KeyValuePair<Color, Team> entry in Plugin.teamTracker.teams)
            {
                Backpack bp = backpacks[i];
                Plugin.itemTracker.SetItemToTeam(bp, entry.Value);
                i++;
            }

            //16 Spawn a magic bugle for testing
            if (testItem != null)
            {
                foreach (Vector3 position in Utility.GetEvenlySpacedPointsAroundCampfire(backpack_count, 4f, 4f,
                         campfire.gameObject.transform.position, campfire.gameObject.transform.eulerAngles,
                         campfire.advanceToSegment))
                {
                    Vector3 finalPosition = position + new Vector3(0, 10f, 0f);
                    Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    Item bp = Utility.Add(testItem, finalPosition, rotation);
                    //bp.transform.parent = __instance.gameObject.transform;
                }
            }


        }

    //private static void AddMarshmallows(Campfire __instance)
    //{
    //    if (__instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
    //    {
    //        return;
    //    }

    //    int amountOfMarshmallowsToSpawn = PhotonNetwork.CurrentRoom.PlayerCount;

    //    Plugin.Logger.LogInfo($"Will spawn {amountOfMarshmallowsToSpawn} marshmallows for {PhotonNetwork.CurrentRoom.PlayerCount} people!");
    //    Vector3 position = __instance.gameObject.transform.position;
    //    Vector3 eulerAngles = __instance.gameObject.transform.eulerAngles;
    //    Plugin.Marshmallows.Add(__instance, Utility.SpawnMarshmallows(amountOfMarshmallowsToSpawn, position, eulerAngles, __instance.advanceToSegment));
    //}
}