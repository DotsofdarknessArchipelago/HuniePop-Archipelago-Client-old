using BepInEx;
using BepInEx.Logging;
using BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HuniePopArchipelagoClient.Utils;
using UnityEngine;

namespace BepInEx5ArchipelagoPluginTemplate.templates
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.dots.hunniepop";
        public const string PluginName = "HunniePopArchielago";
        public const string PluginVersion = "0.1.0";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        public string uri = "localhost";
        public string name = "Player1";
        public string pass = "";

        public bool enablecon = false;

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            ArchipelagoClient = new ArchipelagoClient();
            Patches.patch(ArchipelagoClient);
            ArchipelagoConsole.Awake();

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.Hidden = !ArchipelagoConsole.Hidden;
                ArchipelagoConsole.UpdateWindow();
            }
        }

        private void OnGUI()
        {
            GUI.depth = -1;
            // show the mod is currently loaded in the corner
            ArchipelagoConsole.OnGUI();

            GUI.backgroundColor = Color.black;

            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 40), "");
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "Client V(" + PluginVersion + "), World V(" + ArchipelagoClient.ServerData.slotData["world_version"]  + "): Status: Connected");
                
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 130), "");

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), APDisplayInfo + statusMessage);
                GUI.Label(new Rect(Screen.width - 295, 40, 150, 20), "Host: ");
                GUI.Label(new Rect(Screen.width - 295, 60, 150, 20), "Player Name: ");
                GUI.Label(new Rect(Screen.width - 295, 80, 150, 20), "Password: ");

                uri = GUI.TextField(new Rect(Screen.width - 150, 40, 140, 20), uri, 100);
                name = GUI.TextField(new Rect(Screen.width - 150, 60, 140, 20), name, 100);
                pass = GUI.TextField(new Rect(Screen.width - 150, 80, 140, 20), pass, 100);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(Screen.width - 200, 105, 100, 20), "Connect") && !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    ArchipelagoClient.ServerData.Uri = uri;
                    ArchipelagoClient.ServerData.SlotName = name;
                    ArchipelagoClient.ServerData.Password = pass;
                    ArchipelagoClient.Connect();
                }

            }

            // this is a good place to create and add a bunch of debug buttons
        }

    }
}