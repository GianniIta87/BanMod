﻿using UnityEngine;
using InnerNet;
using System.Linq;
using AmongUs.GameOptions;
using System.Text.RegularExpressions;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;
using HarmonyLib;
using Rewired.Utils.Platforms.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Collections;
using Il2CppSystem.Diagnostics;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
using System.Threading.Tasks;
using Sentry.Protocol;
using Hazel;
using static BanMod.SpamManager;
using UnityEngine.ResourceManagement.Util;
using Il2CppInterop.Runtime;
using System.Linq.Expressions;

namespace BanMod;
public static class Utils
{
    //Useful for getting full lists of all the Among Us cosmetics IDs
    public static ReferenceDataManager referenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance;
    public static bool isShip => ShipStatus.Instance;
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool isPlayer => PlayerControl.LocalPlayer;
    public static bool isHost = AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;
    public static bool isMeeting => MeetingHud.Instance;
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Proceeding;
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool InGame;
    public static bool AlreadyDied;
    public static bool IsInGame => InGame;
    public static bool IsNotJoined => AmongUsClient.Instance.GameState == InnerNetClient.GameStates.NotJoined;
    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool IsLocalGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInTask => InGame && !MeetingHud.Instance;
    public static bool IsMeeting => InGame && MeetingHud.Instance;
    public static bool IsVoting => IsMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool IsShip => ShipStatus.Instance != null;
    public static bool IsCanMove => PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.CanMove;
    internal static bool IsLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool IsDead => PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead;
    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", string.Empty);
    public static string RemoveHtmlTagsTemplate(this string str) => Regex.Replace(str, string.Empty, string.Empty);
    public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
    public class PlayerState(byte playerId)
    {
        public readonly byte PlayerId = playerId;
        public bool IsDead { get; set; } = false;
        public bool Disconnected { get; set; } = false;
        
    }
    public static bool IsHost(this InnerNetObject ino) => ino.OwnerId == AmongUsClient.Instance.HostId;
    public static bool IsHost(this byte id) => GetPlayerById(id)?.OwnerId == AmongUsClient.Instance.HostId;
    public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    public static PlayerControl GetPlayerById(int PlayerId, bool fast = true)
    {
        if (PlayerId is > byte.MaxValue or < byte.MinValue) return null;


        return BanMod.AllPlayerControls.FirstOrDefault(x => x.PlayerId == PlayerId);
    }

    private static readonly Dictionary<string, Sprite> CachedSprites = [];
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            Logger.Error($"Error loading texture from: {path}", "LoadImage");
        }

        return null;
    }
    public static void ShowHelp(byte ID)
    {
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, (
            Translator.GetString("CommandList")
            + $"\n  ○ <color=#FF0000><b>/id</b></color> {Translator.GetString("Command.id")}"
            + $"\n  ○ <color=#FF0000><b>/add (id)</b></color> {Translator.GetString("Command.add")}"
            + $"\n  ○ <color=#FF0000><b>/dlt (id)</b></color> {Translator.GetString("Command.dlt")}"
            + $"\n  ○ <color=#FF0000><b>/level (num)</b></color> {Translator.GetString("Command.level")}"
            + $"\n  ○ <color=#FF0000><b>/msg</b></color> {Translator.GetString("Command.msg")}"
            + $"\n  ○ <color=#FF0000><b>/msgs</b></color> {Translator.GetString("Command.msgs")}"
            + $"\n  ○ <color=#FF0000><b>/msgw</b></color> {Translator.GetString("Command.msgw")}"
            + $"\n  ○ <color=#FF0000><b>/spam</b></color> {Translator.GetString("Command.spam")}"
            + $"\n  ○ <color=#FF0000><b>/word</b></color> {Translator.GetString("Command.word")}"
            ));
           
    }
    public static unsafe class FastDestroyableSingleton<T> where T : MonoBehaviour
    {
        private static readonly IntPtr FieldPtr;
        private static readonly Func<IntPtr, T> CreateObject;

        static FastDestroyableSingleton()
        {
            FieldPtr = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<DestroyableSingleton<T>>.NativeClassPtr, nameof(DestroyableSingleton<T>._instance));
            var constructor = typeof(T).GetConstructor([typeof(IntPtr)]);
            var ptr = Expression.Parameter(typeof(IntPtr));
            var create = Expression.New(constructor!, ptr);
            var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
            CreateObject = lambda.Compile();
        }

        public static T Instance
        {
            get
            {
                IntPtr objectPointer;
                IL2CPP.il2cpp_field_static_get_value(FieldPtr, &objectPointer);
                return objectPointer == IntPtr.Zero ? DestroyableSingleton<T>.Instance : CreateObject(objectPointer);
            }
        }
    }
    public static bool fullBrightActive()
    {
        return Utils.IsDead || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }


    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream?.CopyTo(ms);
            texture.LoadImage(ms.ToArray(), false);
            return texture;
        }
        catch
        {
            Logger.Error($"读入Texture失败：{path}", "LoadImage");
        }

        return null;
    }
    public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - TimeStampStartTime).TotalSeconds;
    private static readonly DateTime TimeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static NetworkedPlayerInfo GetPlayerInfoById(int PlayerId) =>
       GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => info.PlayerId == PlayerId);
    public static bool chatUiActive()
    {
        try
        {
            return BanMod.AktiveChat.Value || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }
    public static void openChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening){
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
        }

    }
    public static void closeChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }

    }

    public class PlayerVersion(Version ver, string tagStr, string forkId)
    {
        public readonly string forkId = forkId;
        public readonly string tag = tagStr;
        public readonly Version version = ver;

        public PlayerVersion(string ver, string tagStr, string forkId) : this(Version.Parse(ver), tagStr, forkId)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PlayerVersion)obj);
        }

        private bool Equals(PlayerVersion other)
        {
            return forkId == other.forkId && tag == other.tag && Equals(version, other.version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(forkId, tag, version);
        }
    }



    public static string getColoredPingText(int ping){

        if (ping <= 100){ // Green for ping < 100

            return $"<color=#00ff00ff>PING: {ping} ms</color>";

        } else if (ping < 400){ // Yellow for 100 < ping < 400

            return $"<color=#ffff00ff>PING: {ping} ms</color>";

        } else{ // Red for ping > 400

            return $"<color=#ff0000ff>PING: {ping} ms</color>";
        }
    }


    public static KeyCode stringToKeycode(string keyCodeStr){

        if(!string.IsNullOrEmpty(keyCodeStr)){ // Empty strings are automatically invalid

            try{
                
                // Case-insensitive parse of UnityEngine.KeyCode to check if string is validssss
                KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodeStr, true);
                
                return keyCode;

            }catch{}
        
        }

        return KeyCode.Delete; // If string is invalid, return Delete as the default key
    }

}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckName))]
class PlayerControlCheckNamePatch
{
    public static void Postfix(PlayerControl __instance, ref string playerName)
    {
        if (!AmongUsClient.Instance.AmHost || !Utils.IsLobby) return;
        playerName = __instance.Data.PlayerName ?? playerName;
        if (BanManager.CheckDenyNamePlayer(__instance, playerName)) return;
    }
}


