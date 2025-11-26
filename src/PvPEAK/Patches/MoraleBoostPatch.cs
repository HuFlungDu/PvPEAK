using HarmonyLib;
using Photon.Pun;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace PvPEAK.Patches
{
    internal class MoraleBoostPatch
    {
        
        [HarmonyPatch(typeof(MoraleBoost), nameof(MoraleBoost.SpawnMoraleBoost))]
        [HarmonyPrefix]
        static bool SpawnMoraleBoostPrefix(ref bool __result, Vector3 origin, float radius, float baselineStaminaBoost, float staminaBoostPerAdditionalScout, bool sendToAll = false, int minScouts = 1)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            __result = false;
            foreach (KeyValuePair<Color, Team> entry in Plugin.teamTracker.teams)
            {
                List<Character> list = new List<Character>();
                foreach (string uuid in entry.Value.players) {
                    if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                    {
                        if (radius == -1f || Vector3.Distance(player.character.Center, origin) <= radius)
                        {
                            list.Add(player.character);
                        }
                    }
                }
                if (list.Count < minScouts)
                {
                    continue;
                }
                __result = true;
                foreach (Character character in list)
                {
                    character.MoraleBoost(baselineStaminaBoost, list.Count);
                }

            }
            return false;
        }
    }
}
