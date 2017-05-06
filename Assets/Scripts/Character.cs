using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI; //UI用に宣言
using System.Collections.Generic;
using System.Linq;

public class Character : MovingObject
{
    //------------------------------------------
    //
    // 一時変数
    //
    //------------------------------------------
    // ライフ回復用
    private float NatureGainLife = 0;

    //------------------------------------------
    //
    // ここから下はデータセーブ対象
    //
    //------------------------------------------
    //職業レベルデータ
    public List<JobData> JobList = null;
    public JobData CurrentJob = null;

    //スキルデータ
    public string ActiveSkillID = "";

    //位置情報
    public string CurrentMapID = "";
    public int CurrentMapLevel = 1;

    public string JsonPath = "/_Save/JsonData/NPC";
    public string JsonFile = "/Noname.json";

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        // 初期値設定
        if (JobList == null || JobList.Count == 0) JobList = JobManager.instance.GetEmptyJobList();
        if (CurrentJob == null) CurrentJob = JobList[0];
        else if (CurrentJob.ID == "") CurrentJob = JobList[0];
    }

    private void PatchSkillEffect(SkillEffect b)
    {
        switch (b.TypeValue)
        {
            case SkillEffect.EffectType.UpAttack:
                Status.AttackBase += b.Value;
                break;
            case SkillEffect.EffectType.UpCrit:
                Status.CritOffset += b.Value;
                break;
            case SkillEffect.EffectType.UpDodge:
                Status.Dodge += b.Value;
                break;
            case SkillEffect.EffectType.UpAim:
                Status.Aim += b.Value;
                break;
            case SkillEffect.EffectType.AddAttackTimes:
                Status.AdditionalAttackTimes += b.Value;
                break;
            case SkillEffect.EffectType.UpLife:
                Status.life_max += b.Value;
                break;
        }
    }

    protected override void UpdateStatus()
    {
        Status.life_max = (Status.level - 1) * 5 + 25;

        Status.AttackBase = CurrentJob.Level + Status.level;
        Status.Dodge = 0;
        Status.Aim = 0;
        Status.Defence = 0;
        Status.CritOffset = 0;
        Status.AdditionalAttackTimes = 0;

        // 装備の適用
        if (Equip.Armor != null)
        {
            ItemProfile tmp = Equip.Armor.GetProfile();
            if (tmp != null)
            {
                Status.Dodge += Equip.Armor.GetProfile().Dodge;
                Status.Defence += Equip.Armor.GetProfile().Defence;
            }
        }

        // スキルの適用
        SkillData ActiveSkill = JobManager.instance.GetSkillFromID(ActiveSkillID);
        List<SkillData> UsableSkill = JobManager.instance.GetUsableSkill(this);

        if (ActiveSkill != null)
        {
            foreach (SkillEffect b in ActiveSkill.SkillEffect)
            {
                PatchSkillEffect(b);
            }
        }

        foreach (SkillData s in UsableSkill)
        {
            if (s.IsActiveSkill) continue;
            foreach (SkillEffect b in s.SkillEffect)
            {
                PatchSkillEffect(b);
            }
        }

        //上限下限処理

        base.UpdateStatus();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void EndTurnProcess()
    {

        base.EndTurnProcess();

        if (Status.life < Status.life_max)
        {
            NatureGainLife += Status.life_max / 100f;
            if (NatureGainLife >= 1f)
            {
                RecoverLife((int)Math.Round(NatureGainLife));
                NatureGainLife -= (float)Math.Round(NatureGainLife);
            }
        }
    }

    public void GetExp(int value)
    {
        Status.exp += value;
        if (MessageWindow.instance) MessageWindow.instance.ConOut(value + "の経験値を獲得\n");
        CheckLevelUp();
    }

    public void CheckLevelUp()
    {
        // 次のレベルまでの経験値の計算式(暫定)
        // ベース経験値 Lv1->2 : 20 それ以降 前のLvの必要経験値 * 1.5
        // Lvが10以降 (Lv * 10)^(Lv/10 + 1)の必要経験値追加
        int NextLevelExp = 20;
        for (int i = 1; i < Status.level; i++)
        {
            NextLevelExp += NextLevelExp * 2;
        }
        if (Status.level >= 10)
        {
            NextLevelExp += (int)Mathf.Pow((NextLevelExp * 10), ((int)(Status.level / 10) + 1));
        }
        if (NextLevelExp <= Status.exp)
        {
            Status.level++;
            int skillexpadd = 500 * (int)(Status.level / 2);
            if (skillexpadd > 5000) skillexpadd = 5000;
            Status.jobexp += skillexpadd;
            MessageWindow.instance.ConOut(Status.Name + "のレベルが上がった！\n");
            MessageWindow.instance.ConOut(skillexpadd + "のスキルポイントを獲得！\n");
        }
    }

    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        return base.AttemptMove<T>(xDir, yDir);
    }

    protected override void OnDestroy()
    {
        MessageWindow.instance.ConOut(Status.Name + "は倒れてしまった！\n");
        base.OnDestroy();
    }

    protected override void OnCantMove<T>(T component)
    {
        throw new NotImplementedException();
    }

    //-----------------------------------------------------------------------//
    //
    // 戦闘判定用
    //
    //-----------------------------------------------------------------------//
    protected override bool BattleCanHit3Way()
    {
        SkillData ActiveSkill = JobManager.instance.GetSkillFromID(ActiveSkillID);
        List<SkillData> UsableSkill = JobManager.instance.GetUsableSkill(this);
        if (ActiveSkill != null)
        {
            foreach (SkillEffect b in ActiveSkill.SkillEffect)
            {
                if (b.TypeValue == SkillEffect.EffectType.Hit3Way) return true;
            }
        }
        foreach (SkillData s in UsableSkill)
        {
            if (s.IsActiveSkill) continue;
            for (int i = 0; i < s.SkillEffect.Count; i++)
            {
                SkillEffect b = s.SkillEffect[i];
                if (b.TypeValue == SkillEffect.EffectType.Hit3Way) return true;
            }
        }
        return false;
    }

    //-----------------------------------------------------------------------//
    //
    // システム管理用
    //
    //-----------------------------------------------------------------------//
    [System.Serializable]
    class SaveData
    {
        // Status
        public CharacterStatus Status;

        // Equip
        public int RightHand = -1;
        public int LeftHand = -1;
        public int Armor = -1;

        // Buffs
        public List<CharacterBuff> Buffs = new List<CharacterBuff>();

        // Items
        public ItemInventory Inventory;

        //職業レベルデータ
        public List<JobData> JobList = new List<JobData>();
        public JobData CurrentJob;

        //スキルデータ
        public string ActiveSkillID = "";

        //位置データ
        public string CurrentMapID = "";
        public int CurrentMapLevel = 1;
        public Vector2 logicalPos = new Vector2();
    }

    public virtual bool HoldCharacterData()
    {
        SaveData save = new SaveData();
        save.Status = Status;
        save.Buffs = Buffs;
        save.Inventory = Inventory;
        save.JobList = JobList;
        save.CurrentJob = CurrentJob;
        save.ActiveSkillID = ActiveSkillID;
        save.CurrentMapID = GameManager.instance.boardScript.MapID;
        save.logicalPos = logicalPos;
        save.CurrentMapLevel = GameManager.instance.boardScript.level;

        // 装備はインスタンスではなくポインタに定義し直す(所持品からなくなったとき不都合が出るので)
        int i = 0;
        foreach (ItemNode n in Inventory.Items)
        {
            if (Equip.RightHand == n) save.RightHand = i;
            if (Equip.LeftHand == n) save.LeftHand = i;
            if (Equip.Armor == n) save.Armor = i;
            i++;
        }

        SaveManager.HoldJson(JsonPath, JsonFile, JsonUtility.ToJson(save, true));
        return true;
    }

    public virtual bool LoadCharacterData()
    {
        SaveData save = new SaveData();
        string json = SaveManager.LoadJson(JsonPath, JsonFile);
        if (json == "") return false;
        save = JsonUtility.FromJson<SaveData>(json);
        Status = save.Status;
        Buffs = save.Buffs;
        Inventory = save.Inventory;
        JobList = save.JobList;
        CurrentJob = save.CurrentJob;
        ActiveSkillID = save.ActiveSkillID;
        CurrentMapID = save.CurrentMapID;
        logicalPos = save.logicalPos;
        CurrentMapLevel = save.CurrentMapLevel;

        // 装備はインスタンスではなくポインタに定義し直す(所持品からなくなったとき不都合が出るので)
        if (save.RightHand >= 0) Equip.RightHand = Inventory.Items[save.RightHand];
        if (save.LeftHand >= 0) Equip.LeftHand = Inventory.Items[save.LeftHand];
        if (save.Armor >= 0) Equip.Armor = Inventory.Items[save.Armor];

        return true;
    }
}
