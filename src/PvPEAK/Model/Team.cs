using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PvPEAK.Model;
public class Team
{
    public HashSet<string> players = new();
    public Color teamColor;

    public Team(Color color)
    {
        this.teamColor = color;
    }

    public void AddPlayer(Character character)
    {
        this.AddPlayer(character.player.view.Owner);
    }
    public void AddPlayer(Player player)
    {
        this.AddPlayer(player.view.Owner);
    }
    public void AddPlayer(Photon.Realtime.Player player)
    {
        this.players.Add(player.UserId);
    }

    public void RemovePlayer(Character character)
    {
        this.RemovePlayer(character.player.view.Owner);
    }
    public void RemovePlayer(Player player)
    {
        this.RemovePlayer(player.view.Owner);
    }
    public void RemovePlayer(Photon.Realtime.Player player)
    {
        this.players.Remove(player.UserId);
    }

    public bool HasPlayer(Character character)
    {
        return this.HasPlayer(character.player.view.Owner);
    }
    public bool HasPlayer(Player player)
    {
        return this.HasPlayer(player.view.Owner);
    }
    public bool HasPlayer(Photon.Realtime.Player player)
    {
        return this.players.Contains(player.UserId);
    }
}
