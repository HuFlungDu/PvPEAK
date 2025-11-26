using BepInEx.Logging;
using Photon.Pun;
using PvPEAK.GameObjects;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using Zorro.Core;
using static Zorro.ControllerSupport.Rumble.RumbleClip;
using Random = UnityEngine.Random;

namespace PvPEAK;

public enum CampfireFoods
{
    Marshmallow = 46,
    Glizzy = 154
}

public static class Utility
{
    public static List<Vector3> GetEvenlySpacedPointsAroundCampfire(int numPoints, float innerRadius, float outerRadius, Vector3 campfirePosition, Vector3 campfireAngles, Segment advanceToSegment)
    {
        List<Vector3> points = new List<Vector3>();
        Quaternion campfireRotation = Quaternion.Euler(campfireAngles);
        
        for (int i = 0; i < numPoints; i++)
        {
            float radius = outerRadius;
            if (i % 2 == 0)
            {
                radius = innerRadius;
            }
            
            float angle = i * Mathf.PI * 2f / numPoints;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
            
            Vector3 localPos = new Vector3(x, 0f, z);
            Vector3 rotatedOffset = campfireRotation * localPos;
            Vector3 worldPos = campfirePosition + rotatedOffset;
            worldPos.y += -0.05f;
            
            //Fixes edge cases where SetToGround places marshmallows below the floor
            Vector3 worldPosGrounded = SetToGround(worldPos);
            if (Vector3.Distance(worldPos, worldPosGrounded) <= 1f)
            {
                worldPos = worldPosGrounded;
            }
            
            points.Add(worldPos);
        }
        
        return points;
    }

    public static List<Item> SpawnMarshmallows(int number, Vector3 campfirePosition, Vector3 campfireAngles, Segment advanceToSegment)
    {
        List<Item> marshmallows = new();
        
        foreach (Vector3 position in GetEvenlySpacedPointsAroundCampfire(number, 2f, 2.5f, campfirePosition, campfireAngles,
                     advanceToSegment))
        {
            float chance = Random.Range(0.0f, 1.0f);
            CampfireFoods randomEnum;
            if (chance < 0.5f)
            {
                randomEnum = CampfireFoods.Glizzy;
            }
            else
            {
                randomEnum = CampfireFoods.Marshmallow;
            }
            ushort randomValue = (ushort) randomEnum;
        
            Item obj = SingletonAsset<ItemDatabase>.Instance.itemLookup[randomValue];
            obj.GetName();
            
            
            Vector3 directionToCampfire = (campfirePosition - position).normalized;
            Quaternion rotation = Quaternion.LookRotation(directionToCampfire, Vector3.up);
            rotation *= Quaternion.Euler(0f, Random.Range(-30f, -150f), 0f);
            marshmallows.Add(Add(obj, position, rotation));
        }
        return marshmallows;
    }

    public static List<Backpack> SpawnBackpacks(int number, Vector3 campfirePosition, Vector3 campfireAngles, Segment advanceToSegment, Vector3? offset = null)
    {
        if (offset == null)
        {
            offset = Vector3.zero;
        }
        List<Backpack> list = new List<Backpack>();
        Item obj = SingletonAsset<ItemDatabase>.Instance.itemLookup[6];
        foreach (Vector3 position in Utility.GetEvenlySpacedPointsAroundCampfire(number, 3.3f, 3.7f, campfirePosition, campfireAngles, advanceToSegment)) {
            Vector3 finalPosition = position + offset.Value;
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Item bp = Utility.Add(obj, finalPosition, rotation);
            list.Add(bp as Backpack);
        }
        return list;
    }

    private static Vector3 SetToGround(Vector3 vector)
    {
        return HelperFunctions.GetGroundPos(vector, HelperFunctions.LayerType.TerrainMap);
    }

