using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemWindow : MenuWindow {

    public GameObject ItemSelectWindow;
    public int ItemWindowIndex = 0;
    public int PreviousItemWindowIndex = -255;
    public Text ItemWindowText;
    public int ItemsPerPages = 8;
    private int page = 0;
    private int numberofpages = 1;

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

        player = Player.instance;

        int key = 0;
        page = ItemWindowIndex / ItemsPerPages;
        numberofpages = player.Inventory.Items.Count / ItemsPerPages + 1;
        if (player.Inventory.Items.Count % ItemsPerPages == 0
            && player.Inventory.Items.Count != 0)
        {
            numberofpages--;
        }

        if (Input.GetAxisRaw("Horizontal") > 0) key = 6;
        else if (Input.GetAxisRaw("Horizontal") < 0) key = 4;
        else if (Input.GetAxisRaw("Vertical") > 0) key = 8;
        else if (Input.GetAxisRaw("Vertical") < 0) key = 2;

        if (Input.GetKeyDown("x"))
        {
            State = WindowState.Inactive;
            return;
        }

        if (Input.GetKeyDown("z"))
        {
            if (GameManager.instance.player.Inventory.Items.Count <= ItemWindowIndex
                || ItemWindowIndex < 0)
            {
                return;
            }
            State = WindowState.WaitChildren;
            ChildrenWindow = ItemSelectWindow;
            ChildrenWindow.SetActive(true);
            ChildrenWindow.GetComponent<MenuWindow>().State = WindowState.Active;
            if (ChildrenWindow.GetComponent<ItemSelectWindow>())
            {
                ChildrenWindow.GetComponent<ItemSelectWindow>().ItemWindow = this.gameObject;
            }
            return;
        }

        if (key == 2)
        {
            ItemWindowIndex++;
        }

        if (key == 8)
        {
            ItemWindowIndex--;
        }

        if (key == 6)
        {
            ItemWindowIndex += ItemsPerPages;
        }
        if (key == 4)
        {
            ItemWindowIndex -= ItemsPerPages;
        }

        if (ItemWindowIndex > GameManager.instance.player.Inventory.Items.Count - 1)
        {
            ItemWindowIndex = GameManager.instance.player.Inventory.Items.Count - 1;
        }
        if (ItemWindowIndex < 0)
        {
            ItemWindowIndex = 0;
        }

        if (PreviousItemWindowIndex != ItemWindowIndex)
        {
            PreviousItemWindowIndex = ItemWindowIndex;
            page = ItemWindowIndex / ItemsPerPages;
            StartCoroutine(KeyRepert(0.1f));
        }
    }

    protected override void MenuSpecificReflesh()
    {

        ItemWindowText.text = "";
        ItemWindowText.text += "<color=lightblue>Page : ";
        for (int i = 0; i < numberofpages; i++)
        {
            if (page == i) ItemWindowText.text += "[";
            ItemWindowText.text += (i + 1);
            if (page == i) ItemWindowText.text += "]";
            ItemWindowText.text += " ";
        }
        ItemWindowText.text += "</color>";
        ItemWindowText.text += "\n";
        if (GameManager.instance.player.Inventory.Items.Count != 0)
        {
            for (int i = page * ItemsPerPages; i < page * ItemsPerPages + ItemsPerPages; i++)
            {
                if (GameManager.instance.player.Inventory.Items.Count < (i + 1))
                {
                    break;
                }
                if (i == ItemWindowIndex)
                {
                    ItemWindowText.text += SelectionTag;
                }
                if (player.CheckEquipDest(player.Inventory.Items[i]) == MovingObject.EquipDest.RightHand)
                {
                    if (player.Inventory.Items[i].GetProfile().EqTypeValue == ItemProfile.EquipType.OneHand) ItemWindowText.text += "(右手)";
                    else ItemWindowText.text += "(両手)";
                }
                if (player.CheckEquipDest(player.Inventory.Items[i]) == MovingObject.EquipDest.LeftHand)
                {
                    ItemWindowText.text += "(左手)";
                }
                if (player.CheckEquipDest(player.Inventory.Items[i]) == MovingObject.EquipDest.Armor)
                {
                    ItemWindowText.text += "(胴体)";
                }
                ItemWindowText.text += player.Inventory.Items[i].GetProfile().Name;
                if (player.Inventory.Items[i].GetProfile().stackable)
                {
                    ItemWindowText.text += " x" + player.Inventory.Items[i].stack;
                }
                if (i == ItemWindowIndex)
                {
                    ItemWindowText.text += SelectionTagFooter;
                }
                ItemWindowText.text += "\n";
            }
        }

    }
}
