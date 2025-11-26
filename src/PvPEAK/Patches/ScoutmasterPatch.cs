using HarmonyLib;
using PvPEAK.GameObjects;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace PvPEAK.Patches
{
    internal class ScoutmasterPatch
    {

        // This segregates scoutmaster into teams. He won't come after a team of 1.
        [HarmonyPatch(typeof(Scoutmaster), nameof(Scoutmaster.ViableTargets))]
        [HarmonyPrefix]
        static bool ScoutmasterViableTargetsPrefix(ref int __result, Scoutmaster __instance)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            __result = 0;

            foreach (KeyValuePair<Color, Team> entry in Plugin.teamTracker.teams)
            {
                int num = 0;
                foreach (string uuid in entry.Value.players)
                {
                    if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                    {
                        Character character = player.character;
                        if (!character.isBot && !character.data.dead && !character.data.fullyPassedOut)
                        {
                            num++;
                        }
                    }
                }
                __result = Math.Max(__result, num);
            }
            return false;
        }

        [HarmonyPatch(typeof(Scoutmaster), nameof(Scoutmaster.LookForTarget))]
        [HarmonyPrefix]
        static bool LookForTargetPrefix(Scoutmaster __instance)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            if (__instance.ViableTargets() < 2 || __instance.sinceLookForTarget < 30f)
            {
                return false;
            }

            __instance.sinceLookForTarget = 0f;
            Plugin.Logger.LogInfo("Trying to spawn scoutmaster");
            if (!(UnityEngine.Random.value > 0.1f))
            {
                Character target = null;
                foreach (var entry in Plugin.teamTracker.teams)
                {
                    Character highestCharacter = GetHighestCharacterOnTeam(entry.Value, null);
                    Character highestCharacter2 = GetHighestCharacterOnTeam(entry.Value, highestCharacter);
                    if (highestCharacter2 == null) { continue; }
                    if (highestCharacter.Center.y > highestCharacter2.Center.y + __instance.attackHeightDelta && highestCharacter.Center.y < __instance.maxAggroHeight)
                    {
                        if (target == null || highestCharacter.Center.y > target.Center.y)
                        {
                            target = highestCharacter;
                        }
                    }
                }
                if (target != null)
                {
                    __instance.SetCurrentTarget(target);
                }
            }
            return false;
        }

        public static Character GetHighestCharacterOnTeam(Team team, Character ignoredCharacter)
        {
            Character result = null;
            foreach (string uuid in team.players)
            {
                if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                {
                    Character character = player.character;
                    if (!character.isBot && !character.data.dead && !character.data.fullyPassedOut && !(character == ignoredCharacter) && (character == null || character.Center.y > result.Center.y))
                    {
                        result = character;
                    }
                }
            }
            return result;
        }

        [HarmonyPatch(typeof(Scoutmaster), nameof(Scoutmaster.GetClosestOther))]
        [HarmonyPrefix]
        static bool ScoutmasterGetClosestOtherPrefix(ref Character __result, Scoutmaster __instance, Character currentTarget)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            __result = null;

            Team team = Plugin.teamTracker.GetPlayerTeam(currentTarget);

            float num = float.MaxValue;
            foreach (string uuid in team.players)
            {
                if (Plugin.player_mapping.TryGetValue(uuid, out Player player) && player.character != null)
                {
                    Character character = player.character;
                    if (!character.isBot && !(character == currentTarget))
                    {
                        float num2 = Vector3.Distance(character.Center, currentTarget.Center);
                        if (num2 < num)
                        {
                            num = num2;
                            __result = character;
                        }
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Scoutmaster), nameof(Scoutmaster.VerifyTarget))]
        [HarmonyPrefix]
        static bool ScoutmasterVerifyTargetPrefix(Scoutmaster __instance)
        {
            if (!Plugin.ItsPvPEAK) { return true; }
            if (__instance.ViableTargets() < 2)
            {
                __instance.SetCurrentTarget(null);
                return false;
            }

            Team team = Plugin.teamTracker.GetPlayerTeam(__instance.currentTarget);

            Character closestOther = __instance.GetClosestOther(__instance.currentTarget);
            Character highestCharacter = GetHighestCharacterOnTeam(team, null);
            Character highestCharacter2 = GetHighestCharacterOnTeam(team, highestCharacter);

            if (highestCharacter.Center.y > __instance.maxAggroHeight)
            {
                __instance.SetCurrentTarget(null);
            }
            else if (__instance.currentTarget != highestCharacter)
            {
                __instance.SetCurrentTarget(null);
            }
            else if (highestCharacter.Center.y < highestCharacter2.Center.y + __instance.attackHeightDelta - 20f)
            {
                __instance.SetCurrentTarget(null);
            }
            else if (Vector3.Distance(closestOther.Center, __instance.currentTarget.Center) < 15f)
            {
                __instance.SetCurrentTarget(null);
            }
            return false;
        }
    }
}
