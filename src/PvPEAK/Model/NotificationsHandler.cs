using BepInEx.Logging;
using PvPEAK.GameObjects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PvPEAK.Model;
public class NotificationsHandler
{
    public static GameObject notificationsUI;

    public static void Initialize()
    {
        Plugin.Logger.LogInfo("Creating notificationsUI");
        if (notificationsUI != null) { UnityEngine.Object.Destroy(notificationsUI); }

        notificationsUI = new GameObject("NotificationsUI");
        RectTransform rectTransform = notificationsUI.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);

        notificationsUI.AddComponent<Notifications>();
    }

    public static void SetNotification(string text, float duration)
    {
        notificationsUI.GetComponent<Notifications>().SetNotification(text, duration);
    }
}
