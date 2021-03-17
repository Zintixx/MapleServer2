﻿using System;
using System.Collections.Generic;
using System.Linq;
using Maple2Storage.Enums;
using Maple2Storage.Types;
using Maple2Storage.Types.Metadata;
using MaplePacketLib2.Tools;
using MapleServer2.Constants;
using MapleServer2.Data;
using MapleServer2.Data.Static;
using MapleServer2.Enums;
using MapleServer2.Packets;
using MapleServer2.Servers.Game;
using MapleServer2.Tools;
using MapleServer2.Types;
using Microsoft.Extensions.Logging;

namespace MapleServer2.PacketHandlers.Game
{
    public class BeautyHandler : GamePacketHandler
    {
        public override RecvOp OpCode => RecvOp.BEAUTY;

        public BeautyHandler(ILogger<BeautyHandler> logger) : base(logger) { }

        private enum BeautyMode : byte
        {
            LoadShop = 0x0,
            NewBeauty = 0x3,
            ModifyExistingBeauty = 0x5,
            ModifySkin = 0x6,
            RandomHair = 0x7,
            ChooseRandomHair = 0xC,
            SaveHair = 0x10,
            DeleteSavedHair = 0x12,
            ChangeToSavedHair = 0x15,
            DyeItem = 0x16
        }

        public override void Handle(GameSession session, PacketReader packet)
        {
            BeautyMode mode = (BeautyMode) packet.ReadByte();

            switch (mode)
            {
                case BeautyMode.LoadShop:
                    HandleLoadShop(session, packet);
                    break;
                case BeautyMode.NewBeauty:
                    HandleNewBeauty(session, packet);
                    break;
                case BeautyMode.ModifyExistingBeauty:
                    HandleModifyExistingBeauty(session, packet);
                    break;
                case BeautyMode.ModifySkin:
                    HandleModifySkin(session, packet);
                    break;
                case BeautyMode.RandomHair:
                    HandleRandomHair(session, packet);
                    break;
                case BeautyMode.ChooseRandomHair:
                    HandleChooseRandomHair(session, packet);
                    break;
                case BeautyMode.SaveHair:
                    HandleSaveHair(session, packet);
                    break;
                case BeautyMode.DeleteSavedHair:
                    HandleDeleteSavedHair(session, packet);
                    break;
                case BeautyMode.ChangeToSavedHair:
                    HandleChangeToSavedHair(session, packet);
                    break;
                case BeautyMode.DyeItem:
                    HandleDyeItem(session, packet);
                    break;
                default:
                    IPacketHandler<GameSession>.LogUnknownMode(mode);
                    break;
            }
        }

        private static void HandleLoadShop(GameSession session, PacketReader packet)
        {
            int npcId = packet.ReadInt();
            BeautyCategory category = (BeautyCategory) packet.ReadByte();

            NpcMetadata beautyNpc = NpcMetadataStorage.GetNpc(npcId);
            if (beautyNpc == null)
            {
                return;
            }

            BeautyMetadata beautyShop = BeautyMetadataStorage.GetShopById(beautyNpc.ShopId);
            if (beautyShop == null)
            {
                return;
            }

            if (beautyShop.BeautyCategory == BeautyCategory.Dye)
            {
                if (beautyShop.BeautyType == BeautyShopType.Dye)
                {
                    session.Send(BeautyPacket.LoadDyeShop(beautyShop));
                }
                else
                {
                    session.Send(BeautyPacket.LoadBeautyShop(beautyShop));
                }
                return;
            }

            if (beautyShop.BeautyCategory == BeautyCategory.Save)
            {
                session.Send(BeautyPacket.LoadSaveShop(beautyShop));
                session.Send(BeautyPacket.InitializeSaves());
                session.Send(BeautyPacket.LoadSaveWindow());
                session.Send(BeautyPacket.LoadSavedHairCount((short) session.Player.HairInventory.SavedHair.Count));
                if (session.Player.HairInventory.SavedHair.Count != 0)
                {
                    session.Player.HairInventory.SavedHair = session.Player.HairInventory.SavedHair.OrderBy(o => o.CreationTime).ToList();
                    session.Send(BeautyPacket.LoadSavedHairs(session.Player.HairInventory.SavedHair));
                }

                return;
            }

            List<BeautyItem> beautyItems = BeautyMetadataStorage.GetGenderItems(beautyShop.ShopId, session.Player.Gender);

            session.Send(BeautyPacket.LoadBeautyShop(beautyShop, beautyItems));
        }

