﻿using Maple2Storage.Types.Metadata;
using MapleServer2.Data.Static;

namespace MapleServer2.Types
{
    public class Npc : NpcMetadata
    {
        public short ZRotation; // In degrees * 10

        public Npc(int id)
        {
            NpcMetadata npc = NpcMetadataStorage.GetNpcMetadata(id);
            if (npc != null)
            {
                Id = npc.Id;
                Animation = 255;
                Friendly = npc.Friendly;
                Kind = npc.Kind;
                ShopId = npc.ShopId;
            }
        }

        public bool IsShop()
        {
            return Kind == 13;
        }

        public bool IsBank()
        {
            return Kind == 2;
        }
    }
}
