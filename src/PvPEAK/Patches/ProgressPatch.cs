using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvPEAK.Patches
{
    // Handles "The fog/lava is rising" stuff
    internal class ProgressPatch
    {

        // These don't work, they appear to be remnants. We'll leave the patch in case they get turned back on.
        // Lava happens on a timer now
        [HarmonyPatch(typeof(MovingLava), nameof(MovingLava.PlayersHaveMovedOn))]
        [HarmonyPrefix]
        static bool MovingLavaPlayersHaveMovedOnPrefix(ref bool __result, MovingLava __instance)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            if (Character.AllCharacters.Count == 0)
            {
                return false;
            }
            __result = false;
            float num = 879f; // Magic number defined in the function
            foreach (var entry in Plugin.teamTracker.teams)
            {
                bool teamMovedOn = true;
                foreach (string uuid in entry.Value.players)
                {
                    if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                    {
                        Character character = player.character;
                        if (character.Center.y < num)
                        {
                            teamMovedOn = false; break;
                        }
                    }
                }
                __result |= teamMovedOn;
            }
            if (__result) { Plugin.Logger.LogInfo("Players have moved on"); }
            return false;
        }
        [HarmonyPatch(typeof(Fog), nameof(Fog.PlayersHaveMovedOn))]
        [HarmonyPrefix]
        static bool FogPlayersHaveMovedOnPrefix(ref bool __result, Fog __instance)
        {

            if (!Plugin.ItsPvPEAK) { return true; }
            if (Character.AllCharacters.Count == 0)
            {
                return false;
            }
            __result = false;
            float num = __instance.StopHeight() + __instance.startMoveHeightThreshold;
            foreach (var entry in Plugin.teamTracker.teams)
            {
                bool teamMovedOn = true;
                foreach (string uuid in entry.Value.players)
                {
                    if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                    {
                        Character character = player.character;
                        if (character.Center.y < num)
                        {
                            teamMovedOn = false; break;
                        }
                    }
                }
                __result |= teamMovedOn;
            }
            if (__result) { Plugin.Logger.LogInfo("Players have moved on"); }
            return false;
        }

        [HarmonyPatch(typeof(OrbFogHandler), nameof(OrbFogHandler.PlayersHaveMovedOn))]
        [HarmonyPrefix]
        static bool OrbFogHandlerPlayersHaveMovedOnPrefix(ref bool __result, OrbFogHandler __instance)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            if (Character.AllCharacters.Count == 0)
            {
                return true;
            }
            if (Ascents.currentAscent < 0)
            {
                return true;
            }
            __result = false;
            
            foreach (var entry in Plugin.teamTracker.teams)
            {
                bool teamMovedOn = true;
                bool foundPlayer = false;
                if (entry.Value.players.Count == 0)
                {
                    teamMovedOn = false;
                }
                foreach (string uuid in entry.Value.players)
                {
                    if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                    {
                        Character character = player.character;
                        foundPlayer = true;
                        if (character.Center.y < __instance.currentStartHeight || character.Center.z < __instance.currentStartForward)
                        {
                            teamMovedOn = false; break;
                        }
                    }
                }
                __result |= teamMovedOn && foundPlayer;
            }
            if (__result) { Plugin.Logger.LogInfo("Players have moved on"); }
            return false;
        }
    }
}
