using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace PvPEAK.GameObjects
{
    public class CampfireBackpackSpawner : MonoBehaviour
    {
        Campfire campfire;
        List<Backpack> backpacks;
        public void Awake()
        {
            this.campfire = GetComponent<Campfire>();
            Plugin.Logger.LogInfo($"Backpack Spawner awake: {campfire}");
            this.backpacks = new List<Backpack>();
            Plugin.campfireBackpackMapping[this.campfire] = this.backpacks;
        }

        public void Start()
        {
            Plugin.Logger.LogInfo($"Backpack Spawner start: {campfire}");
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
            Plugin.Logger.LogInfo($"Backpack Spawner onEnable: {campfire}");
            if (PhotonNetwork.IsMasterClient)
            {
                int backpack_count = Plugin.teamTracker.Count - 1;

                // This is just cuz technically the backpacks could be already grabbed or something? I dunno, don't want hanging references in case something weird happens. It never should.
                // Add 5f to the Y of the backpack, because otherwise it can spawn in bushes.
                List<Backpack> spawnedBackpacks = Utility.SpawnBackpacks(backpack_count, this.gameObject.transform.position, this.gameObject.transform.eulerAngles, this.campfire.advanceToSegment, new Vector3(0, 5f, 0));
                foreach (Backpack backpack in spawnedBackpacks)
                {
                    backpack.transform.parent = this.transform.parent.parent;
                }
                this.backpacks.AddRange(spawnedBackpacks);
            }
        }
        public void OnDisable()
        {
            Plugin.Logger.LogInfo($"Backpack Spawner onDisable: {campfire}");
            Plugin.Logger.LogInfo($"Leaving lobby: {Plugin.leavingLobby}");
            // Don't destroy items if the master client user quits. I don't know a better way to do this.
            if (PhotonNetwork.IsMasterClient && gameObject.scene.isLoaded)
            {
                Plugin.Logger.LogInfo($"Deleting objects");
                foreach (Backpack backpack in this.backpacks.ToList())
                {
                    if (backpack != null && (backpack.holderCharacter ?? backpack.lastHolderCharacter ?? backpack.lastThrownCharacter ?? null) == null)
                    {
                        PhotonNetwork.Destroy(backpack.gameObject);
                        this.backpacks.Remove(backpack);
                    }
                }
            }
            
        }
    }
}
