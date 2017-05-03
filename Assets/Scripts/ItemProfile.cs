using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemNode
{
    public string ID = "";
    public int stack = 0;
    public int UsableTime = 0;

    public ItemProfile GetProfile()
    {
        return ItemManager.instance.GetItemProfileFromID(ID);
    }

    // デフォルトコンストラクタ
    public ItemNode(string ID, int stack, int UsableTime)
    {
        this.ID = ID;
        this.stack = stack;
        this.UsableTime = UsableTime;
    }

    // コピーコンストラクタ
    public ItemNode(ItemNode source)
    {
        this.ID = source.ID;
        this.stack = source.stack;
        this.UsableTime = source.UsableTime;
    }

    public bool Compare(ItemNode source)
    {
        if (source == null) return false;

        if (this.ID != source.ID) return false;
        if (this.UsableTime != source.UsableTime) return false;

        return true;
    }

    public string GetDisplayName()
    {
        return "<color=yellow>" + GetProfile().Name + "</color>";
    }

}

[System.Serializable]
public class ItemInventory
{
    public List<ItemNode> Items;
    public int Size = 40;

    // デフォルトコンストラクタ
    public ItemInventory()
    {
        Items = new List<ItemNode>();
        Items.Clear();
    }

    public bool CheckExistItem(ItemNode source)
    {
        foreach (ItemNode i in Items)
        {
            if (i == source)
            {
                return true;
            }
        }
        return false;
    }

    public bool AddItem(ItemNode source)
    {
        if (source.GetProfile().stackable)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Compare(source))
                {
                    Items[i].stack += source.stack;
                    return true;
                }
            }
            if (Items.Count >= Size) return false;
            Items.Add(source);
        }
        else
        {
            if (Items.Count >= Size) return false;
            Items.Add(source);
        }
        return true;
    }

    public void RemoveItem(ItemNode source, int amount)
    {
        if (source.GetProfile().stackable)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Compare(source))
                {
                    Items[i].stack -= amount;
                    if (Items[i].stack <= 0)
                    {
                        Items.RemoveAt(i);
                    }
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                for (int j = 0; j < Items.Count; j++)
                {
                    if (Items[j].Compare(source))
                    {
                        Items.RemoveAt(j);
                        break;
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class ItemProfile
{
    public enum ItemType
    {
        NoEffect, RecoverLife,
        GainFood,
    }
    public enum EquipType
    {
        None, OneHand, TwoHand, Armor, OneHandDual
    }

    public string Name = "";
    public string ID = "";
    public string GfxID = "";
    public ItemType UseEffectTypeValue = ItemType.NoEffect;
    public string UseEffectType = "NoEffect";
    public ItemType ThrowEffectTypeValue = ItemType.NoEffect;
    public string ThrowEffectType = "NoEffect";
    public EquipType EqTypeValue;
    public string EqType = "None";
    public int UsableTime = 1;
    public int Stack = 1;
    public int EffectValue = 1;
    public int GainFoodValue = 500;
    public string Attribute = "";
    public bool stackable = false;

    public List<string> NGJobIDsList = new List<string>();
    public string NGJobIDs = "";

    // 武器性能
    public int Attack = 0;
    public int AttackCriticalRate = 13;
    public int AttackTimes = 1;
    public int AttackBase = 0;
    public int Aim = 0;

    //防具性能
    public int Defence = 0;
    public int Dodge = 0;

    public void ParseStr()
    {
        if (UseEffectType != "")
        {
            UseEffectTypeValue = (ItemType)System.Enum.Parse(typeof(ItemType), UseEffectType);
        }
        if (ThrowEffectType != "")
        {
            ThrowEffectTypeValue = (ItemType)System.Enum.Parse(typeof(ItemType), ThrowEffectType);
        }
        if (EqType != "")
        {
            EqTypeValue = (EquipType)System.Enum.Parse(typeof(EquipType), EqType);
        }

        foreach (string id in NGJobIDs.Split(',')) NGJobIDsList.Add(id);

        foreach (string attri in Attribute.Split(','))
        {
            switch (attri)
            {
                case "Stackable":
                    stackable = true;
                    break;
                default:
                    break;
            }
        }
    }

    // デフォルトコンストラクタ
    public ItemProfile()
    {
    }

    // コピーコンストラクタ
    public ItemProfile(ItemProfile source)
    {
        this.Name = source.Name;
        this.ID = source.ID;
        this.UseEffectTypeValue = source.UseEffectTypeValue;
        this.ThrowEffectTypeValue = source.ThrowEffectTypeValue;
        this.UsableTime = source.UsableTime;
        this.Stack = source.Stack;
        this.EffectValue = source.EffectValue;
        this.GainFoodValue = source.GainFoodValue;
        this.stackable = source.stackable;

        // 武器性能
        this.Attack = source.Attack;
        this.AttackCriticalRate = source.AttackCriticalRate;
        this.AttackTimes = source.AttackTimes;
    }

    public bool Compare(ItemProfile source)
    {
        if (source == null) return false;

        if (
               this.ID != source.ID
            || this.UseEffectTypeValue != source.UseEffectTypeValue
            || this.ThrowEffectTypeValue != source.ThrowEffectTypeValue
            || this.UsableTime != source.UsableTime
            || this.EffectValue != source.EffectValue
            || this.GainFoodValue != source.GainFoodValue
            || this.stackable != source.stackable

            || this.Attack != source.Attack
            || this.AttackCriticalRate != source.AttackCriticalRate
            || this.AttackTimes != source.AttackTimes
            )
        {
            return false;
        }

        return true;
    }
}
