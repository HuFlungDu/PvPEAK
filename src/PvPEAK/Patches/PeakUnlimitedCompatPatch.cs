using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PvPEAK.Patches
{
    internal class PeakUnlimitedCompatPatch
    {
        // Don't want peak unlimited adding more marshmallows and backpacks, since we handle that
        [HarmonyPatch(typeof(PEAKUnlimited.Patches.CampfireAwakePatch), "AddMarshmallows")]
        [HarmonyPrefix]
        static bool AddMarshmallowsPrefix()
        {
            if (Plugin.ItsPvPEAK)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(PEAKUnlimited.Patches.CampfireAwakePatch), "AddBackpacks")]
        [HarmonyPrefix]
        static bool AddBackpacksPrefix()
        {
            if (Plugin.ItsPvPEAK)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PEAKUnlimited.Utility), nameof(PEAKUnlimited.Utility.SpawnMarshmallows))]
        [HarmonyPrefix]
        static bool SpawnMarshmallowsPrefix(ref List<GameObject> __result)
        {
            if (Plugin.ItsPvPEAK)
            {
                __result = new();
                return false;
            }
            return true;
        }

        // These functions will despawn our marshmallows if a new player joins, so just skip them.
        [HarmonyPatch(typeof(PEAKUnlimited.Patches.OnPlayerEnteredRoomPatch), "Postfix")]
        [HarmonyPrefix]
        static bool OnPlayerEnteredRoomPatchPostfixPrefix()
        {
            if (Plugin.ItsPvPEAK)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PEAKUnlimited.Patches.OnPlayerLeftRoomPatch), "Postfix")]
        [HarmonyPrefix]
        static bool OnPlayerLeftRoomPatchPostfixPrefix()
        {
            if (Plugin.ItsPvPEAK)
            {
                return false;
            }
            return true;
        }
    }
}
