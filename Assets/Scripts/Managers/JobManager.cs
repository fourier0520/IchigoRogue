using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct Requirement
{
    public string JobID;
    public int Level;
}

[System.Serializable]
public class SkillEffect
{
    public enum EffectType
    {
        NoEffect, UpAttack, UpDodge, UpCrit,
        AddAttackTimes,
        Hit3Way, UpAim,
        UpLife,
    }
    public int Value = 0;
    public EffectType TypeValue = EffectType.NoEffect;
    public string Type = "";

    public void ParseStr()
    {
        if (Type != "")
        {
            TypeValue = (EffectType)System.Enum.Parse(typeof(EffectType), Type);
        }
    }
}

[System.Serializable]
public class SkillData
{
    public string Name = "";
    public string ID = "";
    public bool IsActiveSkill = true;
    
    public List<Requirement> RequireLevel = new List<Requirement>();
    public string RequireCurrentJob = "";
    public List<SkillEffect> SkillEffect = new List<SkillEffect>();

    // デフォルトコンストラクタ
    public SkillData()
    {
    }

    // コピーコンストラクタ
    public SkillData(SkillData source)
    {
        this.Name = source.Name;
        this.ID = source.ID;
        this.IsActiveSkill = source.IsActiveSkill;
    }
}

[System.Serializable]
public class JobData
{
    public string Name = "";
    public string ID = "";
    public int Level = 0;
    int TableType = 0;

    public int GetNextExp(out bool Valid)
    {
        Valid = true;
        int[,] Table = 
            new int[,] {
                { 1000, 1000, 1500, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 7500, 9000, 10500, 12000, 13500 },
                { 500, 1000, 1000, 1500, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 7500, 9000, 10500, 12000 }
            };

        if (Table.GetLength(1) > Level)
        {
            return Table[TableType,Level];
        }
        else
        {
            Valid = false;
            return 0;
        }
    }

    public void JobLevelUp(int value = 1)
    {
        Level += value;
    }

    // デフォルトコンストラクタ
    public JobData()
    {
    }

    // コピーコンストラクタ
    public JobData(JobData source)
    {
        this.Name = source.Name;
        this.ID = source.ID;
        this.Level = source.Level;
        this.TableType = source.TableType;
    }
}

[System.Serializable]
public class JobJsonLoader
{
    public List<JobData> EmptyJobList = new List<JobData>();
    public List<SkillData> ReferenceSkillList = new List<SkillData>();
}

public class JobManager : MonoBehaviour {

    // 職業データの初期化
    public List<JobData> EmptyJobList;
    public List<SkillData> ReferenceSkillList;
    static public JobManager instance = null;

    private JobJsonLoader Loader;

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        this.gameObject.SetActive(false);

        //NKTextMan.saveText("/Jobs_.json", JsonUtility.ToJson(instance, true));

        string json = SaveManager.LoadFixedJson("_Data/Jobs");
        JsonUtility.FromJsonOverwrite(json, instance);
        foreach (SkillData s in instance.ReferenceSkillList)
        {
            foreach(SkillEffect e in s.SkillEffect)
            {
                e.ParseStr();
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    public List<JobData> GetEmptyJobList()
    {
        List<JobData> tmp = new List<JobData>();
        tmp.Clear();

        foreach(JobData j in EmptyJobList)
        {
            tmp.Add(new JobData(j));
        }

        return tmp;
    }

    public List<SkillData> GetUsableSkill(Player player)
    {
        List<JobData> Jobs = player.JobList;
        List<SkillData> tmp = new List<SkillData>();
        tmp.Clear();

        foreach (SkillData s in ReferenceSkillList)
        {
            bool valid = true;
            if (s.RequireCurrentJob != "" && s.RequireCurrentJob != player.CurrentJob.ID) continue;
            for (int i = 0; i < s.RequireLevel.Count; i++)
            {
                foreach (JobData j in Jobs)
                {
                    if (j.ID == s.RequireLevel[i].JobID && j.Level < s.RequireLevel[i].Level)
                    {
                        valid = false;
                        break;
                    }
                }
                // ID重複チェック
                foreach (SkillData t in tmp)
                {
                    if (t.ID == s.ID)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid) break;
            }
            if (valid) tmp.Add(s);
        }

        return tmp;
    }

    public SkillData GetSkillFromID(string ID)
    {
        foreach (SkillData s in ReferenceSkillList)
        {
            if (s.ID == ID) return s;
        }

        return null;
    }
}
