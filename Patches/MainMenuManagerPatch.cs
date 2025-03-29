using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BanMod;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.First)]
public class MainMenuManagerStartPatch
{
    public static GameObject amongUsLogo;
    public static GameObject Ambience;
    public static SpriteRenderer ModLogo { get; private set; }

    private static void Postfix(MainMenuManager __instance)
    {
        amongUsLogo = GameObject.Find("LOGO-AU");
        var rightpanel = __instance.gameModeButtons.transform.parent;
        var logoObject = new GameObject("titleLogo_TOHE");
        var logoTransform = logoObject.transform;
        ModLogo = logoObject.AddComponent<SpriteRenderer>();
        logoTransform.parent = rightpanel;
        logoTransform.localPosition = new(-0.16f, 0f, 1f);
        logoTransform.localScale *= 1.2f;
        if ((Ambience = GameObject.Find("Ambience")) != null)
        {
            Ambience.SetActive(false);
        }
    }
}

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatch
{
    private static PassiveButton template;
    [HarmonyPatch(nameof(MainMenuManager.Start)), HarmonyPostfix, HarmonyPriority(Priority.Normal)]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        if (template == null) template = __instance.quitButton;
        __instance.screenTint.gameObject.transform.localPosition += new Vector3(1000f, 0f);
        __instance.screenTint.enabled = false;
        __instance.rightPanelMask.SetActive(true);
        __instance.mainMenuUI.FindChild<SpriteRenderer>("BackgroundTexture").transform.gameObject.SetActive(false);
        __instance.mainMenuUI.FindChild<SpriteRenderer>("WindowShine").transform.gameObject.SetActive(false);
        __instance.mainMenuUI.FindChild<Transform>("ScreenCover").gameObject.SetActive(false);
        GameObject leftPanel = __instance.mainMenuUI.FindChild<Transform>("LeftPanel").gameObject;
        GameObject rightPanel = __instance.mainMenuUI.FindChild<Transform>("RightPanel").gameObject;
        rightPanel.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        GameObject maskedBlackScreen = rightPanel.FindChild<Transform>("MaskedBlackScreen").gameObject;
        maskedBlackScreen.GetComponent<SpriteRenderer>().enabled = false;
        maskedBlackScreen.transform.localScale = new Vector3(7.35f, 4.5f, 4f);
        __instance.mainMenuUI.gameObject.transform.position += new Vector3(-0.2f, 0f);
        leftPanel.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        leftPanel.gameObject.FindChild<SpriteRenderer>("Divider").enabled = false;
        leftPanel.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.name == "Shine").ToList().ForEach(r => r.enabled = false);
        GameObject splashArt = new("SplashArt");
        splashArt.transform.position = new Vector3(0, 0f, 600f); //= new Vector3(0, 0.40f, 600f);
        var spriteRenderer = splashArt.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Utils.LoadSprite("BanMod.Resources.image.png", 150f);
        if (template == null) return;
        var howToPlayButton = __instance.howToPlayButton;
        var freeplayButton = howToPlayButton.transform.parent.Find("FreePlayButton");
        if (freeplayButton != null) freeplayButton.gameObject.SetActive(false);
        howToPlayButton.transform.SetLocalX(0);

    }

    public static PassiveButton CreateButton(string name, Vector3 localPosition, Color32 normalColor, Color32 hoverColor, UnityEngine.Events.UnityAction action, string label, Vector2? scale = null)
    {
        var button = Object.Instantiate(template, MainMenuManagerStartPatch.ModLogo.transform);
        button.name = name;
        Object.Destroy(button.GetComponent<AspectPosition>());
        button.transform.localPosition = localPosition;
        button.OnClick = new();
        button.OnClick.AddListener(action);
        var buttonText = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TMP_Text>();
        buttonText.fontSize = buttonText.fontSizeMax = buttonText.fontSizeMin = 3.5f;
        buttonText.enableWordWrapping = false;
        buttonText.text = label;
        var normalSprite = button.inactiveSprites.GetComponent<SpriteRenderer>();
        var hoverSprite = button.activeSprites.GetComponent<SpriteRenderer>();
        normalSprite.color = normalColor;
        hoverSprite.color = hoverColor;
        var container = buttonText.transform.parent;
        Object.Destroy(container.GetComponent<AspectPosition>());
        Object.Destroy(buttonText.GetComponent<AspectPosition>());
        container.SetLocalX(0f);
        buttonText.transform.SetLocalX(0f);
        buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        var buttonCollider = button.GetComponent<BoxCollider2D>();
        if (scale.HasValue)
        {
            normalSprite.size = hoverSprite.size = buttonCollider.size = scale.Value;
        }
        buttonCollider.offset = new(0f, 0f);
        return button;
    }
    public static void Modify(this PassiveButton passiveButton, UnityEngine.Events.UnityAction action)
    {
        if (passiveButton == null) return;
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(action);
    }
    public static T FindChild<T>(this MonoBehaviour obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First((T c) => c.name == name2);
    }
    public static T FindChild<T>(this GameObject obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First((T c) => c.name == name2);
    }
    public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        IEnumerator<TSource> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            action(enumerator.Current);
        }

        enumerator.Dispose();
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPostfix]
    public static void OpenMenu_Postfix()
    {
        if (MainMenuManagerStartPatch.ModLogo != null) MainMenuManagerStartPatch.ModLogo.gameObject.SetActive(false);
    }
    [HarmonyPatch(nameof(MainMenuManager.ResetScreen)), HarmonyPostfix]
    public static void ResetScreen_Postfix()
    {
        if (MainMenuManagerStartPatch.ModLogo != null) MainMenuManagerStartPatch.ModLogo.gameObject.SetActive(true);
    }
}
