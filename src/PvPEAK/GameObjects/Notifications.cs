using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace PvPEAK.GameObjects;
public class Notifications : MonoBehaviour
{
    public Canvas canvas;
    public static int shadowMaterialID;
    public TextMeshProUGUI notificationsGUI;
    public int textPosX = 0;
    public int textPosY = 0;
    public float textSetTime = 0;
    public float textDuration = 0;
    public string text = "";


    public void Awake()
    {
        canvas = this.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scalar = canvas.gameObject.AddComponent<CanvasScaler>();
        scalar.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scalar.referenceResolution = new Vector2(1920f, 1080f);

        UnityEngine.Object[] materialList = Resources.FindObjectsOfTypeAll(typeof(Material));
        foreach (var material in materialList)
        {
            //Debug.Log($"Found Material: {material.name} ID({material.GetInstanceID()})");
            if (material.name == "DarumaDropOne-Regular SDF Shadow")
            {
                shadowMaterialID = material.GetInstanceID();
            }
        }

        //Finds the TMP_Font
        if (Plugin.fontAsset == null)
        {
            UnityEngine.Object[] fontList = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (TMP_FontAsset font in fontList)
            {
                if (font.name.Equals("DarumaDropOne-Regular SDF"))
                {
                    Plugin.fontAsset = font;
                }
            }
        }

        GameObject notificationsUI = new GameObject("NotificationsUI");
        notificationsUI.transform.SetParent(canvas.transform, worldPositionStays: false);
        notificationsGUI = notificationsUI.AddComponent<TextMeshProUGUI>();
        Vector3 notificationsPos = default(Vector3);
        notificationsPos.x = textPosX;
        notificationsPos.y = textPosY;
        notificationsGUI.text = "";
        SetupText(notificationsGUI, notificationsPos);
        notificationsGUI.font = Plugin.fontAsset;
        notificationsGUI.fontMaterial = (Material)Resources.InstanceIDToObject(shadowMaterialID);
        notificationsGUI.color = new Color(0.8745f, 0.8549f, 0.7608f, 1f);
        notificationsGUI.fontSize = 25;
    }

    public void LateUpdate()
    {
        if (this.text != "")
        {
            if (this.textDuration > 0 && Time.time > this.textSetTime+this.textDuration)
            {
                this.SetNotification("");
            }
            this.notificationsGUI.text = this.text;
        }
    }

    public static void SetupText(TextMeshProUGUI text, Vector3 anchoredPos)
    {
        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(.5f, .5f);
        rectTransform.anchorMax = new Vector2(.5f, .5f);
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.position += anchoredPos;
        rectTransform.sizeDelta = new Vector2(500f, 500f);
        text.alignment = TextAlignmentOptions.Center;
    }

    public void SetNotification(string text, float timeout = 0)
    {
        this.text = text;
        this.textSetTime = Time.time;
        this.textDuration = timeout;
    }
}
