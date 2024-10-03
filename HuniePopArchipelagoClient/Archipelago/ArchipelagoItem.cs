using Archipelago.MultiClient.Net.Models;
using BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace HuniePopArchipelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        public NetworkItem item;
        public int recieved;
        public int processed;

    }

    public class ArchipelageItemList
    {
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();
        public string seed = "";

        public ArchipelagoItem getitem(long flag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item.Item == flag) { return list[i]; }
            }
            return null;
        }

        public void add(NetworkItem netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item.Equals(netitem))
                {
                    list[i].recieved += 1;
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem();
            item.item = netitem;
            item.recieved = 1;
            item.processed = 0;
            list.Add(item);
        }

        public void reset()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].recieved = 0;
            }
        }

        public int itemtoprocess()
        {
            int p = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].recieved > list[i].processed)
                {
                    p += (list[i].recieved - list[i].processed);
                }
            }
            return p;
        }
    }
}