    public static Item Add(Item item, Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.IsConnected)
            return null;
        Plugin.Logger.LogInfo($"Spawn item: {item.name} at {position}");
        return PhotonNetwork.InstantiateItemRoom(item.name, position, rotation).GetComponent<Item>();
    }

    public static float DistanceToCampfire(Vector3 position, out Campfire nearest)
    {
        float distance = float.PositiveInfinity;
        nearest = null;
        foreach (Campfire campfire in Plugin.CampfireList)
        {
            if (campfire != null)
            {

                float campfire_distance = Vector3.Distance(position, campfire.transform.position);
                if (campfire_distance < distance)
                {
                    distance = campfire_distance;
                    nearest = campfire;
                }
            }
            //distance = Math.Min(distance, campfire_distance);
            //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Distance: {campfire_distance}, Smallest distance: {distance}");
        }
        return distance;
    }

    public static float DistanceToSatue(Vector3 position, out RespawnChest nearest)
    {
        float distance = float.PositiveInfinity;
        nearest = null;
        foreach (RespawnChest statue in Plugin.StatueList)
        {
            float statue_distance = Vector3.Distance(position, statue.transform.position);
            if (statue_distance < distance)
            {
                distance = statue_distance;
                nearest = statue;
            }
            //distance = Math.Min(distance, statue_distance);
            //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info, DebugLogType.CampfireLogic, $"Distance: {statue_distance}, Smallest distance: {distance}");
        }
        return distance;
    }

    public static bool ItemIsInSafeZone(Item item)
    {
        return (DistanceToCampfire(item.Center(), out var __) <= Plugin.ConfigurationHandler.ItemSafeDistance)/* || (DistanceToSatue(item.Center(), out var ___) <= Plugin.ConfigurationHandler.ItemSafeDistance)*/;
    }

    public static void AddCampfireSpawners(Campfire __instance)
    {
        Plugin.Logger.LogInfo("Backpackification enabled and starting!");

        __instance.gameObject.AddComponent<CampfireBackpackSpawner>();
        __instance.gameObject.AddComponent<CampfireFoodSpawner>();
            //backpack_count--;
            //Plugin.Logger.LogInfo($"Begin spawning {backpack_count} backpacks at {__instance}");
            //List<Backpack> backpacks = Utility.SpawnBackpacks(backpack_count, __instance.gameObject.transform.position, __instance.gameObject.transform.eulerAngles, __instance.advanceToSegment, new Vector3(0, 10f, 0f));
            //Plugin.Logger.LogInfo($"Spawned {backpacks.Count} backpacks at {__instance}");
            //foreach (Backpack bp in backpacks)
            //{
            //    Plugin.campfireBackpackMapping[__instance].Add(bp);
            //    bp.transform.parent = __instance.gameObject.transform;
            //}

        //Item obj = SingletonAsset<ItemDatabase>.Instance.itemLookup[6];
        //int numberOfExtraPlayers = PhotonNetwork.CurrentRoom.PlayerCount - Plugin.VanillaMaxPlayers;
        //int number = 0;
        //if (numberOfExtraPlayers > 0)
        //{
        //    double backpackNumber = numberOfExtraPlayers * 0.25;

        //    if (backpackNumber % 4 == 0)
        //    {
        //        number = (int)backpackNumber;
        //    }
        //    else
        //    {
        //        number = (int)backpackNumber;
        //        if (Random.Range(0f, 1f) <= backpackNumber - number)
        //        {
        //            number++;
        //        }
        //    }
        //}

        //if (Plugin.ConfigurationHandler.CheatBackpacks != 0)
        //{
        //    UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info,DebugLogType.BackpackLogic,"Cheat Backpacks enabled = " + Plugin.ConfigurationHandler.CheatBackpacks);
        //    number = Plugin.ConfigurationHandler.CheatBackpacks - 1; //Minus one as there is already a backpack present
        //}

        //UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info,DebugLogType.BackpackLogic,"Backpacks enabled = " + number);
        //if (number > 0)
        //{
        //    foreach (Vector3 position in Utility.GetEvenlySpacedPointsAroundCampfire(number, 3.3f, 3.7f,
        //                 __instance.gameObject.transform.position, __instance.gameObject.transform.eulerAngles,
        //                 __instance.advanceToSegment))
        //    {
        //        Vector3 finalPosition = position;
        //        if (__instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
        //        {
        //            finalPosition =
        //                position + new Vector3(0, 10f, 0f); // stops backpacks on the beach spawning underground...
        //        }

        //        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        //        Utility.Add(obj, finalPosition, rotation).transform.parent = __instance.gameObject.transform;
        //    }
        //}
        //else
        //{
        //    UnlimitedLogger.GetInstance().DebugMessage(LogLevel.Info,DebugLogType.BackpackLogic,
        //        "Not enough players to add additional backpacks, use the Cheat Backpack configuration setting if you want to override this!");
        //}
    }
}