        private static void HandleNewBeauty(GameSession session, PacketReader packet)
        {
            byte unk = packet.ReadByte();
            bool useVoucher = packet.ReadBool();
            int beautyItemId = packet.ReadInt();
            EquipColor equipColor = packet.Read<EquipColor>();
            int colorIndex = packet.ReadInt();

            Item beautyItem = new Item(beautyItemId) { Color = equipColor, IsTemplate = false };
            BeautyMetadata beautyShop = BeautyMetadataStorage.GetCosmeticShopByItemId(beautyItem.Id);

            if (useVoucher)
            {
                PayWithVoucher(session, beautyShop);
            }
            else
            {
                PayWithShopItemTokenCost(session, beautyItemId);
            }

            ModifyBeauty(session, packet, beautyItem);
        }

        private static void HandleModifyExistingBeauty(GameSession session, PacketReader packet)
        {
            byte unk = packet.ReadByte();
            bool useVoucher = packet.ReadBool();
            long beautyItemUid = packet.ReadLong();
            EquipColor equipColor = packet.Read<EquipColor>();
            int colorIndex = packet.ReadInt();

            Item beautyItem = session.Player.Equips.FirstOrDefault(x => x.Value.Uid == beautyItemUid).Value;
            BeautyMetadata beautyShop = BeautyMetadataStorage.GetCosmeticShopByItemId(beautyItem.Id);

            if (useVoucher)
            {
                PayWithVoucher(session, beautyShop);
            }
            else
            {
                PayWithShopTokenCost(session, beautyShop);
            }

            beautyItem.Color = equipColor;
            ModifyBeauty(session, packet, beautyItem);
        }

