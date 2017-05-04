using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStatus
{
    public string Name = "";
    public string ID = "";
    public string GfxId = "";
    public int food = 10000;
    public int food_max = 10000;
    public int gold = 0;
    public int exp = 0;
    public int jobexp = 3000;
    public int OriginalActionCommandTime = 60;

    // 戦闘力
    public int level = 1;
    public int life = 5;
    public int life_max = 5;

    public int NwAttack = 5;
    public int NwAttackTimes = 1;
    public int NwAttackCriticalRate = 10;

    public int AttackBase = 1;
    public int Defence = 0;
    public int Aim = 0;
    public int Dodge = 0;
    public int CritOffset = 0;
    public int AdditionalAttackTimes = 0;

    public int MagicPower = 0;

    public List<string> UsableMagicList = new List<string>();
    public string UsableMagic = "";
    public string Drop = "";

    public void ParseStr()
    {
        if (UsableMagic != "")
        {
            foreach (string attri in UsableMagic.Split(','))
            {
                UsableMagicList.Add(attri);
            }
        }
    }

    // デフォルトコンストラクタ
    public CharacterStatus()
    {
    }

    // コピーコンストラクタ
    public CharacterStatus(CharacterStatus source)
    {
        Name = source.Name;
        ID = source.ID;
        GfxId = source.GfxId;
        food = source.food;
        food_max = source.food_max;
        gold = source.gold;
        exp = source.exp;
        jobexp = source.jobexp;
        OriginalActionCommandTime = source.OriginalActionCommandTime;

        level = source.level;
        life = source.life;
        life_max = source.life_max;

        NwAttack = source.NwAttack;
        NwAttackTimes = source.NwAttackTimes;
        NwAttackCriticalRate = source.NwAttackCriticalRate;

        AttackBase = source.AttackBase;
        Defence = source.Defence;
        Dodge = source.Dodge;
        Aim = source.Aim;
        CritOffset = source.CritOffset;
        AdditionalAttackTimes = source.AdditionalAttackTimes;

        MagicPower = source.MagicPower;
        UsableMagicList = source.UsableMagicList;
        Drop = source.Drop;
    }
}

[System.Serializable]
public class CharacterEquip
{
    public ItemNode RightHand = null;
    public ItemNode LeftHand = null;
    public ItemNode Armor = null;
}

[System.Serializable]
public class CharacterBuff
{
    public enum BuffType
    {
        NoEffect, UpAttack, UpDodge, UpCrit
    }

    public string ID = "";
    public int Value = 0;
    public int Turn = 0;
    public BuffType Type = BuffType.NoEffect;

    // デフォルトコンストラクタ
    public CharacterBuff(string ID, int Value, int Turn, BuffType Type)
    {
        this.ID = ID;
        this.Value = Value;
        this.Turn = Turn;
        this.Type = Type;
    }
}

public abstract class MovingObject : MonoBehaviour {

    //------------------------------------------
    //
    // ここから下はデータセーブ対象
    //
    //------------------------------------------
    // Status
    public CharacterStatus Status;

    // Equip
    public CharacterEquip Equip;

    // Buffs
    public List<CharacterBuff> Buffs;

    // Items
    public ItemInventory Inventory;

    // Position
    public Vector2 logicalPos;

    public enum TurnCommand
    {
        Attack, Move, ThrowItem, UseItem, Undef,
        MoveMap,
        PutItem,
        CastMagic
    }

    public enum AttackAnimationType
    {
        Normal, Throw
    }

    public enum EquipDest
    {
        None, RightHand, LeftHand, Armor
    }

    private float moveTime = 0.2f;
    private float inverseMoveTime;

    public LayerMask AttackableLayer;
    public LayerMask MoveBlockingLayer;
    public LayerMask GetItemLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;

    private int ActualActionCommandTime = 60;
    public int remainingActionCommandTime = 0;

