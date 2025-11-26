using HarmonyLib;
using Peak.Afflictions;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvPEAK.Patches;
// Used for things like the friendship bugle toot
internal class MassAfflictionPatch
{
    [HarmonyPatch(typeof(Action_ApplyMassAffliction), nameof(Action_ApplyMassAffliction.TryAddAfflictionToLocalCharacter))]
    [HarmonyPrefix]
    static bool TryAddAfflictionToLocalCharacterPrefix(Action_ApplyMassAffliction __instance)
    {
        if (!Plugin.ItsPvPEAK) { return true; }
        if (__instance.name.ToLower().Contains("bugle_magic"))
        {
            if (Plugin.teamTracker.GetPlayerTeam(__instance.character.player) != Plugin.teamTracker.GetPlayerTeam(Character.localCharacter.player))
            {
                return false;
            }
        }
        return true;
    }
}
