using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using PvPEAK.GameObjects;
using PvPEAK.Model;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zorro.Core;

namespace PvPEAK.Patches;

public class OnPlayerLeftRoomPatch : MonoBehaviour
{
    [HarmonyPatch(typeof(ReconnectHandler), nameof(ReconnectHandler.OnPlayerLeftRoom))]
    [HarmonyPostfix]
    static void Postfix(ReconnectHandler __instance, Photon.Realtime.Player otherPlayer)
    {
        if (Plugin.IsAfterAwake)
        {
            Team team = Plugin.teamTracker.GetPlayerTeam(otherPlayer);
            team.RemovePlayer(otherPlayer);
            if (team.players.Count == 0)
            {
                Plugin.teamTracker.RemoveTeam(team);
            }
        }
        //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.NetworkingLogic,"Someone has left the room! Number: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + ConfigurationHandler.ConfigMaxPlayers.Value);
        //if (!Plugin.ConfigurationHandler.IsLateMarshmallowsEnabled)
        //    return;
        //if (Plugin.CampfireList == null || Plugin.CampfireList.Count == 0) return;
        //Segment segment = Singleton<MapHandler>.Instance.GetCurrentSegment();
        //if (Plugin.IsAfterAwake && PhotonNetwork.IsMasterClient && Plugin.ConfigurationHandler.CheatMarshmallows == 0)
        //{
        //    //Delete existing marshmallows
        //    foreach (var campfireMarshmallows in Plugin.Marshmallows)
        //    {
        //        if (campfireMarshmallows.Key.advanceToSegment > segment)
        //        {
        //            foreach (var marshmallow in campfireMarshmallows.Value)
        //            {
        //                PhotonNetwork.Destroy(marshmallow);
        //            }
        //        }
        //    }
        //    Plugin.Marshmallows.Clear();
        //    foreach (Campfire campfire in Plugin.CampfireList)
        //    {
        //        if (campfire.advanceToSegment > segment)
        //        {
        //            //respawn this campfires marshmallows
        //            Plugin.Marshmallows.Add(campfire, Utility.SpawnMarshmallows(PhotonNetwork.CurrentRoom.PlayerCount, campfire.transform.position, campfire.gameObject.transform.eulerAngles, campfire.advanceToSegment));
        //        }
        //    }
        //}
    }
    // Make sure we keep screaming that this is PvPEAK
    [HarmonyPatch(typeof(ReconnectHandler), nameof(ReconnectHandler.OnMasterClientSwitched))]
    [HarmonyPostfix]
    static void OnMasterClientSwitchedPostfix(ReconnectHandler __instance, Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player.localPlayer.GetComponent<Networking>().view.RPC("SetItsPvPEAK_RPC", RpcTarget.AllBuffered, Plugin.ItsPvPEAK);

            // At this point the old host is still in the list, so we need to wait for them to leave
            // This is stupid; it would be nice to have an actual handle on the old MasterClient so we are just gonno do it like this.
            // Probably better to give everyone a spawner and have it only spawn/despawn if it's the master client.
            //__instance.StartCoroutine(respawnCampfireStuff());
            //IEnumerator respawnCampfireStuff()
            //{
            //    // Wait for the specified number of seconds
            //    yield return new WaitForSeconds(.5f);
            //    foreach (Campfire campfire in Plugin.CampfireList)
            //    {
            //        Utility.AddCampfireSpawners(campfire);
            //    }
            //}
            
        }
    }
}