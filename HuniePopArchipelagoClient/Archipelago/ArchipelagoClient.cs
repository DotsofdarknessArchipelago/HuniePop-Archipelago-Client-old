using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HuniePopArchipelagoClient.Archipelago;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
namespace BepInEx5ArchipelagoPluginTemplate.templates.Archipelago
{

    public class ArchipelagoClient
    {
        public const string APVersion = "0.5.0";
        private const string Game = "Hunie Pop";

        public static bool Authenticated;
        private bool attemptingConnection;

        public static ArchipelagoData ServerData = new();
        private static ArchipelagoSession session;

        public static Queue<long> itemstoprocess = new Queue<long>();
        public static ArchipelageItemList alist = new ArchipelageItemList();

        /// <summary>
        /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (Authenticated || attemptingConnection) return;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
                SetupSession();
            }
            catch (Exception e)
            {
                ArchipelagoConsole.LogMessage(e.Message);
                Plugin.BepinLogger.LogError(e);
            }

            TryConnect();
        }

        /// <summary>
        /// add handlers for Archipelago events
        /// </summary>
        private void SetupSession()
        {
            session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += OnSessionErrorReceived;
            session.Socket.SocketClosed += OnSessionSocketClosed;
        }

        /// <summary>
        /// attempt to connect to the server with our connection info
        /// </summary>
        private void TryConnect()
        {
            try
            {
                // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
                ThreadPool.QueueUserWorkItem(
                    _ => HandleConnectResult(
                        session.TryConnectAndLogin(
                            Game,
                            ServerData.SlotName.Trim(),
                            ItemsHandlingFlags.AllItems, // TODO make sure to change this line
                            new Version(APVersion),
                            password: ServerData.Password.Trim(),
                            requestSlotData: true // ServerData.NeedSlotData
                        )));
            }
            catch (Exception e)
            {
                Plugin.BepinLogger.LogError(e);
                ArchipelagoConsole.LogMessage(e.Message);
                HandleConnectResult(new LoginFailure(e.ToString()));
                attemptingConnection = false;
            }
        }

        /// <summary>
        /// handle the connection result and do things
        /// </summary>
        /// <param name="result"></param>
        private void HandleConnectResult(LoginResult result)
        {
            string outText;
            if (result.Successful)
            {
                var success = (LoginSuccessful)result;

                ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
                Authenticated = true;

                session.Locations.CompleteLocationChecks(ServerData.CheckedLocations.ToArray());
                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

                ArchipelagoConsole.LogMessage(outText);
                if (File.Exists(Application.persistentDataPath + "/archdata"))
                {
                    using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                        if (session.RoomState.Seed == savedlist.seed)
                        {
                            ArchipelagoConsole.LogMessage("file found restoring session");
                            alist = savedlist;
                            alist.reset();
                        }
                        else
                        {
                            ArchipelagoConsole.LogMessage("file found but dosent match server seed/playername creating new session");
                            alist.seed = session.RoomState.Seed;
                        }
                    }
                }
                else
                {
                    ArchipelagoConsole.LogMessage("file not found creating new session");
                    alist.seed = session.RoomState.Seed;
                }

            }
            else
            {
                var failure = (LoginFailure)result;
                outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
                outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

                Plugin.BepinLogger.LogError(outText);

                Authenticated = false;
                Disconnect();
            }

            ArchipelagoConsole.LogMessage(outText);
            attemptingConnection = false;

            if (Authenticated)
            {
                alist.reset();

                foreach (NetworkItem item in session.Items.AllItemsReceived)
                {
                    alist.add(item);
                }
            }

        }

        /// <summary>
        /// something we wrong or we need to properly disconnect from the server. cleanup and re null our session
        /// </summary>
        private void Disconnect()
        {
            Plugin.BepinLogger.LogDebug("disconnecting from server...");
            session?.Socket.Disconnect();
            session = null;
            Authenticated = false;
        }

        public void SendMessage(string message)
        {
            session.Socket.SendPacketAsync(new SayPacket { Text = message });
        }

        private bool CompareItems(ArchipelageItemList Itemlist1, NetworkItem Item2)
        {
            using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                for (int i = 0; i < Itemlist1.list.Count; i++)
                {
                    if (Itemlist1.list[i].item.Equals(Item2) || Itemlist1.list[i].item.Equals(savedlist.list[i]))
                        return false;
                }
                return true;
            }



        }

        /// <summary>
        /// we received an item so reward it here
        /// </summary>
        /// <param name="helper">item helper which we can grab our item from</param>
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var receivedItem = helper.DequeueItem();
            if (CompareItems(alist, receivedItem))
              { alist.add(receivedItem);
                ArchipelagoConsole.LogMessage("ITEM RECIEVED: " + session.Items.GetItemName(receivedItem.Item));
                if (helper.Index < ServerData.Index) return;
            }
            ServerData.Index++;  
        
        }

        public static void sendloc(int loc)
        {
            session.Locations.CompleteLocationChecks(loc);
        }

        public static bool hasloc(int loc)
        {
            return session.Locations.AllLocationsChecked.Contains(loc);
        }

        public static string seed()
        {
            return session.RoomState.Seed;
        }

        public static string getname(long flag)
        {
            return session.Items.GetItemName(flag);
        }

        public static void complete()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            session.Socket.SendPacket(statusUpdatePacket);
        }


        /// <summary>
        /// something went wrong with our socket connection
        /// </summary>
        /// <param name="e">thrown exception from our socket</param>
        /// <param name="message">message received from the server</param>
        private void OnSessionErrorReceived(Exception e, string message)
        {
            Plugin.BepinLogger.LogError(e);
            ArchipelagoConsole.LogMessage(message);
        }

        /// <summary>
        /// something went wrong closing our connection. disconnect and clean up
        /// </summary>
        /// <param name="reason"></param>
        private void OnSessionSocketClosed(string reason)
        {
            Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
            ArchipelagoConsole.LogMessage($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }
    }
}