        private static void HandleModifySkin(GameSession session, PacketReader packet)
        {
            byte unk = packet.ReadByte();
            SkinColor skinColor = packet.Read<SkinColor>();
            bool useVoucher = packet.ReadBool();

            BeautyMetadata beautyShop = BeautyMetadataStorage.GetShopById(501);

            if (useVoucher)
            {
                PayWithVoucher(session, beautyShop);
            }
            else
            {
                PayWithShopTokenCost(session, beautyShop);
            }

            session.Player.SkinColor = skinColor;
            session.FieldManager.BroadcastPacket(SkinColorPacket.Update(session.FieldPlayer, skinColor));
        }
        private static void HandleRandomHair(GameSession session, PacketReader packet)
        {
            int shopId = packet.ReadInt();
            bool useVoucher = packet.ReadBool();

            BeautyMetadata beautyShop = BeautyMetadataStorage.GetShopById(shopId);
            List<BeautyItem> beautyItems = BeautyMetadataStorage.GetGenderItems(beautyShop.ShopId, session.Player.Gender);

            if (useVoucher)
            {
                PayWithVoucher(session, beautyShop);
            }
            else
            {
                PayWithShopTokenCost(session, beautyShop);
            }

            // Grab random hair
            Random randomHair = new Random();
            int indexHair = randomHair.Next(beautyItems.Count);
            BeautyItem chosenHair = beautyItems[indexHair];

            //Grab a preset hair and length of hair
            ItemMetadata beautyItemData = ItemMetadataStorage.GetMetadata(chosenHair.ItemId);
            Random randomPreset = new Random();
            int indexPreset = randomPreset.Next(beautyItemData.HairPresets.Count);
            HairPresets chosenPreset = beautyItemData.HairPresets[indexPreset];

            //Grab random front hair length
            Random randomFrontLength = new Random();
            double chosenFrontLength = randomFrontLength.NextDouble() *
                (beautyItemData.HairPresets[indexPreset].MaxScale - beautyItemData.HairPresets[indexPreset].MinScale) + beautyItemData.HairPresets[indexPreset].MinScale;

            //Grab random back hair length
            Random randomBackLength = new Random();
            double chosenBackLength = randomBackLength.NextDouble() *
                (beautyItemData.HairPresets[indexPreset].MaxScale - beautyItemData.HairPresets[indexPreset].MinScale) + beautyItemData.HairPresets[indexPreset].MinScale;

            // Grab random preset color
            ColorPaletteMetadata palette = ColorPaletteMetadataStorage.GetMetadata(2); // pick from palette 2. Seems like it's the correct palette for basic hair colors

            Random randomColor = new Random();
            int indexColor = randomColor.Next(palette.DefaultColors.Count);
            EquipColor color = palette.DefaultColors[indexColor];

            Dictionary<ItemSlot, Item> equippedInventory = session.Player.GetEquippedInventory(InventoryTab.Gear);

            Item newHair = new Item(chosenHair.ItemId)
            {
                Color = color,
                HairD = new HairData((float) chosenBackLength, (float) chosenFrontLength, chosenPreset.BackPositionCoord, chosenPreset.BackPositionRotation, chosenPreset.FrontPositionCoord, chosenPreset.FrontPositionRotation),
                IsTemplate = false
            };

            //Remove old hair
            if (session.Player.Equips.Remove(ItemSlot.HR, out Item previousHair))
            {
                previousHair.Slot = -1;
                session.Player.HairInventory.RandomHair = previousHair; // store the previous hair
                session.FieldManager.BroadcastPacket(EquipmentPacket.UnequipItem(session.FieldPlayer, previousHair));
            }

            equippedInventory[ItemSlot.HR] = newHair;

            session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, newHair, ItemSlot.HR));
            session.Send(BeautyPacket.RandomHairOption(previousHair, newHair));
        }

        private static void HandleChooseRandomHair(GameSession session, PacketReader packet)
        {
            byte selection = packet.ReadByte();

            if (selection == 0) // player chose previous hair
            {
                Dictionary<ItemSlot, Item> equippedInventory = session.Player.GetEquippedInventory(InventoryTab.Gear);

                //Remove current hair
                if (session.Player.Equips.Remove(ItemSlot.HR, out Item newHair))
                {
                    newHair.Slot = -1;
                    session.FieldManager.BroadcastPacket(EquipmentPacket.UnequipItem(session.FieldPlayer, newHair));
                }

                equippedInventory[ItemSlot.HR] = session.Player.HairInventory.RandomHair; // apply the previous hair

                session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, session.Player.HairInventory.RandomHair, ItemSlot.HR));

                Item voucher = new Item(20300246) { }; // Chic Salon Voucher
                InventoryController.Add(session, voucher, true);

                session.Send(BeautyPacket.ChooseRandomHair(voucher.Id));
            }
            else // player chose new hair
            {
                session.Send(BeautyPacket.ChooseRandomHair());
            }

            session.Player.HairInventory.RandomHair = null; // remove random hair option from hair inventory
        }

        private static void HandleSaveHair(GameSession session, PacketReader packet)
        {
            long hairUid = packet.ReadLong();

            Item hair = session.Player.Equips.FirstOrDefault(x => x.Value.Uid == hairUid).Value;
            if (hair == null || hair.ItemSlot != ItemSlot.HR)
            {
                return;
            }

            if (session.Player.HairInventory.SavedHair.Count > 30) // 30 is the max slots
            {
                return;
            }

            Item hairCopy = new Item(hair.Id)
            {
                HairD = hair.HairD,
                Color = hair.Color,
                CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + AccountStorage.TickCount
            };

            session.Player.HairInventory.SavedHair.Add(hairCopy);

            session.Send(BeautyPacket.SaveHair(hair, hairCopy));
        }

        private static void HandleDeleteSavedHair(GameSession session, PacketReader packet)
        {
            long hairUid = packet.ReadLong();

            Item hair = session.Player.HairInventory.SavedHair.FirstOrDefault(x => x.Uid == hairUid);
            if (hair == null)
            {
                return;
            }

            session.Send(BeautyPacket.DeleteSavedHair(hair.Uid));
            session.Player.HairInventory.SavedHair.Remove(hair);
        }

        private static void HandleChangeToSavedHair(GameSession session, PacketReader packet)
        {
            long hairUid = packet.ReadLong();

            Item hair = session.Player.HairInventory.SavedHair.FirstOrDefault(x => x.Uid == hairUid);
            if (hair == null)
            {
                return;
            }

            BeautyMetadata beautyShop = BeautyMetadataStorage.GetShopById(510);

            PayWithShopTokenCost(session, beautyShop);

            if (session.Player.Equips.Remove(hair.ItemSlot, out Item removeItem))
            {
                removeItem.Slot = -1;
                session.FieldManager.BroadcastPacket(EquipmentPacket.UnequipItem(session.FieldPlayer, removeItem));
            }

            Dictionary<ItemSlot, Item> equippedInventory = session.Player.GetEquippedInventory(InventoryTab.Gear);

            equippedInventory[removeItem.ItemSlot] = hair;

            session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, hair, hair.ItemSlot));
            session.Send(BeautyPacket.ChangetoSavedHair());
        }

        private static void HandleDyeItem(GameSession session, PacketReader packet)
        {
            BeautyMetadata beautyShop = BeautyMetadataStorage.GetShopById(506);

            byte itemCount = packet.ReadByte();

            short[] quantity = new short[itemCount];
            bool[] useVoucher = new bool[itemCount];
            byte[] unk1 = new byte[itemCount];
            long[] unk2 = new long[itemCount];
            int[] unk3 = new int[itemCount];
            long[] itemUid = new long[itemCount];
            int[] itemId = new int[itemCount];
            EquipColor[] equipColor = new EquipColor[itemCount];
            int[] paletteId = new int[itemCount];
            CoordF[] hatXPosition = new CoordF[itemCount];
            CoordF[] hatYPosition = new CoordF[itemCount];
            CoordF[] hatZPosition = new CoordF[itemCount];
            CoordF[] hatRotation = new CoordF[itemCount];
            int[] hatScale = new int[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                quantity[i] = packet.ReadShort(); // should always be one
                useVoucher[i] = packet.ReadBool();
                unk1[i] = packet.ReadByte(); // just 0
                unk2[i] = packet.ReadLong(); // just 0
                unk3[i] = packet.ReadInt(); // also 0
                itemUid[i] = packet.ReadLong();
                itemId[i] = packet.ReadInt();
                equipColor[i] = packet.Read<EquipColor>();
                paletteId[i] = packet.ReadInt();
                Item item = session.Player.GetEquippedItem(itemUid[i]);
                if (item == null)
                {
                    return;
                }

                if (item.ItemSlot == ItemSlot.CP)
                {
                    hatXPosition[i] = packet.Read<CoordF>(); // TODO: implement correct hat positioning
                    hatYPosition[i] = packet.Read<CoordF>();
                    hatZPosition[i] = packet.Read<CoordF>();
                    hatRotation[i] = packet.Read<CoordF>();
                    hatScale[i] = packet.ReadInt();
                }

                if (useVoucher[i])
                {
                    PayWithVoucher(session, beautyShop);
                }
                else
                {
                    PayWithShopTokenCost(session, beautyShop);
                }

                item.Color = equipColor[i];
                session.FieldManager.BroadcastPacket(ItemExtraDataPacket.Update(session.FieldPlayer, item));
            }
        }

        private static void ModifyBeauty(GameSession session, PacketReader packet, Item beautyItem)
        {
            ItemSlot itemSlot = ItemMetadataStorage.GetSlot(beautyItem.Id);

            // remove current item
            if (session.Player.Equips.Remove(itemSlot, out Item removeItem))
            {
                removeItem.Slot = -1;
                session.FieldManager.BroadcastPacket(EquipmentPacket.UnequipItem(session.FieldPlayer, removeItem));
            }
            // equip new item

            Dictionary<ItemSlot, Item> equippedInventory = session.Player.GetEquippedInventory(InventoryTab.Gear);

            switch (itemSlot)
            {
                case ItemSlot.HR:
                    float backLength = BitConverter.ToSingle(packet.Read(4), 0);
                    CoordF backPositionCoord = packet.Read<CoordF>();
                    CoordF backPositionRotation = packet.Read<CoordF>();
                    float frontLength = BitConverter.ToSingle(packet.Read(4), 0);
                    CoordF frontPositionCoord = packet.Read<CoordF>();
                    CoordF frontPositionRotation = packet.Read<CoordF>();

                    beautyItem.HairD = new HairData(backLength, frontLength, backPositionCoord, backPositionRotation, frontPositionCoord, frontPositionRotation);

                    equippedInventory[itemSlot] = beautyItem;

                    session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, beautyItem, itemSlot));
                    break;
                case ItemSlot.FA:

                    equippedInventory[itemSlot] = beautyItem;

                    session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, beautyItem, itemSlot));
                    break;
                case ItemSlot.FD:
                    byte[] faceDecorationPosition = packet.Read(16);

                    beautyItem.FaceDecorationD = faceDecorationPosition;

                    equippedInventory[itemSlot] = beautyItem;

                    session.FieldManager.BroadcastPacket(EquipmentPacket.EquipItem(session.FieldPlayer, beautyItem, itemSlot));
                    break;
            }
        }

        private static void PayWithVoucher(GameSession session, BeautyMetadata shop)
        {
            string voucherTag = ""; // using an Item's tag to search for any applicable voucher
            switch (shop.BeautyType)
            {
                case BeautyShopType.Hair:
                    if (shop.BeautyCategory == BeautyCategory.Special)
                    {
                        voucherTag = "beauty_hair_special";
                        break;
                    }
                    voucherTag = "beauty_hair";
                    break;
                case BeautyShopType.Face:
                    voucherTag = "beauty_face";
                    break;
                case BeautyShopType.Makeup:
                    voucherTag = "beauty_makeup";
                    break;
                case BeautyShopType.Skin:
                    voucherTag = "beauty_skin";
                    break;
                case BeautyShopType.Dye:
                    voucherTag = "beauty_itemcolor";
                    break;
                default:
                    session.Send(NoticePacket.Notice("Unknown Beauty Shop", NoticeType.FastText));
                    return;
            }

            Item voucher = session.Player.Inventory.Items.FirstOrDefault(x => x.Value.Tag == voucherTag).Value;
            if (voucher == null)
            {
                session.Send(NoticePacket.Notice(SystemNotice.ItemNotFound, NoticeType.FastText));
                return;
            }

            session.Send(BeautyPacket.UseVoucher(voucher.Id, 1));
            InventoryController.Consume(session, voucher.Uid, 1);
        }

        private static void PayWithShopTokenCost(GameSession session, BeautyMetadata beautyShop)
        {
            int cost;
            if (beautyShop.SpecialCost != 0)
            {
                cost = beautyShop.SpecialCost;
            }
            else
            {
                cost = beautyShop.TokenCost;
            }

            switch (beautyShop.TokenType)
            {
                case ShopCurrencyType.Meso:
                    session.Player.Wallet.Meso.Modify(-(cost));
                    break;
                case ShopCurrencyType.ValorToken:
                    session.Player.Wallet.ValorToken.Modify(-(cost));
                    break;
                case ShopCurrencyType.Treva:
                    session.Player.Wallet.Treva.Modify(-(cost));
                    break;
                case ShopCurrencyType.Rue:
                    session.Player.Wallet.Rue.Modify(-(cost));
                    break;
                case ShopCurrencyType.HaviFruit:
                    session.Player.Wallet.HaviFruit.Modify(-(cost));
                    break;
                case ShopCurrencyType.Meret:
                case ShopCurrencyType.GameMeret:
                    session.Player.Wallet.Meret.Modify(-(cost));
                    break;
                case ShopCurrencyType.EventMeret:
                    session.Player.Wallet.RemoveMerets(cost);
                    break;
                case ShopCurrencyType.Item:
                    Item itemCost = session.Player.Inventory.Items.FirstOrDefault(x => x.Value.Id == beautyShop.RequiredItemId).Value;
                    if (itemCost.Amount < cost)
                    {
                        return;
                    }
                    InventoryController.Consume(session, itemCost.Uid, cost);
                    break;
                default:
                    session.SendNotice($"Unknown currency: {beautyShop.TokenType}");
                    break;
            }
        }

        private static void PayWithShopItemTokenCost(GameSession session, int beautyItemId)
        {
            BeautyMetadata beautyShop = BeautyMetadataStorage.GetCosmeticShopByItemId(beautyItemId);
            BeautyItem item = beautyShop.Items.FirstOrDefault(x => x.ItemId == beautyItemId);

            switch (item.TokenType)
            {
                case ShopCurrencyType.Meso:
                    session.Player.Wallet.Meso.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.ValorToken:
                    session.Player.Wallet.ValorToken.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.Treva:
                    session.Player.Wallet.Treva.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.Rue:
                    session.Player.Wallet.Rue.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.HaviFruit:
                    session.Player.Wallet.HaviFruit.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.Meret:
                case ShopCurrencyType.GameMeret:
                    session.Player.Wallet.Meret.Modify(-(item.TokenCost));
                    break;
                case ShopCurrencyType.EventMeret:
                    session.Player.Wallet.RemoveMerets(item.TokenCost);
                    break;
                case ShopCurrencyType.Item:
                    Item itemCost = session.Player.Inventory.Items.FirstOrDefault(x => x.Value.Id == item.RequiredItemId).Value;
                    if (itemCost.Amount < item.TokenCost)
                    {
                        return;
                    }
                    InventoryController.Consume(session, itemCost.Uid, item.TokenCost);
                    break;
                default:
                    session.SendNotice($"Unknown currency: {item.TokenType}");
                    break;
            }
        }
    }
}
