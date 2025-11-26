using Photon.Pun;
using PvPEAK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace PvPEAK.GameObjects
{
    public class CampfireFoodSpawner : MonoBehaviour
    {
        Campfire campfire;
        List<Item> foods;
        public void Awake()
        {
            this.campfire = GetComponent<Campfire>();
            Plugin.Logger.LogInfo($"Food Spawner awake: {campfire}");
            this.foods = new();
            Plugin.campfireFoodMapping[this.campfire] = this.foods;
        }

        public void Start()
        {
            Plugin.Logger.LogInfo($"Food Spawner start: {campfire}");
            //int backpack_count = Plugin.teamTracker.Count-1;
            //this.backpacks = Utility.SpawnBackpacks(backpack_count, this.gameObject.transform.position, this.gameObject.transform.eulerAngles, this.campfire.advanceToSegment);
            //Plugin.campfireBackpackMapping[this.campfire] = this.backpacks;
            //foreach (Backpack backpack in this.backpacks)
            //{
            //    backpack.transform.parent = this.transform.parent.parent;
            //}
        }

        public void OnEnable()
        {
            Plugin.Logger.LogInfo($"Food Spawner onEnable: {campfire}");
            if (PhotonNetwork.IsMasterClient)
            {
                int food_count = PhotonNetwork.CurrentRoom.PlayerCount;

                // This is just cuz technically the backpacks could be already grabbed or something? I dunno, don't want hanging references in case something weird happens. It never should.
                List<Item> spawnedFood = Utility.SpawnMarshmallows(food_count, this.gameObject.transform.position, this.gameObject.transform.eulerAngles, this.campfire.advanceToSegment);
                var i = 0;
                foreach (var entry in Plugin.teamTracker.teams)
                {
                    foreach (string uuid in entry.Value.players)
                    {
                        Plugin.itemTracker.SetItemToTeam(spawnedFood[i], entry.Value);
                        i++;
                    }
                }

                this.foods.AddRange(spawnedFood);
            }
        }
        public void OnDisable()
        {
            Plugin.Logger.LogInfo($"Food Spawner onDisable: {campfire}");
            Plugin.Logger.LogInfo($"Leaving lobby: {Plugin.leavingLobby}");
            // Don't destroy items if the master client user quits. I don't know a better way to do this.
            if (PhotonNetwork.IsMasterClient && gameObject.scene.isLoaded)
            {
                foreach (Item item in this.foods.ToList())
                {
                    if (item != null && (item.holderCharacter ?? item.lastHolderCharacter ?? item.lastThrownCharacter ?? null) == null)
                    {
                        PhotonNetwork.Destroy(item.gameObject);
                        this.foods.Remove(item);
                    }
                }
            }
        }
    }
}
