﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Maple2Storage.Types.Metadata
{
    [XmlType]
    public class ItemMetadata
    {
        [XmlElement(Order = 1)]
        public int Id;
        [XmlElement(Order = 2)]
        public ItemSlot Slot;
        [XmlElement(Order = 3)]
        public GemSlot Gem;
        [XmlElement(Order = 4)]
        public InventoryTab Tab;
        [XmlElement(Order = 5)]
        public int Rarity;
        [XmlElement(Order = 6)]
        public int StackLimit;
        [XmlElement(Order = 7)]
        public bool EnableBreak;
        [XmlElement(Order = 8)]
        public bool IsTwoHand;
        [XmlElement(Order = 9)]
        public bool IsDress;
        [XmlElement(Order = 10)]
        public bool IsTemplate;
        [XmlElement(Order = 11)]
        public int PlayCount;
        [XmlElement(Order = 12)]
        public List<int> SellPrice = new List<int>();
        [XmlElement(Order = 13)]
        public List<int> SellPriceCustom = new List<int>();
        [XmlElement(Order = 14)]
        public string FileName;
        [XmlElement(Order = 15)]
        public int SkillID;
        [XmlElement(Order = 16)]
        public List<int> RecommendJobs = new List<int>();
        [XmlElement(Order = 17)]
        public List<ItemContent> Content;
        [XmlElement(Order = 18)]
        public List<ItemBreakReward> BreakRewards;
        [XmlElement(Order = 19)]
        public string FunctionName;
        [XmlElement(Order = 20)]
        public int FunctionId;
        [XmlElement(Order = 21)]
        public int FunctionDuration;
        [XmlElement(Order = 22)]
        public int FunctionFieldId;
        [XmlElement(Order = 23)]
        public byte FunctionCapacity;
        [XmlElement(Order = 24)]
        public byte FunctionTargetLevel;
        [XmlElement(Order = 25)]
        public short FunctionCount;
        [XmlElement(Order = 26)]
        public byte FunctionTotalUser;
        [XmlElement(Order = 27)]
        public string Tag;
        [XmlElement(Order = 28)]
        public int ShopID;

        // Required for deserialization
        public ItemMetadata()
        {
            Content = new List<ItemContent>();
            BreakRewards = new List<ItemBreakReward>();
        }

        public override string ToString() =>
            $"ItemMetadata(Id:{Id},Slot:{Slot},GemSlot:{Gem},Tab:{Tab},Rarity:{Rarity},StackLimit:{StackLimit},IsTwoHand:{IsTwoHand},IsTemplate:{IsTemplate},PlayCount:{PlayCount},FileName:{FileName}," +
            $"SkillID:{SkillID},RecommendJobs:{string.Join(",", RecommendJobs)},Content:{string.Join(",", Content)},FunctionName:{FunctionName},FunctionId:{FunctionId},FunctionDuration:{FunctionDuration}," +
            $"FunctionFieldId:{FunctionFieldId},FunctionCapacity:{FunctionCapacity},FunctionTargetLevel:{FunctionTargetLevel},FunctionCount:{FunctionCount},FunctionTotalUser:{FunctionTotalUser},Tag:{Tag},ShopID:{ShopID}";

        protected bool Equals(ItemMetadata other)
        {
            return Id == other.Id && Slot == other.Slot && Gem == other.Gem && Tab == other.Tab && Rarity == other.Rarity &&
            StackLimit == other.StackLimit && IsTwoHand == other.IsTwoHand && IsTemplate == other.IsTemplate && PlayCount ==
            other.PlayCount && FileName == other.FileName && SkillID == other.SkillID && Content.SequenceEqual(other.Content);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ItemMetadata) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Slot, Gem, Tab, Rarity, StackLimit);
        }

        public static bool operator ==(ItemMetadata left, ItemMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemMetadata left, ItemMetadata right)
        {
            return !Equals(left, right);
        }
    }

    [XmlType]
    public class ItemContent
    {
        [XmlElement(Order = 1)]
        public readonly int Id;
        [XmlElement(Order = 2)]
        public readonly int Id2;
        [XmlElement(Order = 3)]
        public readonly int MinAmount;
        [XmlElement(Order = 4)]
        public readonly int MaxAmount;
        [XmlElement(Order = 5)]
        public readonly int DropGroup;
        [XmlElement(Order = 6)]
        public readonly int SmartDropRate;
        [XmlElement(Order = 7)]
        public readonly int Rarity;
        [XmlElement(Order = 8)]
        public readonly int EnchantLevel;

        // Required for deserialization
        public ItemContent() { }

        public ItemContent(int id, int minAmount, int maxAmount, int dropGroup, int smartDropRate, int rarity, int enchant, int id2 = 0)
        {
            Id = id;
            Id2 = id2;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            DropGroup = dropGroup;
            SmartDropRate = smartDropRate;
            Rarity = rarity;
            EnchantLevel = enchant;
        }

        public override string ToString() =>
            $"ItemContent(Id:{Id},Id2:{Id2},MinAmount:{MinAmount},MaxAmount:{MaxAmount},DropGroup:{DropGroup},SmartDropRate:{SmartDropRate},Rarity:{Rarity},EnchantLevel:{EnchantLevel})";

        protected bool Equals(ItemContent other)
        {
            return Id == other.Id && Id2 == other.Id2 && MinAmount == other.MinAmount && MaxAmount == other.MaxAmount &&
                   DropGroup == other.DropGroup &&
                   SmartDropRate == other.SmartDropRate && Rarity == other.Rarity &&
                   EnchantLevel == other.EnchantLevel;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ItemContent) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Id2, MinAmount, MaxAmount, DropGroup, SmartDropRate, Rarity, EnchantLevel);
        }

        public static bool operator ==(ItemContent left, ItemContent right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemContent left, ItemContent right)
        {
            return !Equals(left, right);
        }
    }

    [XmlType]
    public class ItemBreakReward
    {
        [XmlElement(Order = 1)]
        public int Id;
        [XmlElement(Order = 2)]
        public int Count;

        public ItemBreakReward() { }

        public ItemBreakReward(int id, int count)
        {
            Id = id;
            Count = count;
        }

        public override string ToString() => $"Id: {Id}, Amount: {Count}";
    }
}