    public Vector2 destPos;
    private Vector2 direction;
    private Vector2 attackLine;

    // Animation Wait Flag
    public bool isMoving = false;
    public bool isMovingPre = false;
    public bool isAttackingAnimation = false;
    public bool isDamegeAnimation = false;
    public bool waitDamegeEffect = false;
    public bool waitAttackingProcess = false;

    // Battle Variable
    private TurnCommand Command;
    public AttackAnimationType AnimationType = AttackAnimationType.Normal;

    public ItemNode ThrowItem;
    public Item ThrowItemInstance;
    public ItemNode UseItem;
    public ItemNode PutItem;

    public string UseMagic;


    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();

        moveTime = GameManager.instance.turnDelay;
        inverseMoveTime = 1f / moveTime;

        logicalPos = transform.position;
        GameManager.instance.AddMovingObjectToList(this);
        direction = new Vector2(0, -1);
        Command = TurnCommand.Undef;
        ActualActionCommandTime = Status.OriginalActionCommandTime;

        UpdateStatus();
        InitializeStatus();
    }

    protected virtual void Update()
    {

        if (isDamegeAnimation)
        {
            return;
        }

        if (isAttackingAnimation)
        {
            return;
        }

        //update animationspeed
        if (Player.instance && GameManager.instance)
        {
            float acratio = (float)GetActionCommandTime() / (float)Player.instance.GetActionCommandTime();
            if (acratio < 1f) moveTime = GameManager.instance.turnDelay * acratio;
            else moveTime = GameManager.instance.turnDelay;
        }

        if (logicalPos != (Vector2)transform.position && !GameManager.instance.isProhibitAnimation)
        {
            MoveAnimation();
        }

        // Update Status
        UpdateStatus();
    }

    protected virtual void UpdateStatus()
    {
        VerifyEquip();
    }

    protected virtual void InitializeStatus()
    {
    }

    protected virtual void OnDestroy()
    {
        GameManager.instance.RemoveMovingObjectToList(this);
    }

    public virtual void DestroyWithExp()
    {
        int exp = (int)Mathf.Pow(Status.level, 2) * 10;
        if (Player.instance.gameObject != gameObject) {
            MessageWindow.instance.ConOut(Status.Name + "をやっつけた！ ");
            Player.instance.GetExp(exp);
        }
        if(Status.Drop != "")
        {
            ItemManager.instance.GenerateItemFromNode(new ItemNode(Status.Drop, 1, 1), logicalPos);
        }
        Destroy(this.gameObject);
    }

    public bool CanAction()
    {
        if (GetActionCommandTime() <= remainingActionCommandTime)
        {
            return true;
        }
        return false;
    }

    public TurnCommand GetCommand()
    {
        return Command;
    }
    public void SetCommand(TurnCommand c)
    {
        Command = c;
    }

    public Vector2 GetDirection()
    {
        return direction;
    }

    /// Set and Normarize Direction
    public void SetDirection( Vector2 v)
    {
        direction = v;
        NormalizeDirection();
    }
    private void NormalizeDirection()
    {
        if (direction.x > 0) direction.x = 1;
        if (direction.x < 0) direction.x = -1;
        if (direction.y > 0) direction.y = 1;
        if (direction.y < 0) direction.y = -1;
    }

    public Vector2 GetAttackLine()
    {
        return attackLine;
    }
    public void SetAttackLine(Vector2 v)
    {
        attackLine = v;
    }

    public virtual void EndTurnProcess()
    {

    }

    public int GetDirectionNumber()
    {
        int directionNumber = 2;

        if (direction.x > 0)
        {
            if (direction.y < 0)
            {
                directionNumber = 3;
            }
            else if (direction.y > 0)
            {
                directionNumber = 9;
            }
            else
            {
                directionNumber = 6;
            }
        }
        else if (direction.x < 0)
        {
            if (direction.y < 0)
            {
                directionNumber = 1;
            }
            else if (direction.y > 0)
            {
                directionNumber = 7;
            }
            else
            {
                directionNumber = 4;
            }
        }
        else if (direction.y > 0)
        {
            directionNumber = 8;
        }
        else if (direction.y < 0)
        {
            directionNumber = 2;
        }
        return directionNumber;
    }

    //現在地から目的地(引数end)へ移動するためのメソッド
    public void MoveAnimation()
    {
        StartCoroutine(SmoothMovement(logicalPos));
    }

    //現在地から目的地(引数end)へ移動するためのメソッド
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        isMovingPre = true;
        inverseMoveTime = 1f / moveTime;
        if (RogueGeneric.GetNumFromVector(end - transform.position) % 2 != 0)
            inverseMoveTime *= Mathf.Sqrt(2);
        //現在地から目的地を引き、2点間の距離を求める(Vector3型)
        //sqrMagnitudeはベクトルを2乗したあと2点間の距離に変換する(float型)
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        //2点間の距離が0になった時、ループを抜ける
        //Epsilon : ほとんど0に近い数値を表す
        while (sqrRemainingDistance > float.Epsilon)
        {
            //現在地と移動先の間を1秒間にinverseMoveTime分だけ移動する場合の、
            //1フレーム分の移動距離を算出する
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            //算出した移動距離分、移動する
            rb2D.MovePosition(newPosition);
            //現在地が目的地寄りになった結果、sqrRemainDistanceが小さくなる
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            if (sqrRemainingDistance < 0.01)
            {
                isMovingPre = false;
            }
            //1フレーム待ってから、while文の先頭へ戻る
            yield return null;
        }
        isMoving = false;
    }


    //移動可能かを判断するメソッド　可能な場合はSmoothMovementへ
    protected bool CanMove(int xDir, int yDir)
    {
        //現在地を取得
        Vector2 start = logicalPos;
        //目的地を取得
        Vector2 end = start + new Vector2(xDir, yDir);
        //自身のColliderを無効にし、Linecastで自分自身を判定しないようにする
        boxCollider.enabled = false;
        //現在地と目的地との間にblockingLayerのついたオブジェクトが無いか判定
        RaycastHit2D hit = Physics2D.Linecast(start, end, MoveBlockingLayer);
        //Colliderを有効に戻す
        boxCollider.enabled = true;
        if (hit.transform != null)
        {
            return false;
        }

        //movingObjectとの干渉があれば移動失敗
        if (GameManager.instance.ExitstMovingObject(end))
        {
            return false;
        }
        return true;
    }

    //移動を試みるメソッド
    //virtual : 継承されるメソッドに付ける修飾子
    //<T>：ジェネリック機能　型を決めておかず、後から指定する
    protected virtual bool AttemptMove<T>(int xDir, int yDir)
        //ジェネリック用の型引数をComponent型で限定
        where T : Component
    {
        //Moveメソッド実行 戻り値がtrueなら移動成功、falseなら移動失敗
        bool canMove = CanMove(xDir, yDir);
        if (canMove)
        {
            destPos = logicalPos + new Vector2(xDir, yDir);
        }
        return canMove;
    }

    public void Move()
    {
        logicalPos = destPos;
    }

    //abstract: メソッドの中身はこちらでは書かず、サブクラスにて書く
    //<T>：AttemptMoveと同じくジェネリック機能
    //障害物があり移動ができなかった場合に呼び出される
    protected abstract void OnCantMove<T>(T component) where T : Component;

    public virtual List<T> AttemptAttack<T>()
        where T : Component
    {
        List<T> others = new List<T>();
        T other;
        others.Clear();
        other = CheckHitLine<T>(logicalPos, logicalPos + direction, AttackableLayer, false);
        if (other)
        {
            others.Add(other);
        }
        return others;
    }

    public virtual T CheckHitLine<T>
        (Vector2 From, Vector2 To, LayerMask blockingLayer, bool AllowSlat)
        where T : Component
    {
        RaycastHit2D hit;

        if (!AllowSlat)
        {
            boxCollider.enabled = false;
            hit = Physics2D.Linecast(From, To, blockingLayer);
            boxCollider.enabled = true;
            if (hit.transform)
            {
                T other = hit.transform.GetComponent<T>();
                if (other)
                {
                    return other;
                }
            }
            return null;
        }
        else
        {
            Vector2 Line = (To - From);
            int length = Mathf.Max((int)Mathf.Abs(Line.x), (int)Mathf.Abs(Line.y));
            Vector2 UnitLine = Line / length;
            for (int l = 0; l <= length; l++)
            {
                boxCollider.enabled = false;
                hit = Physics2D.Linecast(From + UnitLine * l, From + UnitLine * l, blockingLayer);
                boxCollider.enabled = true;
                if (hit.transform)
                {
                    T other = hit.transform.GetComponent<T>();
                    if (other)
                    {
                        return other;
                    }
                }
            }
            return null;
        }
    }

    public virtual IEnumerator Attack<T>()
        where T : MovingObject
    {
        waitAttackingProcess = true;

        MovingObject other;
        List<MovingObject> others = new List<MovingObject>();
        List<int> damege = new List<int>();
        List<bool> crit = new List<bool>();
        bool tmp = false;
        List<Vector2> AttackLines = new List<Vector2>();
        bool AllowSlat = false;

        int[] Attack = { Status.NwAttack, 0 };
        int[] AttackTimes = { Status.NwAttackTimes, 0 };
        int[] CriticalRate = { Status.NwAttackCriticalRate, 13 };
        int[] AttackBase = { Status.AttackBase, 0 };
        int[] Aim = { Status.Aim, 0 };

        ItemProfile RightHand = null;
        if (Equip.RightHand != null) RightHand = ItemManager.instance.GetItemProfileFromID(Equip.RightHand.ID);
        ItemProfile LeftHand = null;
        if (Equip.LeftHand != null) LeftHand = ItemManager.instance.GetItemProfileFromID(Equip.LeftHand.ID);

        // 右手の攻撃
        if (RightHand != null)
        {
            Attack[0] = RightHand.Attack;
            AttackTimes[0] = RightHand.AttackTimes;
            CriticalRate[0] = RightHand.AttackCriticalRate;
            AttackBase[0] = Status.AttackBase + RightHand.AttackBase;
            Aim[0] = Status.Aim + RightHand.Aim;
        }
        // 左手の攻撃
        if (LeftHand != null)
        {
            Attack[1] = LeftHand.Attack;
            AttackTimes[1] = LeftHand.AttackTimes;
            CriticalRate[1] = LeftHand.AttackCriticalRate;
        }

        //近接ステータスの再計算
        AttackTimes[0] += Status.AdditionalAttackTimes;

        if (attackLine == new Vector2(0, 0)) attackLine = direction;
        AttackLines.Add(attackLine);
        if (BattleCanHit3Way())
        {
            attackLine = RogueGeneric.RotateDirection(attackLine, 1);
            AttackLines.Add(attackLine);
            attackLine = RogueGeneric.RotateDirection(attackLine, -2);
            AttackLines.Add(attackLine);
            AllowSlat = true;
        }

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < AttackTimes[j]; i++)
            {
                AttackAnimation();
                while (isAttackingAnimation) yield return null;

                foreach (Vector2 line in AttackLines)
                {
                    other = CheckHitLine<MovingObject>(logicalPos, logicalPos + line, AttackableLayer, AllowSlat);
                    if (other)
                    {
                        others.Add(other);
                        if (BattleHit(Aim[0], other.Status.Dodge))
                        {
                            damege.Add(BattlePhyDamege(Attack[j], AttackBase[j], CriticalRate[j], other.Status.Defence, out tmp));
                            crit.Add(tmp);
                        }
                        else
                        {
                            damege.Add(-1);
                            crit.Add(false);
                        }
                    }
                }
            }
        }

        if (others.Count != 0)
        {
            MessageWindow.instance.ConOut(Status.Name + "の攻撃！\n");
        }

        StartCoroutine(WaitDamegeEffect(others, damege, crit));
        do
        {
            yield return null;
        } while (waitDamegeEffect);
        waitAttackingProcess = false;
    }

    public virtual IEnumerator Throw<T>()
        where T : MovingObject
    {
        waitAttackingProcess = true;

        MovingObject other;

        AttackAnimation();
        ThrowItemInstance = Item.GenerateItemFromNode(ThrowItem, logicalPos);
        Inventory.RemoveItem(ThrowItem, 1);
        yield return null;
        ThrowItemInstance.Throw(attackLine, this);
        do
        {
            yield return null;
        } while (ThrowItemInstance.isThrown);
        if (ThrowItemInstance.other)
        {
            other = ThrowItemInstance.other.GetComponent<T>();
            if (other)
            {
                StartCoroutine(ThrowItemInstance.ThrowEffect(other));
                Destroy(ThrowItemInstance.gameObject);
                do
                {
                    yield return null;
                } while (ThrowItemInstance.isWaitAnimation);
            }
        }
        waitAttackingProcess = false;
    }

    public virtual IEnumerator CastMagic<T>()
        where T : MovingObject
    {
        waitAttackingProcess = true;

        Magic MagicInstance = MagicManager.instance.GenerateMagicFromID(UseMagic, logicalPos);
        yield return null;
        MagicInstance.CastMagic(attackLine, this);
        do
        {
            yield return null;
        } while (MagicInstance.isThrown);
        StartCoroutine(MagicInstance.MagicEffect(this));
        do
        {
            yield return null;
        } while (MagicInstance.isWaitAnimation);
        Destroy(MagicInstance.gameObject);
        waitAttackingProcess = false;
    }

    internal IEnumerator WaitDamegeEffect<T>(List<T> others, List<int> damege, List<bool> crit)
        where T : MovingObject
    {
        waitDamegeEffect = true;
        for (int i = 0; i < others.Count; i++)
        {
            if (others[i])
            {
                if (damege[i] < 0)
                {
                    MessageWindow.instance.ConOut(others[i].Status.Name + "はかわした！\n");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    if (crit[i])
                    {
                        MessageWindow.instance.ConOut("クリティカル！");
                    }
                    others[i].DamegeAnimation();
                    do
                    {
                        yield return null;
                    } while (others[i].isDamegeAnimation);
                    others[i].Damege(damege[i]);
                    yield return null;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        waitDamegeEffect = false;
    }

    public void AttackAnimation(int speedFactor = 1)
    {
        Vector2 offset = direction / 3.0f;
        StartCoroutine(AttackAnimationCoroutine(logicalPos + offset, speedFactor));
    }

    protected IEnumerator AttackAnimationCoroutine(Vector3 end, int speedFactor = 1)
    {
        isAttackingAnimation = true;
        inverseMoveTime = 1f / moveTime * speedFactor;
        if (RogueGeneric.GetNumFromVector(end - transform.position) % 2 != 0)
            inverseMoveTime *= Mathf.Sqrt(2);
        Vector3 startPosition = transform.position;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        end = startPosition;
        sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
        isAttackingAnimation = false;
    }

    public virtual void CheckDeath()
    {

    }

    public virtual void Damege(int damege)
    {
        MessageWindow.instance.ConOut(Status.Name + "に" + damege + "のダメージ！\n", MessageWindow.ConOutType.Add);
        ReduceLife(damege);
    }

    public virtual void ReduceLife(int value)
    {
        Status.life -= value;

        // Check life is 0
        if (Status.life <= 0)
        {
            GetComponent<Renderer>().enabled = false;
            DestroyWithExp();
        }
    }

    public virtual void DamegeAnimation()
    {
        isDamegeAnimation = true;
        StartCoroutine(DamegeAnimationRoutine());
    }

    protected IEnumerator DamegeAnimationRoutine()
    {
        float Duration = Time.time + 0.5f;
        int skipFrame = 0;
        while (Time.time < Duration)
        {
            if (skipFrame != 3)
            {
                skipFrame++;
            }
            else
            {
                skipFrame = 0;
                GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
            }
            yield return null;
        }
        GetComponent<Renderer>().enabled = true;
        isDamegeAnimation = false;
    }

    public virtual void RecoverLife(int recover)
    {
        Status.life += recover;
        if (Status.life > Status.life_max)
        {
            Status.life = Status.life_max;
        }
    }

    //-----------------------------------------------------------------------//
    //
    // ステータス関係
    //
    //-----------------------------------------------------------------------//
    public int GetActionCommandTime()
    {
        return ActualActionCommandTime;
    }

    public bool ConsumeJobExp(int value)
    {
        if (Status.jobexp >= value)
        {
            Status.jobexp -= value;
            return true;
        }
        else return false;
    }

    public void AddBuff(string ID, int Value, int Turn, CharacterBuff.BuffType Type)
    {
        foreach(CharacterBuff b in Buffs)
        {
            if(b.ID == ID)
            {
                b.Turn = Turn;
                b.Value = Value;
                b.Type = Type;
                return;
            }
        }
        Buffs.Add(new CharacterBuff(ID, Value, Turn, Type));
    }

    //-----------------------------------------------------------------------//
    //
    // アイテム関係
    //
    //-----------------------------------------------------------------------//
    public Item GetItemFromTile(List<Item> items)
    {
        foreach(Item c in items)
        {
            if (c.logicalPos == logicalPos)
            {
                return c;
            }
        }
        return null;
    }

    public EquipDest CheckEquipDest(ItemNode Node)
    {
        if (Node == Equip.RightHand) return EquipDest.RightHand;
        if (Node == Equip.LeftHand) return EquipDest.LeftHand;
        if (Node == Equip.Armor) return EquipDest.Armor;
        return EquipDest.None;
    }

    public void ResetEquip()
    {
        Equip.RightHand = null;
        Equip.LeftHand = null;
        Equip.Armor = null;
    }

    public bool CheckEquipAvailable(ItemNode node)
    {
        foreach (string id in node.GetProfile().NGJobIDsList)
        {
            if (id == Player.instance.CurrentJob.ID)
            {
                return false;
            }
        }
        return true;
    }

    public void VerifyEquip()
    {
        if (!Inventory.CheckExistItem(Equip.RightHand)) Equip.RightHand = null;
        if (!Inventory.CheckExistItem(Equip.LeftHand)) Equip.LeftHand = null;
        if (!Inventory.CheckExistItem(Equip.Armor)) Equip.Armor = null;
    }

    //-----------------------------------------------------------------------//
    //
    // 戦闘判定用
    //
    //-----------------------------------------------------------------------//
    private bool BattleHit(int hit, int dodge)
    {
        int dice2d6hit = Random.Range(1, 7) + Random.Range(1, 7);
        int dice2d6dodge = Random.Range(1, 7) + Random.Range(1, 7);

        if (dice2d6hit == 2) return false;
        if (dice2d6dodge == 2) return true;
        dice2d6hit += 6;

        if (dice2d6hit > dice2d6dodge)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int BattlePhyDamege(int atk, int atk_base, int cri, int def, out bool crit)
    {
        int result = atk_base;
        crit = false;
        result += RogueGeneric.CalculateKeyNo(atk, cri, out crit);
        result -= def;
        return result;
    }

    public void GetMagicDamege(int d)
    {
        ReduceLife(d);
    }

    protected virtual bool BattleCanHit3Way()
    {
        return false;
    }
}
