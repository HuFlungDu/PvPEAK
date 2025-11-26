using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvPEAK.Patches;
internal class SkipIntroPatch
{
    [HarmonyPatch(typeof(Pretitle), nameof(Pretitle.Start))]
    [HarmonyPrefix]
    static bool Prefix(Pretitle __instance)
    {
        __instance.loadWait = 0;
        return true;
    }
}
