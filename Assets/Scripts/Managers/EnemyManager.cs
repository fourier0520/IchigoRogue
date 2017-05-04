using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {

    [System.Serializable]
    public class ReferenceEnemyData
    {
        public List<CharacterStatus> ReferenceEnemies;
    }

    public List<Enemy> EnemiyTempletes;
    public List<CharacterStatus> ReferenceEnemies;
    static public EnemyManager instance = null;

    private ReferenceEnemyData Data = new ReferenceEnemyData();

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }

        Data.ReferenceEnemies = ReferenceEnemies;
        //NKTextMan.saveText("/ReferenceEnemies_.json", JsonUtility.ToJson(Data, true));
        string json = SaveManager.LoadFixedJson("_Data/ReferenceEnemies");
        JsonUtility.FromJsonOverwrite(json, Data);
        ReferenceEnemies = Data.ReferenceEnemies;
        foreach (CharacterStatus i in ReferenceEnemies) i.ParseStr();

        this.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public Enemy GetEnemyTempleteFromGfxId(string id)
    {
        foreach (Enemy e in EnemiyTempletes)
        {
            if (e.Status.GfxId == id) return e;
        }
        return EnemiyTempletes[0];
    }

    public Enemy GenerateEnemyFromID(string id, Vector2 pos)
    {
        foreach (CharacterStatus c in ReferenceEnemies)
        {
            if (c.ID == id) {
                Enemy tmp = Instantiate(GetEnemyTempleteFromGfxId(c.GfxId), pos, GetEnemyTempleteFromGfxId(c.GfxId).transform.rotation);
                tmp.Status = new CharacterStatus(c);
                return tmp;
            }
        }
        return null;
    }

    public Enemy GenerateEnemyFromStatus(CharacterStatus s, Vector2 pos)
    {
        Enemy tmp = Instantiate(GetEnemyTempleteFromGfxId(s.GfxId), pos, GetEnemyTempleteFromGfxId(s.GfxId).transform.rotation);
        tmp.Status = s;
        return tmp;
    }
}
