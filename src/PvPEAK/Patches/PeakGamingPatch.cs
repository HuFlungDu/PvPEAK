using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PvPEAK.Patches;

    internal class PeakGamingPatch
    {
        [HarmonyPatch(typeof(CharacterMovement), "Update")]
        [HarmonyPrefix]
        public static void MovementAndJumpPrefix(CharacterMovement __instance)
        {
            if (Plugin.CheatsEnabled)
            {
                //Sprint Patch
                Traverse.Create(__instance).Field("sprintMultiplier").SetValue(25f);

                //Jump patch
                if (Input.GetKey(KeyCode.Space))
                {
                    Character character = (Character)Traverse.Create(__instance).Field("character").GetValue();
                    if (character.IsLocal)
                    {
                        character.refs.view.RPC("JumpRpc", 0, [false]);
                    }
                }
            } else
            {
                Traverse.Create(__instance).Field("sprintMultiplier").SetValue(2f);
            }
        }

        [HarmonyPatch(typeof(CharacterMovement), "FallFactor")]
        [HarmonyPostfix]
        public static void RagdollPostfix(ref float __result)
        {
            if (Plugin.CheatsEnabled)
            {
                __result = 0;
            }
        }

        [HarmonyPatch(typeof(CharacterClimbing), "GetRequestedPostition")]
        [HarmonyPrefix]
        public static void ClimbingPrefix(CharacterClimbing __instance)
        {
            if (Plugin.CheatsEnabled)
            {
                __instance.climbSpeedMod = 30f;
            }
        }

        [HarmonyPatch(typeof(Character), "GetTotalStamina")]
        [HarmonyPrefix]
        public static void StaminaPrefix(Character __instance)
        {
                if (Plugin.CheatsEnabled)
                {
                    __instance.data.currentStamina = 100f;
                }
        }

        [HarmonyPatch(typeof(CharacterMovement), "TryToJump")]
        [HarmonyPrefix]
        public static bool JumpPrefix()
        {
            if (Plugin.CheatsEnabled)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CharacterAfflictions), "Update")]
        [HarmonyPrefix]
        public static void CharacterAfflictionsUpdatePrefix(CharacterAfflictions __instance)
        {
            if (Plugin.CheatsEnabled)
            {
                Traverse.Create(__instance.character.data).Field("isInvincible").SetValue(true);
            } else
            {
                Traverse.Create(__instance.character.data).Field("isInvincible").SetValue(false);
            }
        }
}

