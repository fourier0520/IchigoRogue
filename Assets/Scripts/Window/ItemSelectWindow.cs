using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectWindow : MenuWindow
{
    public GameObject ItemWindow;
    public int ItemSelectWindowIndex = 0;
    public int PreviousItemSelectWindowIndex = -255;
    public Text ItemSelectWindowText;

    Player player;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void MenuSpecificUpdate()
    {
        this.gameObject.SetActive(true);
        int key = 0;

        int ItemWindowIndex = ItemWindow.GetComponent<ItemWindow>().ItemWindowIndex;

        player = Player.instance;

        List<string> ItemSelectCommands = new List<string>();
        ItemSelectCommands.Clear();

        if (player.CheckEquipDest (player.Inventory.Items[ItemWindowIndex]) == Player.EquipDest.None)
        {
            if (player.Inventory.Items[ItemWindowIndex].GetProfile().EqTypeValue == ItemProfile.EquipType.OneHand)
            {
                ItemSelectCommands.Add("装備(右手)");
            }
            if (player.Inventory.Items[ItemWindowIndex].GetProfile().EqTypeValue == ItemProfile.EquipType.OneHandDual)
            {
                ItemSelectCommands.Add("装備(右手)");
                if (player.Equip.RightHand != null)
                    ItemSelectCommands.Add("装備(左手)");
            }
            if (player.Inventory.Items[ItemWindowIndex].GetProfile().EqTypeValue == ItemProfile.EquipType.TwoHand)
            {
                ItemSelectCommands.Add("装備(両手)");
            }
            if (player.Inventory.Items[ItemWindowIndex].GetProfile().EqTypeValue == ItemProfile.EquipType.Armor)
            {
                ItemSelectCommands.Add("装備");
            }
        }
        else
        {
            if (player.Inventory.Items[ItemWindowIndex].GetProfile().EqTypeValue != ItemProfile.EquipType.None)
            {
                ItemSelectCommands.Add("はずす");
            }
        }
        if (player.Inventory.Items[ItemWindowIndex].GetProfile().UseEffectTypeValue != ItemProfile.ItemType.NoEffect)
        {
            ItemSelectCommands.Add("使う");
        }
        ItemSelectCommands.Add("投げる");
        ItemSelectCommands.Add("置く");
        ItemSelectCommands.Add("やめる");

        if (Input.GetAxisRaw("Horizontal") > 0) key = 6;
        else if (Input.GetAxisRaw("Horizontal") < 0) key = 4;
        else if (Input.GetAxisRaw("Vertical") > 0) key = 8;
        else if (Input.GetAxisRaw("Vertical") < 0) key = 2;

        if (Input.GetKeyDown("x"))
        {
            State = WindowState.Inactive;
            this.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKeyDown("z"))
        {
            bool EquipAvailable = player.CheckEquipAvailable(player.Inventory.Items[ItemWindowIndex]);
            if (ItemSelectCommands[ItemSelectWindowIndex] == "装備(右手)")
            {
                if (EquipAvailable)
                {
                    player.Equip.RightHand = player.Inventory.Items[ItemWindowIndex];
                }
                else
                {
                    MessageWindow.instance.ConOut("現在の技能では装備できません。");
                }
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "装備(左手)")
            {
                if (EquipAvailable)
                {
                    if (player.Equip.RightHand != null)
                    {
                        if (player.Equip.RightHand.GetProfile().EqTypeValue == ItemProfile.EquipType.TwoHand
                            || player.Equip.RightHand.GetProfile().EqTypeValue == ItemProfile.EquipType.OneHand)
                        {
                            player.Equip.RightHand = null;
                        }
                    }
                    player.Equip.LeftHand = player.Inventory.Items[ItemWindowIndex];
                }
                else
                {
                    MessageWindow.instance.ConOut("現在の技能では装備できません。");
                }
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "装備(両手)")
            {
                if (EquipAvailable)
                {
                    player.Equip.RightHand = player.Inventory.Items[ItemWindowIndex];
                    player.Equip.LeftHand = null;
                }
                else
                {
                    MessageWindow.instance.ConOut("現在の技能では装備できません。");
                }
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "装備")
            {
                if (EquipAvailable)
                {
                    player.Equip.Armor = player.Inventory.Items[ItemWindowIndex];
                }
                else
                {
                    MessageWindow.instance.ConOut("現在の技能では装備できません。");
                }
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "はずす")
            {
                if (player.Equip.RightHand == player.Inventory.Items[ItemWindowIndex])
                    player.Equip.RightHand = null;
                if (player.Equip.LeftHand == player.Inventory.Items[ItemWindowIndex])
                    player.Equip.LeftHand = null;
                if (player.Equip.Armor == player.Inventory.Items[ItemWindowIndex])
                    player.Equip.Armor = null;
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "使う")
            {
                GameManager.instance.player.UseItem = player.Inventory.Items[ItemWindowIndex];
                MenuWindow.Command = WindowTurnCommand.UseItem;
                State = WindowState.Decide;
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "投げる")
            {
                GameManager.instance.player.ThrowItem = player.Inventory.Items[ItemWindowIndex];
                MenuWindow.Command = WindowTurnCommand.ThrowItem;
                State = WindowState.Decide;
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "置く")
            {
                if (GameManager.instance.CanPutItem(Player.instance))
                {
                    GameManager.instance.player.PutItem = player.Inventory.Items[ItemWindowIndex];
                    MenuWindow.Command = WindowTurnCommand.PutItem;
                    State = WindowState.Decide;
                }
                else
                {
                    MessageWindow.instance.ConOut("ここには置けません。");
                }
            }

            if (ItemSelectCommands[ItemSelectWindowIndex] == "やめる")
            {
                State = WindowState.Inactive;
            }

            return;
        }

        if (key == 2)
        {
            ItemSelectWindowIndex++;
        }

        if (key == 8)
        {
            ItemSelectWindowIndex--;
        }

        if (ItemSelectWindowIndex > ItemSelectCommands.Count - 1)
        {
            ItemSelectWindowIndex = ItemSelectCommands.Count - 1;
        }

        if (ItemSelectWindowIndex < 0)
        {
            ItemSelectWindowIndex = 0;
        }

        if (PreviousItemSelectWindowIndex != ItemSelectWindowIndex)
        {
            PreviousItemSelectWindowIndex = ItemSelectWindowIndex;
            StartCoroutine(KeyRepert(0.1f));
        }

        ItemSelectWindowText.text = "";
        for (int i = 0; i < ItemSelectCommands.Count; i++)
        {
            if (i == ItemSelectWindowIndex) ItemSelectWindowText.text += SelectionTag;
            ItemSelectWindowText.text += ItemSelectCommands[i];
            if (i == ItemSelectWindowIndex) ItemSelectWindowText.text += SelectionTagFooter;
            ItemSelectWindowText.text += "\n";
        }
    }
}
