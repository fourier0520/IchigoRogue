using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI; //UI用に宣言
using System.Collections.Generic;
using System.Linq;

public class Player : MovingObject
{
    public static Player instance = null;

    public Camera mainCamera;
    private int cameraYOffset = -1;
    private Animator playerAnimator;
    public bool isKeyRepeat = false;
    private bool isKeyRepeatReserve = false;

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

    protected override void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        playerAnimator = GetComponent<Animator>();
        mainCamera = Camera.main;

        base.Start();
        playerAnimator.SetInteger("Direction", GetDirectionNumber());

        // 初期値設定
        if (JobList == null || JobList.Count == 0) JobList = JobManager.instance.GetEmptyJobList();
        if (CurrentJob == null) CurrentJob = JobList[0];
        else if (CurrentJob.ID == "") CurrentJob = JobList[0];
    }

    protected override void UpdateStatus()
    {
        //ベース計算
        /*
        Status.level = 0;
        foreach (JobData Job in JobList)
        {
            if (Status.level < Job.Level) Status.level = Job.Level;
        }*/

        Status.life_max = (Status.level - 1) * 5 + 25;

        Status.AttackBase = CurrentJob.Level + Status.level;
        Status.Dodge = 0;
        Status.Aim = 0;
        Status.Defence = 0;
        Status.CritOffset = 0;
        Status.AdditionalAttackTimes = 0;

        // 装備の適用
        if(Equip.Armor != null)
        {
            ItemProfile tmp = Equip.Armor.GetProfile();
            if(tmp != null)
            {
                Status.Dodge += Equip.Armor.GetProfile().Dodge;
                Status.Defence += Equip.Armor.GetProfile().Defence;
            }
        }

        // スキルの適用
        SkillData ActiveSkill = JobManager.instance.GetSkillFromID(ActiveSkillID);
        List<SkillData> UsableSkill = JobManager.instance.GetUsableSkill(this);

        if(ActiveSkill != null)
        {
            foreach (SkillEffect b in ActiveSkill.SkillEffect)
            {
                switch (b.Type)
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
                }
            }
        }

        foreach(SkillData s in UsableSkill)
        {
            if (s.IsActiveSkill) continue;
            for (int i = 0; i < s.SkillEffect.Count; i++)
            {
                SkillEffect b = s.SkillEffect[i];
                switch (b.Type)
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
                }
            }
        }

        //上限下限処理

        base.UpdateStatus();
    }

    protected override void Update()
    {
        mainCamera.transform.position = new Vector3(
                gameObject.transform.position.x,
                gameObject.transform.position.y + cameraYOffset,
                gameObject.transform.position.z - 10);

        base.Update();

        if (isKeyRepeat) return;
        //プレイヤーの順番じゃない時Updateは実行しない
        if (!GameManager.instance.playersTurn)
            return;
        if (isMovingPre || isAttackingAnimation)
            return;
        if (GameManager.instance.MainWindow.State == MenuWindow.WindowState.Active
            || GameManager.instance.MainWindow.State == MenuWindow.WindowState.WaitChildren)
        {
            isKeyRepeatReserve = true;
            return;
        }

        if (isKeyRepeatReserve)
        {
            isKeyRepeatReserve = false;
            StartCoroutine(KeyRepert(0.2f));
            return;
        }

        int x = 0;
        int y = 0;
        bool endTurn = false;
        bool isAttack = false;
        SetCommand(TurnCommand.Undef);

        // メニュー呼び出し

        if (Input.GetKeyDown("x"))
        {
            GameManager.instance.MainWindow.gameObject.SetActive(true);
            GameManager.instance.MainWindow.State = MenuWindow.WindowState.Active;
            return;
        }

        if (MenuWindow.Command == MenuWindow.WindowTurnCommand.UseItem)
        {
            MenuWindow.Command = MenuWindow.WindowTurnCommand.Undef;
            SetCommand(TurnCommand.UseItem);
        }
        if (MenuWindow.Command == MenuWindow.WindowTurnCommand.ThrowItem)
        {
            SetAttackLine(GetDirection() * 5);
            AnimationType = AttackAnimationType.Throw;
            MenuWindow.Command = MenuWindow.WindowTurnCommand.Undef;
            SetCommand(TurnCommand.ThrowItem);
        }
        if (MenuWindow.Command == MenuWindow.WindowTurnCommand.PutItem)
        {
            MenuWindow.Command = MenuWindow.WindowTurnCommand.Undef;
            SetCommand(TurnCommand.PutItem);
        }
        if (MenuWindow.Command == MenuWindow.WindowTurnCommand.MoveMap)
        {
            MenuWindow.Command = MenuWindow.WindowTurnCommand.Undef;
            SetCommand(TurnCommand.MoveMap);
        }

        if (GetCommand() == TurnCommand.Undef)
        {
            // 右・左
            x = (int)Input.GetAxisRaw("Horizontal");

            // 上・下
            y = (int)Input.GetAxisRaw("Vertical");

            if (x == 0 && y == 0)
            {
                if (Input.GetKey(KeyCode.Keypad7))
                {
                    x = -1;
                    y = 1;
                }
                if (Input.GetKey(KeyCode.Keypad4))
                {
                    x = -1;
                }
                if (Input.GetKey(KeyCode.Keypad1))
                {
                    x = -1;
                    y = -1;
                }
                if (Input.GetKey(KeyCode.Keypad2))
                {
                    y = -1;
                }
                if (Input.GetKey(KeyCode.Keypad3))
                {
                    x = 1;
                    y = -1;
                }
                if (Input.GetKey(KeyCode.Keypad6))
                {
                    x = 1;
                }
                if (Input.GetKey(KeyCode.Keypad9))
                {
                    x = 1;
                    y = 1;
                }
                if (Input.GetKey(KeyCode.Keypad8))
                {
                    y = 1;
                }
            }

            //上下左右どれかに移動する時
            if (((x != 0 || y != 0) && !Input.GetKey("v")) || (x != 0 && y != 0 && Input.GetKey("v")))
            {
                SetDirection(new Vector2(x, y));
                endTurn = AttemptMove<Wall>(x, y);
                if (endTurn && !Input.GetKey("c"))
                {
                    SetCommand(TurnCommand.Move);
                }
            }
            playerAnimator.SetInteger("Direction", GetDirectionNumber());
        }

        if (GetCommand() == TurnCommand.Undef)
        {
            isAttack = Input.GetKey("z");
            if (isAttack)
            {
                SetAttackLine(GetDirection());
                AnimationType = AttackAnimationType.Normal;
                SetCommand(TurnCommand.Attack);
            }
        }

        if (GetCommand() == TurnCommand.Undef)
        {
            isAttack = Input.GetKey("f");
            if (isAttack)
            {

                SetAttackLine(GetDirection() * 5);
                AnimationType = AttackAnimationType.Throw;
                SetCommand(TurnCommand.ThrowItem);
            }
        }

        //プレイヤーの順番終了
        if (GetCommand() != TurnCommand.Undef)
        {
            GameManager.instance.playersTurn = false;
        }
    }

    public override void EndTurnProcess()
    {

        base.EndTurnProcess();

        ReduceFood(5);

        if(Status.life < Status.life_max)
        {
            NatureGainLife += Status.life_max / 100f;
            if (NatureGainLife >= 1f)
            {
                RecoverLife((int)Math.Round(NatureGainLife));
                NatureGainLife -= (float)Math.Round(NatureGainLife);
            }
        }
    }

    public void ReduceFood(int value)
    {
        if(Status.food > value - 1)
        {
            Status.food -= value;
        }
        else
        {
            ReduceLife(1);
        }
    }

    public void GainFood(int value)
    {
        Status.food += value;
        if (Status.food > Status.food_max)
        {
            Status.food = Status.food_max;
        }
    }

    public void GetExp(int value)
    {
        Status.exp += value;
        if(MessageWindow.instance) MessageWindow.instance.ConOut(value + "の経験値を獲得\n");
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
                if (b.Type == SkillEffect.EffectType.Hit3Way) return true;
            }
        }
        foreach (SkillData s in UsableSkill)
        {
            if (s.IsActiveSkill) continue;
            for (int i = 0; i < s.SkillEffect.Count; i++)
            {
                SkillEffect b = s.SkillEffect[i];
                if (b.Type == SkillEffect.EffectType.Hit3Way) return true;
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

    public virtual bool HoldPlayerData()
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

        SaveManager.HoldJson("/_Save/JsonData" , "/player.json", JsonUtility.ToJson(save, true));
        return true;
    }

    public virtual bool LoadObjectData()
    {
        SaveData save = new SaveData();
        string json = SaveManager.LoadJson("/_Save/JsonData" ,"/player.json");
        if (json == "") return false;
        save = JsonUtility.FromJson<SaveData>(json);
        
        Status = save.Status;
        Buffs = save.Buffs;
        Inventory = save.Inventory;
#if false
        //Jobは増えたり減ったりする可能性あり
        List<JobData> EmptyJobList = JobManager.instance.GetEmptyJobList();
        for(int i = 0;  i <EmptyJobList.Count; i++)
        {
            foreach(JobData j2 in save.JobList)
            {
                if (EmptyJobList[i].ID == j2.ID) EmptyJobList[i] = j2;
            }
        }
        JobList = EmptyJobList;
#else
        JobList = save.JobList;
#endif
        CurrentJob = save.CurrentJob;

        ActiveSkillID = save.ActiveSkillID;
        CurrentMapID = save.CurrentMapID;
        logicalPos = save.logicalPos;
        CurrentMapLevel = save.CurrentMapLevel;

        return true;
    }

    public IEnumerator KeyRepert(float time)
    {
        isKeyRepeat = true;
        yield return new WaitForSeconds(time);
        isKeyRepeat = false;
    }
}