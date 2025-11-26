using Newtonsoft.Json;
using Photon.Pun;
using PvPEAK.GameObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using static PvPEAK.Model.ItemTracker;

namespace PvPEAK.Model
{
    public class TeamTracker
    {
        public Dictionary<Color, Team> teams = new();

        //public class TeamDictJsonConverter : JsonConverter
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        Plugin.Logger.LogInfo($"Type: {objectType}");
        //        return objectType == typeof(Dictionary<Color, Team>);
        //    }

        //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        //    {
        //        if (reader.TokenType == JsonToken.Null)
        //        {
        //            return null;
        //        }
        //        var mappings = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(reader.Value as string);
        //        Dictionary<Color, Team> serializedTeams = new();
        //        foreach (var mapping in mappings)
        //        {
        //            var color_list = (mapping["color"] as List<float>);
        //            Color color = new Color(color_list[0], color_list[1], color_list[2], color_list[3]);
        //            Team team = null;
        //            if (serializedTeams.ContainsKey(color))
        //            {
        //                team = serializedTeams[color];
        //            }
        //            if (team == null)
        //            {
        //                team = new Team(color);
        //                serializedTeams[team.teamColor] = team;
        //            }
        //            Team team = new Team(color);
        //        }
                
        //        return Plugin.teamTracker.GetOrCreateTeamByColor(color);
        //    }

        //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        //    {
        //        if (value == null)
        //        {
        //            writer.WriteNull();
        //            return;
        //        }
        //        List < Dictionary<string, object> > mappings = new();
        //        foreach (KeyValuePair<Color, Team> entry in (value as Dictionary<Color, Team>))
        //        {
        //            foreach (string player in entry.Value.players)
        //            {
        //                mappings.Add(new Dictionary<string, object>{ { "color", new List<float>([entry.Key.r, entry.Key.g, entry.Key.b, entry.Key.a]) }, { "player", player } });
        //            }
        //        }
        //        writer.WriteValue(JsonConvert.SerializeObject(mappings));
        //    }
        //}

        public int Count { get { return teams.Count; } }

        public Team GetPlayerTeam(Character character)
        {
            return this.GetPlayerTeam(character.player);
        }
        public Team GetPlayerTeam(Player player)
        {
            return this.GetPlayerTeam(player.view.Owner);
        }
        public Team GetPlayerTeam(Photon.Realtime.Player player)
        {
            Team team = null;
            foreach (KeyValuePair<Color, Team> entry in this.teams)
            {
                if (entry.Value.HasPlayer(player))
                {
                    team = entry.Value;
                }
            }
            return team;
        }

        public Team GetOrCreateTeamByColor(Color color)
        {
            Team team = null;
            if (this.teams.ContainsKey(color))
            {
                team = this.teams[color];
            }
            if (team == null)
            {
                team = new Team(color);
                this.teams[team.teamColor] = team;
            }
            return team;
        }

        public void RemoveTeam(Color color)
        {
            Team team = GetOrCreateTeamByColor(color);
            this.RemoveTeam(team);
        }

        public void RemoveTeam(Team team)
        {
            this.teams.Remove(team.teamColor);
        }

        public void CreateTeams()
        {
            foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
            {
                Team team = this.GetOrCreateTeamByColor(character.refs.customization.PlayerColor);
                Plugin.Logger.LogInfo("Created team: " + team.teamColor);
                team.AddPlayer(character);
            }
        }

        public void Clear()
        {
            this.teams.Clear();
        }

        //public void TrySyncTeamData(Photon.Realtime.Player? player = null)
        //{
        //    JsonConverter[] converters = [new TeamDictJsonConverter()/*, new ItemJsonConverter()*/];
        //    Plugin.Logger.LogInfo(JsonConvert.SerializeObject(this.not_mappings, converters));
        //    if (Player.localPlayer != null)
        //    {
        //        //;
        //        //Dictionary<int, float[]> serializedMappings = new();
        //        //Dictionary<int, Dictionary<string, float>[]> serializedNotMappings = new();
        //        //foreach (KeyValuePair<Item, Team> mapping in mappings)
        //        //{
        //        //    serializedMappings[mapping.Key.view.ViewID] = [mapping.Value.teamColor.r, mapping.Value.teamColor.g, mapping.Value.teamColor.b, mapping.Value.teamColor.a];
        //        //}
        //        //foreach (KeyValuePair<Item, HashSet<Team>> not_mapping in not_mappings)
        //        //{
        //        //    //This is not the most efficient way to do this, but this is the safest way without dealing with manual serialization
        //        //    serializedNotMappings[not_mapping.Key.view.ViewID] = not_mapping.Value.Select((Team t) => new Dictionary<string, float> { { "r", t.teamColor.r }, { "g", t.teamColor.g }, { "b", t.teamColor.b }, { "a", t.teamColor.a } }).ToArray();
        //        //}
        //        //JsonConverter[] converters = [new TeamJsonConverter(), new ItemJsonConverter()];
        //        Plugin.Logger.LogInfo($"Sending item ownership sync");
        //        if (player != null)
        //        {
        //            Player.localPlayer.GetComponent<Networking>().view.RPC("SyncItems_RPC", player, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.mappings, converters)), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.not_mappings, converters)));
        //        }
        //        else
        //        {
        //            Player.localPlayer.GetComponent<Networking>().view.RPC("SyncItems_RPC", RpcTarget.Others, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.mappings, converters)), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.not_mappings, converters)));
        //        }
        //    }
        //}

        //public void AcceptSyncData(byte[] serializedMappings, byte[] serializedNotMappings)
        //{
        //    JsonConverter[] converters = [new TeamJsonConverter()/*, new ItemJsonConverter()*/];
        //    this.mappings = JsonConvert.DeserializeObject<Dictionary<int, Team>>(Encoding.UTF8.GetString(serializedMappings), converters);
        //    this.not_mappings = JsonConvert.DeserializeObject<Dictionary<int, List<Team>>>(Encoding.UTF8.GetString(serializedNotMappings), converters);
        //    //this.mappings.Clear();
        //    //this.not_mappings.Clear();
        //    //foreach (KeyValuePair<int, float[]> mapping in serializedMappings)
        //    //{
        //    //    Color color = new Color(mapping.Value[0], mapping.Value[1], mapping.Value[2], mapping.Value[3]);
        //    //    Team team = Plugin.teamTracker.GetOrCreateTeamByColor(color);
        //    //    Item item = PhotonNetwork.GetPhotonView(mapping.Key).GetComponent<Item>();
        //    //    this.mappings[item] = team;
        //    //}
        //    //foreach (KeyValuePair<int, Dictionary<string, float>[]> not_mapping in serializedNotMappings)
        //    //{
        //    //    Item item = PhotonNetwork.GetPhotonView(not_mapping.Key).GetComponent<Item>();
        //    //    this.not_mappings[item] = new(not_mapping.Value.Select((Dictionary<string, float> color) => Plugin.teamTracker.GetOrCreateTeamByColor(new Color(color["r"], color["g"], color["b"], color["a"]))));
        //    //}
        //}
    }
}
