using BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;
using HuniePopArchipelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace HuniePopArchipelagoClient.Utils
{
    internal class Patches
    {

        public static ArchipelagoClient arch;
        public static void patch(ArchipelagoClient a)
        {
            arch = a;
            Harmony.CreateAndPatchAll(typeof(Patches));

        }


        /// <summary>
        /// DEBUG PATCH TO MAKE PUZZLES COMPLETE IN 1 MOVE
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "AddResourceValue")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleGame __instance)
        {
            __instance.SetResourceValue(PuzzleGameResourceType.AFFECTION, 9999, true);
        }


        /// <summary>
        /// PROCESS THE ITEMS IN THE ARCH QUEUE AND SAVE THEM TO FLAGS
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "DepartLocation")]
        [HarmonyPrefix]
        public static void archcheck(LocationManager __instance, ref LocationDefinition ____destinationLocation)
        {
            PlayerManager player = GameManager.System.Player;

            Util.processarch(true);
            Util.processarch(false);

            if (player.alphaModeActive)
            {
                ArchipelagoClient.complete();
            }

        }

        [HarmonyPatch(typeof(GameManager), "SaveGame")]
        [HarmonyPostfix]
        public static void saveflags()
        {
            using (StreamWriter file = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, ArchipelagoClient.alist);
            }
        }


        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyPrefix]
        public static void datefin(PuzzleManager __instance, ref PuzzleGame ____activePuzzleGame)
        {
            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl);
            if (____activePuzzleGame.isVictorious)
            {
                if (girlData.relationshipLevel == 1)
                {
                    ArchipelagoClient.sendloc(42069013 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 2)
                {
                    ArchipelagoClient.sendloc(42069014 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 3)
                {
                    ArchipelagoClient.sendloc(42069015 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 4)
                {
                    ArchipelagoClient.sendloc(42069016 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
            }
            if (____activePuzzleGame.isBonusRound && !girlData.gotPanties)
            {
                ArchipelagoClient.sendloc(42069001 + (girlData.GetGirlDefinition().id - 1));
            }
        }

        [HarmonyPatch(typeof(GirlPlayerData), "AddItemToCollection")]
        [HarmonyPostfix]
        public static void cgiftloc(ItemDefinition item, GirlPlayerData __instance, ref bool __result)
        {
            if (__result)
            {
                ArchipelagoClient.sendloc(42069061 + ((__instance.GetGirlDefinition().id - 1) * 24) + (__instance.GetGirlDefinition().collection.IndexOf(item)));
            }
        }

        [HarmonyPatch(typeof(StoreCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool storepurchase(StoreItemSlot storeItemSlot, StoreCellApp __instance, ref int ____currentStoreTab)
        {
            if(____currentStoreTab == 0)
            {
                ArchipelagoConsole.LogMessage("PURCHASED ITEM");
            }
            return true;
        }


        [HarmonyPatch(typeof(LoadScreenSaveFile), "Refresh")]
        [HarmonyPostfix]
        public static void savetextoveride(LoadScreenSaveFile __instance, ref int ____saveFileIndex)
        {
            if (____saveFileIndex == 3)
            {
                __instance.titleLabel.SetText("ARCHIPELAGO FILE");
            }
            else
            {
                __instance.titleLabel.SetText("DISABLED");
            }
        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartFemaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newgirloveride(ref int ____saveFileIndex)
        {
            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (ArchipelagoClient.Authenticated && !saveFile.started)
            {

                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;

                saveFile.tutorialStep = 10;

                saveFile.currentGirl = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["start_girl"]);
                saveFile.currentLocation = 22;

                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    if (ArchipelagoClient.alist.list[i].item.Item > 42069012 && ArchipelagoClient.alist.list[i].item.Item < 42069025)
                    {
                        saveFile.girls[(int)ArchipelagoClient.alist.list[i].item.Item - 42069012].metStatus = 3;
                    }
                }

                return true;

            }
            return false;
        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartMaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newguyoveride(ref int ____saveFileIndex)
        {
            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (ArchipelagoClient.Authenticated && !saveFile.started)
            {

                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;

                saveFile.tutorialStep = 10;

                saveFile.currentGirl = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["start_girl"]);
                saveFile.currentLocation = 22;

                for (int i = 0; i<ArchipelagoClient.alist.list.Count; i++)
                {
                    if (ArchipelagoClient.alist.list[i].item.Item > 42069012 && ArchipelagoClient.alist.list[i].item.Item < 42069025)
                    {
                        saveFile.girls[(int)ArchipelagoClient.alist.list[i].item.Item - 42069012].metStatus = 3;
                    }
                }

                return true;

            }
            return false;

        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoveride(ref int ____saveFileIndex)
        {
            if (____saveFileIndex != 3) { return false; }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), "ResetFile")]
        [HarmonyPostfix]
        public static void savereset(SaveFile __instance)
        {

            __instance.currentGirl = -1;
            __instance.currentLocation = -1;

            __instance.inventory = new InventoryItemSaveData[30];
            for (int j = 0; j < __instance.inventory.Length; j++)
            {
                __instance.inventory[j] = new InventoryItemSaveData();
            }

            //__instance.started = true;
            //__instance.tutorialComplete = true;
            //__instance.tutorialStep = 10;
            //__instance.cellphoneUnlocked = true;
            //__instance.currentGirl = 9;
            //__instance.currentLocation = 22;
            //__instance.girls[9].metStatus = 3;
            //__instance.girls[8].metStatus = 3;
        }


        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool finderoveride(ref int ____saveFileIndex)
        {
            //if (____saveFileIndex != 3) { return false; }
            return true;
        }

        //[HarmonyPatch(typeof(GirlFinderIcon), "Init")]
        //[HarmonyPostfix]
        //public static void finderoverite(GirlFinderIcon __instance, ref LocationDefinition ____girlLocation)
        //{
        //    int[] l = [2, 3, 4, 5, 7, 8, 9, 11, 22];
        //    ____girlLocation = GameManager.Data.Locations.Get(l[UnityEngine.Random.Range(0, l.Count() - 1)]);
        //}

        [HarmonyPatch(typeof(GirlDefinition), "IsAtLocationAtTime")]
        [HarmonyPrefix]
        public static bool sceduleoverite(ref LocationDefinition __result)
        {
            int[] l = [2, 3, 4, 5, 7, 8, 9, 11, 22];
            __result = GameManager.Data.Locations.Get(l[UnityEngine.Random.Range(0, l.Count() - 1)]);
            return false;
        }

        [HarmonyPatch(typeof(GirlPlayerData), "KnowDetail")]
        [HarmonyPrefix]
        public static void favlocation(GirlDetailType type, GirlPlayerData __instance)
        {
            int loc = 42069349 + (int)type + (12*(__instance.GetGirlDefinition().id-1));
            if (!ArchipelagoClient.hasloc(loc)) { ArchipelagoClient.sendloc(loc); }
        }

        [HarmonyPatch(typeof(TraitsCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool talentoveride()
        {
            return false;
        }

        [HarmonyPatch(typeof(LocationManager), "CheckForSecretGirlUnlock")]
        [HarmonyPrefix]
        public static bool secretgirlunlockoverite(ref GirlDefinition __result)
        {
            __result = null;
            return false;
        }

        [HarmonyPatch(typeof(GirlProfileCellApp), "OnCollectionSlotPressed")]
        [HarmonyPrefix]
        public static bool collectionoverite()
        {
            return false;
        }

        [HarmonyPatch(typeof(GirlManager), "GiveItem")]
        [HarmonyPrefix]
        public static void releaseallitems()
        {
            if (GameManager.System.Player.pantiesTurnedIn.Count >= 12) { ArchipelagoClient.complete(); }
        }

        [HarmonyPatch(typeof(PlayerManager), "RollNewStoreList")]
        [HarmonyPrefix]
        public static bool storeoverite(StoreItemPlayerData[] storeList, ItemType itemType)
        {
            if (itemType == ItemType.GIFT || itemType == ItemType.UNIQUE_GIFT)
            {
                for (int l = 0; l < 12; l++)
                {
                    storeList[l].itemDefinition = null;
                    storeList[l].soldOut = true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerManager), "LogTossedItem")]
        [HarmonyPrefix]
        public static void toss(ItemDefinition item)
        {
            ArchipelagoConsole.LogMessage("ITEM TOSSED");
            if(item.type == ItemType.PANTIES)
            {
                long flag = item.id - 276 + 42069000;
                for (int l = 0;l < ArchipelagoClient.alist.list.Count; l++)
                {
                    if (ArchipelagoClient.alist.list[l].item.Item == flag)
                    {
                        ArchipelagoClient.alist.list[l].processed--; 
                        ArchipelagoClient.alist.list[l].priority = false; 
                        break; 
                    }
                }
            }
            else if (item.type == ItemType.UNIQUE_GIFT)
            {
                long flag = item.id - 192 + 42069096;
                for (int l = 0; l < ArchipelagoClient.alist.list.Count; l++)
                {
                    if (ArchipelagoClient.alist.list[l].item.Item == flag)
                    {
                        ArchipelagoClient.alist.list[l].processed--;
                        ArchipelagoClient.alist.list[l].priority = false;
                        break;
                    }
                }
            }
            else if (item.type == ItemType.GIFT)
            {
                long flag = item.id - 48 + 42069024;
                for (int l = 0; l < ArchipelagoClient.alist.list.Count; l++)
                {
                    if (ArchipelagoClient.alist.list[l].item.Item == flag)
                    {
                        ArchipelagoClient.alist.list[l].processed--;
                        ArchipelagoClient.alist.list[l].priority = false;
                        break;
                    }
                }
            }
        }






        [HarmonyPatch(typeof(GameManager), "LoadGame")]
        [HarmonyILManipulator]
        public static void loadgamereset(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Brtrue) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; break; }
            }
        }

        //[HarmonyPatch(typeof(GirlFinderIcon), "Init")]
        //[HarmonyILManipulator]
        //public static void findernull(ILContext ctx, MethodBase orig)
        //{
        //    int call = 0;
        //
        //    for (int i = 0; i < ctx.Instrs.Count; i++)
        //    {
        //        if (call == 17 && ctx.Instrs[i].OpCode == OpCodes.Call)
        //        {
        //            ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_0;
        //            ctx.Instrs[i].Operand = null;
        //            break;
        //        }
        //        if (ctx.Instrs[i].OpCode == OpCodes.Call) { call++; }
        //    }
        //}

    }

    class Util
    {
        public static int girlgifttoloc(GirlPlayerData girl, ItemDefinition item)
        {
            return 0;
        }

        public static void processarch(bool priority)
        {

            PlayerManager player = GameManager.System.Player;
            if (priority)
            {
                ArchipelagoConsole.LogMessage("processing priority items");
            }
            else
            {
                ArchipelagoConsole.LogMessage("processing non priority items");
            }

            for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
            {
                ArchipelagoItem item = ArchipelagoClient.alist.list[i];

                if (item.priority != priority) { continue; }

                if (item.processed >= item.recieved)
                {
                    continue;
                }

                if (item.item.Item > 42069000 && item.item.Item < 42069013)
                {
                    //PANTIES ITEMS
                    if (!player.IsInventoryFull())
                    {
                        ArchipelagoConsole.LogMessage("panties recieved");
                        player.AddItem(GameManager.Data.Items.Get((int)item.item.Item - 42069001 + 277), player.inventory, false, false);
                        item.processed++;
                    }
                }
                else if (item.item.Item > 42069012 && item.item.Item < 42069025)
                {
                    //GIRL UNLOCKS
                    ArchipelagoConsole.LogMessage("girl unlocked");
                    int girlid = (int)item.item.Item - 42069012;
                    for (int j = 0; j < player.girls.Count; j++)
                    {
                        if (player.girls[j].GetGirlDefinition().id == girlid)
                        {
                            player.girls[j].metStatus = GirlMetStatus.MET;
                            item.processed++;
                            break;
                        }
                    }
                }
                else if (item.item.Item > 42069024 && item.item.Item < 42069097)
                {
                    //GIFT ITEMS
                    ArchipelagoConsole.LogMessage("gift item recieved");
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get((int)item.item.Item - 42069025 + 49), player.inventory, false, false);
                        item.processed++;
                    }
                }
                else if (item.item.Item > 42069096 && item.item.Item < 42069169)
                {
                    //UNIQUE GIFT ITEMS
                    ArchipelagoConsole.LogMessage("unique gift item recieved");
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get((int)item.item.Item - 42069097 + 193), player.inventory, false, false);
                        item.processed++;
                    }
                }
                else if (item.item.Item > 42069168 && item.item.Item < 42069217)
                {
                    //TOKEN ITEMS
                    ArchipelagoConsole.LogMessage("token recieved");
                    if (item.item.Item < 42069175)
                    {
                        //TALENT
                        player.UpgradeTraitLevel(PlayerTraitType.TALENT);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069181)
                    {
                        //FLIRTATION
                        player.UpgradeTraitLevel(PlayerTraitType.FLIRTATION);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069187)
                    {
                        //ROMANCE
                        player.UpgradeTraitLevel(PlayerTraitType.ROMANCE);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069193)
                    {
                        //SEXUALITY
                        player.UpgradeTraitLevel(PlayerTraitType.SEXUALITY);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069199)
                    {
                        //PASSION
                        player.UpgradeTraitLevel(PlayerTraitType.PASSION);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069205)
                    {
                        //SENSITIVITY
                        player.UpgradeTraitLevel(PlayerTraitType.SENSITIVITY);
                        item.processed++;
                    }
                    else if (item.item.Item < 42069211)
                    {
                        //CHRISMA
                        player.UpgradeTraitLevel(PlayerTraitType.CHARISMA);
                        item.processed++;
                    }
                    else
                    {
                        //LUCK
                        player.UpgradeTraitLevel(PlayerTraitType.LUCK);
                        item.processed++;
                    }

                }

                if (item.processed == item.recieved) { item.priority = true; }

            }            
        }
    }
}
