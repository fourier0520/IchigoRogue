using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicManager : MonoBehaviour {

    [System.Serializable]
    public class MagicProfileData
    {
        public List<MagicProfile> MagicProfileList;
    }

    public List<Magic> MagicTempleteList;
    public List<MagicProfile> MagicProfileList;
    static public MagicManager instance = null;

    private MagicProfileData Data = new MagicProfileData();

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        Data.MagicProfileList = MagicProfileList;
        //NKTextMan.saveText("/MagicProfiles_.json", JsonUtility.ToJson(Data, true));
        string json = SaveManager.LoadFixedJson("_Data/MagicProfiles");
        JsonUtility.FromJsonOverwrite(json, Data);
        MagicProfileList = Data.MagicProfileList;
        foreach (MagicProfile i in MagicProfileList) i.ParseStr();

        this.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public MagicProfile GetMagicProfileFromID(string ID)
    {
        for (int i = 0; i < MagicProfileList.Count; i++)
        {
            if (MagicProfileList[i].ID == ID)
            {
                return MagicProfileList[i];
            }
        }
        return null;
    }

    public Magic GetMagicTempleteFromGfxId(string ID)
    {
        foreach (Magic t in MagicTempleteList)
        {
            if (t.Profile.GfxID == ID)
            {
                return t;
            }
        }
        return MagicTempleteList[0];
    }

    public Magic GenerateMagicFromID(string id, Vector2 pos)
    {
        MagicProfile p = GetMagicProfileFromID(id);
        if (p == null) return null;

        Magic magic = Instantiate(GetMagicTempleteFromGfxId(p.GfxID), pos, new Quaternion());
        magic.Profile = p;
        return magic;
    }
}

