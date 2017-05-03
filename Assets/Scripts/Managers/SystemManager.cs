using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SystemManager : MonoBehaviour {

    public class SystemGameData
    {
        public string PlayerMapID = "";
        public int PlayerMapLevel = 1;
        public int Time = 0;
    }

    static public SystemGameData GameData = new SystemGameData();

    static public void LoadSystemGameData()
    {
        string json = SaveManager.LoadJson("/_Save/JsonData", "/System.json");
        if(json != "")
        {
            JsonUtility.FromJsonOverwrite(json, GameData);
        }
    }

    static public void HoldSystemGameData()
    {
        GameData.PlayerMapID = GameManager.instance.boardScript.MapID;
        GameData.PlayerMapLevel = GameManager.instance.boardScript.level;

        string json = JsonUtility.ToJson(GameData);
        SaveManager.HoldJson("/_Save/JsonData", "/System.json", json);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
