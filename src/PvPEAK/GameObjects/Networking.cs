using Photon.Pun;
using PvPEAK.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zorro.Core;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace PvPEAK.GameObjects;
public class Networking : MonoBehaviourPunCallbacks
{

    public PhotonView view;
    public string? PvPEAKVersion = null;
    public void Awake()
    {
        this.view = GetComponent<PhotonView>();
    }
    [PunRPC]
    public void SetItsPvPEAK_RPC(bool itis)
    {
        Plugin.ItsPvPEAK = itis;
        if (itis)
        {
            foreach (Campfire campfire in Plugin.CampfireList)
            {
                if (!campfire.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
                {
                    if (campfire.GetComponent<CampfireBackpackSpawner>() == null)
                    {
                        Utility.AddCampfireSpawners(campfire);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void SyncItems_RPC(byte[] mappings, byte[] not_mappings)
    {
        Plugin.itemTracker.AcceptSyncData(mappings, not_mappings);
    }

    [PunRPC]
    public void SetPvPEAKVersion_RPC(PhotonView characterView, string version)
    {
        characterView.GetComponent<Networking>().PvPEAKVersion = version;
    }
}
