using BepInEx.Logging;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Photon.Pun;
using PvPEAK.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI;

namespace PvPEAK.Model;
public class ItemTracker
{
    private Dictionary<int, Team> mappings = new();
    private Dictionary<int, List<Team>> not_mappings = new();

    //This doesn't work for keys anyway, so just implement keys as int
    //public class ItemJsonConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        Plugin.Logger.LogInfo($"Type: {objectType}");
    //        return typeof(Item).IsAssignableFrom(objectType);
    //    }

    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null)
    //        {
    //            return null;
    //        }
    //        Item item = PhotonNetwork.GetPhotonView(Convert.ToInt32(reader.Value)).GetComponent<Item>();
    //        return item;
    //    }

    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //            return;
    //        }
    //        Plugin.Logger.LogDebug("Writing ");
    //        writer.WriteValue((value as Item).view.ViewID);
    //    }
    //}
    public class TeamJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Team);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            var color_list = JsonConvert.DeserializeObject<List<float>>(reader.Value as string);
            Color color = new Color(color_list[0], color_list[1], color_list[2], color_list[3]);
            return Plugin.teamTracker.GetOrCreateTeamByColor(color);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            Color color = (value as Team).teamColor;
            writer.WriteValue(JsonConvert.SerializeObject(new List<float>([color.r, color.g, color.b, color.a])));
        }
    }

    public void SetItemToTeam(Item item, Team team)
    {
        this.mappings[item.photonView.ViewID] = team;
        this.TrySyncItemData();
    }

    public void SetItemToNotTeam(Item item, Team team)
    {
        if (!this.not_mappings.TryGetValue(item.photonView.ViewID, out List<Team> teams))
        {
            teams = new();
            this.not_mappings[item.photonView.ViewID] = teams;
        }
        teams.Add(team);
        this.TrySyncItemData();
    }

    public void RemoveItemFromTeam(Item item, Team team)
    {
        this.mappings.Remove(item.photonView.ViewID);
        this.TrySyncItemData();
    }

    public void RemoveItemFromNotTeam(Item item, Team team)
    {
        if (!this.not_mappings.TryGetValue(item.photonView.ViewID, out List<Team> teams))
        {
            teams = new List<Team>();
            this.not_mappings[item.photonView.ViewID] = teams;
        }
        teams.Remove(team);
        this.TrySyncItemData();
    }

    public void TrySyncItemData(Photon.Realtime.Player? player = null)
    {
        JsonConverter[] converters = [new TeamJsonConverter()/*, new ItemJsonConverter()*/];
        if (Player.localPlayer != null)
        {
            Plugin.Logger.LogInfo($"Sending item ownership sync");
            if (player != null)
            {
                Player.localPlayer.GetComponent<Networking>().view.RPC("SyncItems_RPC", player, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.mappings, converters)), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.not_mappings, converters)));
            }
            else
            {
                Plugin.Logger.LogInfo($"Networking component: {Player.localPlayer.GetComponent<Networking>()}");
                Player.localPlayer.GetComponent<Networking>().view.RPC("SyncItems_RPC", RpcTarget.Others, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.mappings, converters)), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.not_mappings, converters)));
            }
        }
    }

    public void AcceptSyncData(byte[] serializedMappings, byte[] serializedNotMappings)
    {
        JsonConverter[] converters = [new TeamJsonConverter()/*, new ItemJsonConverter()*/];
        this.mappings = JsonConvert.DeserializeObject<Dictionary<int, Team>>(Encoding.UTF8.GetString(serializedMappings), converters);
        this.not_mappings = JsonConvert.DeserializeObject<Dictionary<int, List<Team>>>(Encoding.UTF8.GetString(serializedNotMappings), converters);
        //this.mappings.Clear();
        //this.not_mappings.Clear();
        //foreach (KeyValuePair<int, float[]> mapping in serializedMappings)
        //{
        //    Color color = new Color(mapping.Value[0], mapping.Value[1], mapping.Value[2], mapping.Value[3]);
        //    Team team = Plugin.teamTracker.GetOrCreateTeamByColor(color);
        //    Item item = PhotonNetwork.GetPhotonView(mapping.Key).GetComponent<Item>();
        //    this.mappings[item] = team;
        //}
        //foreach (KeyValuePair<int, Dictionary<string, float>[]> not_mapping in serializedNotMappings)
        //{
        //    Item item = PhotonNetwork.GetPhotonView(not_mapping.Key).GetComponent<Item>();
        //    this.not_mappings[item] = new(not_mapping.Value.Select((Dictionary<string, float> color) => Plugin.teamTracker.GetOrCreateTeamByColor(new Color(color["r"], color["g"], color["b"], color["a"]))));
        //}
    }

    //public void SetItemOwnership(int itemId, Color teamColor)
    //{
    //    Plugin.Logger.LogInfo($"Received remote item ownership: {itemId} to {teamColor}");
        
    //}

    //public void SetItemNotOwnership(int itemId, Color teamColor)
    //{
    //    Team team = Plugin.teamTracker.GetOrCreateTeamByColor(teamColor);
    //    Item item = PhotonNetwork.GetPhotonView(itemId).GetComponent<Item>();
    //    Plugin.Logger.LogInfo($"Got item not ownership change: {item} to {team.teamColor}");
    //    if (!this.not_mappings.TryGetValue(item, out HashSet<Team> teams)) {
    //        teams = new HashSet<Team>();
    //        this.not_mappings[item] = teams;
    //    }
    //    teams.Add(team);
    //}

    //public void RemoveItemOwnership(int itemId, Color teamColor)
    //{
    //    Team team = Plugin.teamTracker.GetOrCreateTeamByColor(teamColor);
    //    Item item = PhotonNetwork.GetPhotonView(itemId).GetComponent<Item>();
    //    Plugin.Logger.LogInfo($"Got remove item ownership: {item} to {team.teamColor}");
    //    this.mappings.Remove(item);
    //}

    //public void RemoveItemNotOwnership(int itemId, Color teamColor)
    //{
    //    Team team = Plugin.teamTracker.GetOrCreateTeamByColor(teamColor);
    //    Item item = PhotonNetwork.GetPhotonView(itemId).GetComponent<Item>();
    //    Plugin.Logger.LogInfo($"Got remove item not ownership change: {item} to {team.teamColor}");
    //    if (!this.not_mappings.TryGetValue(item, out HashSet<Team> teams))
    //    {
    //        teams = new HashSet<Team>();
    //        this.not_mappings[item] = teams;
    //    }
    //    teams.Remove(team);
    //}

    public bool ItemCanBeGrabbedByTeam(Item item, Team team)
    {
        // Doing checks in this order allows us to handle ownership updates without doing any additional network activity
        Character last_holder = item.holderCharacter ?? item.lastHolderCharacter ?? item.lastThrownCharacter;
        if (last_holder != null)
        {
            return team.HasPlayer(last_holder.player.photonView.Owner);
        }
        if (this.not_mappings.TryGetValue(item.photonView.ViewID, out List<Team> teams))
        {
            if (teams.Contains(team))
            {
                return false;
            }
        }
        if (this.mappings.TryGetValue(item.photonView.ViewID, out Team owningTeam))
        {
            return this.mappings[item.photonView.ViewID] == team;
        }
        return true;
    }
    public bool ItemIsOwned(Item item)
    {
        bool ret = false;
        // Doing checks in this order allows us to handle ownership updates without doing any additional network activity
        Character last_holder = item.holderCharacter ?? item.lastHolderCharacter ?? item.lastThrownCharacter;
        if (last_holder != null)
        {
            foreach (KeyValuePair<Color, Team> team in Plugin.teamTracker.teams)
            {
                ret |= team.Value.HasPlayer(last_holder.player.photonView.Owner);
            }
        }
        return ret || this.mappings.ContainsKey(item.photonView.ViewID) || (this.not_mappings.ContainsKey(item.photonView.ViewID) && this.not_mappings[item.photonView.ViewID].Count > 0);
    }
    public void Reset()
    {
        this.mappings = new();
    }
}
