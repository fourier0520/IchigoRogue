using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {

    [System.Serializable]
    public class ItemProfileData
    {
        public List<ItemProfile> ItemProfileList;
    }

    public List<Item> ItemTempleteList;
    public List<ItemProfile> ItemProfileList;
    static public ItemManager instance = null;

    private ItemProfileData Data = new ItemProfileData();

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        Data.ItemProfileList = ItemProfileList;
        //NKTextMan.saveText("/ItemProfiles_.json", JsonUtility.ToJson(Data, true));
        string json = SaveManager.LoadFixedJson("_Data/ItemProfiles");
        JsonUtility.FromJsonOverwrite(json, Data);
        ItemProfileList = Data.ItemProfileList;
        foreach (ItemProfile i in ItemProfileList) i.ParseStr();

        this.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public ItemProfile GetItemProfileFromID(string ID)
    {
        for (int i = 0; i < ItemProfileList.Count; i++)
        {
            if (ItemProfileList[i].ID == ID)
            {
                return ItemProfileList[i];
            }
        }
        return null;
    }

    public Item GetItemTempleteFromGfxId (string ID)
    {
        foreach (Item t in ItemTempleteList)
        {
            if(t.Profile.GfxID == ID)
            {
                return t;
            }
        }
        return ItemTempleteList[0];
    }

    public Item GenerateItemFromNode(ItemNode node, Vector2 pos)
    {
        ItemProfile p = GetItemProfileFromID(node.ID);
        if (p == null) return null;

        Item item = Instantiate(GetItemTempleteFromGfxId (p.GfxID), pos, new Quaternion());
        item.Node = new ItemNode(node);
        item.Profile = p;
        return item;
    }

    /// <summary>
    /// For Debug
    /// </summary>
    /// <returns></returns>
    public Item GenerateRandomItem(Vector2 pos)
    {
        ItemNode node = new ItemNode(ItemProfileList[Random.Range(0, ItemProfileList.Count)].ID, 1, 1);
        return GenerateItemFromNode(node, pos);
    }
}
