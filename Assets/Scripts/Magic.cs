using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MagicProfile
{
    public enum MagicEffectType
    {
        NoEffect, RecoverLife,
        GainFood,
        SummonMonster,
        Damege
    }
    public enum MagicFigureType
    {
        Self, Shot, RandomSpace
    }

    public string Name = "";
    public string ID = "";
    public string GfxID = "";
    public MagicEffectType EffectTypeValue = MagicEffectType.NoEffect;
    public string EffectType = "NoEffect";
    public MagicFigureType FigureTypeValue;
    public string FigureType = "Self";
    public int EffectValue = 1;
    public string Attribute = "";
    public string MonsterID = "";

    // 武器性能
    public int Attack = 0;
    public int AttackCriticalRate = 13;
    public int AttackTimes = 1;
    public int AttackBase = 0;
    public int Aim = 0;
    public int Range = 5;

    //防具性能
    public int Defence = 0;
    public int Dodge = 0;

    public void ParseStr()
    {
        if (EffectType != "")
        {
            EffectTypeValue = (MagicEffectType)System.Enum.Parse(typeof(MagicEffectType), EffectType);
        }
        if (FigureType != "")
        {
            FigureTypeValue = (MagicFigureType)System.Enum.Parse(typeof(MagicFigureType), FigureType);
        }

        foreach (string attri in Attribute.Split(','))
        {
            switch (attri)
            {
                default:
                    break;
            }
        }
    }

    // デフォルトコンストラクタ
    public MagicProfile()
    {
    }

    // コピーコンストラクタ
    public MagicProfile(MagicProfile source)
    {
        this.Name = source.Name;
        this.ID = source.ID;
        this.EffectTypeValue = source.EffectTypeValue;
        this.EffectValue = source.EffectValue;

        // 武器性能
        this.Attack = source.Attack;
        this.AttackCriticalRate = source.AttackCriticalRate;
        this.AttackTimes = source.AttackTimes;
    }

    public bool Compare(MagicProfile source)
    {
        if (source == null) return false;

        if (
               this.ID != source.ID
            || this.EffectTypeValue != source.EffectTypeValue
            || this.EffectValue != source.EffectValue
            || this.Attack != source.Attack
            || this.AttackCriticalRate != source.AttackCriticalRate
            || this.AttackTimes != source.AttackTimes
            )
        {
            return false;
        }

        return true;
    }
}

public class Magic : MonoBehaviour {

    public MagicProfile Profile;

    public float moveTime = 0.05f;
    public float inverseMoveTime;

    // 一時パラメータ
    public bool isThrown = false;
    public bool isMoving = false;
    public bool isWaitAnimation = false;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private ParticleSystem Particle;
    private SpriteRenderer Renderer;

    public LayerMask ThrowBlockingLayer;

    public Vector2 logicalPos;

    public bool MagicIsSuccess = true;

    public List<GameObject> others;


    // Use this for initialization
    void Start () {

        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        Particle = GetComponent<ParticleSystem>();
        Renderer = GetComponent<SpriteRenderer>();
        logicalPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        StartCoroutine(SmoothMovement(logicalPos));
    }

    public virtual void CastMagic(Vector2 Direction, MovingObject owner)
    {
        Vector2 attackLine = RogueGeneric.GetUnitVector(Direction) * Profile.Range;
        others = new List<GameObject>();
        MessageWindow.instance.ConOut(owner.Status.Name + "は" + Profile.Name + "を唱えた！\n");
        if (Profile.FigureTypeValue == MagicProfile.MagicFigureType.Shot)
        {
            StartCoroutine(ShotProcess(attackLine, owner));
        }
        if (Profile.FigureTypeValue == MagicProfile.MagicFigureType.Self)
        {
            Renderer.enabled = false;
            Particle.Play();
            others.Add(owner.gameObject);
        }
        if (Profile.FigureTypeValue == MagicProfile.MagicFigureType.RandomSpace)
        {
            Renderer.enabled = false;
            Vector2 RandomDest;
            if (GetRandomSpace(2, out RandomDest))
            {
                transform.position = RandomDest;
                logicalPos = RandomDest;
                Particle.Play();
            }
            else
            {
                MagicIsSuccess = false;
            }
        }
    }

    private bool GetRandomSpace(int Range, out Vector2 dest)
    {
        int sqrt = (Range * 2 + 1);
        List<int> num = new List<int>();
        for (int i = 0; i < sqrt * sqrt; i++)
        {
            num.Add(i);
        }

        RaycastHit2D hit;
        dest = new Vector2((int)(logicalPos.x - sqrt / 2), (int)(logicalPos.y - sqrt / 2));
        while (num.Count > 0)
        {
            dest = new Vector2((int)(logicalPos.x - sqrt/2), (int)(logicalPos.y - sqrt / 2));
            int random = (int)Random.Range(0, num.Count);
            dest.x += num[random] % sqrt;
            dest.y += num[random] / sqrt;
            boxCollider.enabled = false;
            hit = Physics2D.Linecast(dest, dest, ThrowBlockingLayer);
            boxCollider.enabled = true;
            if (!hit.transform) return true;
            num.RemoveAt(random);
        }
        return false;
    }

    IEnumerator ShotProcess(Vector2 attackLine, MovingObject owner)
    {
        Vector2 newAttackLine = new Vector2(0, 0);
        Vector2 lastAttackLine = new Vector2(0, 0);

        isThrown = true;
        float LineLength = Mathf.Max(Mathf.Abs(attackLine.x), Mathf.Abs(attackLine.y));
        for (int i = 0; i <= (int)LineLength; i++)
        {
            newAttackLine.x = attackLine.x * System.Math.Abs(i / LineLength);
            newAttackLine.y = attackLine.y * System.Math.Abs(i / LineLength);
            if (CollisionDetect<Wall>(newAttackLine, owner))
            {
                break;
            }
            else if (CollisionDetect<MovingObject>(newAttackLine, owner))
            {
                others.Add(CollisionDetect<MovingObject>(newAttackLine, owner).gameObject);
                lastAttackLine = newAttackLine;
                break;
            }
            else
            {
                lastAttackLine = newAttackLine;
            }
        }

        logicalPos += lastAttackLine;
        Renderer.enabled = true;
        StartCoroutine(SmoothMovement(logicalPos));
        while (isMoving)
        {
            yield return null;
        }
        Renderer.enabled = false;
        Particle.Play();
        isThrown = false;
    }

    public virtual T CollisionDetect<T>(Vector2 end, MovingObject owner)
        where T : Component
    {
        T other;
        RaycastHit2D hit;
        boxCollider.enabled = false;
        if (owner.GetComponent<BoxCollider2D>())
        {
            owner.GetComponent<BoxCollider2D>().enabled = false;
        }
        hit = Physics2D.Linecast(logicalPos + end, logicalPos + end, ThrowBlockingLayer);
        boxCollider.enabled = true;
        if (owner.GetComponent<BoxCollider2D>())
        {
            owner.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (hit.transform)
        {
            other = hit.transform.GetComponent<T>();
            if (other)
            {
                return other;
            }
        }
        return null;
    }

    public IEnumerator MagicEffect(MovingObject owner)
    {
        isWaitAnimation = true;

        yield return new WaitForSeconds(0.5f);

        if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.NoEffect
            || !MagicIsSuccess)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n", MessageWindow.ConOutType.Add);
        }

        if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.SummonMonster)
        {
            Enemy e = EnemyManager.instance.GenerateEnemyFromID(Profile.MonsterID, logicalPos);
            MessageWindow.instance.ConOut(e.Status.Name + "が召喚された！\n");
        }

        foreach (GameObject other in others)
        {
            MovingObject m;
            if (m = other.GetComponent<MovingObject>())
            {
                if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.RecoverLife)
                {
                    MessageWindow.instance.ConOut(m.Status.Name + "のHPが" + Profile.EffectValue + "回復した！\n", MessageWindow.ConOutType.Add);
                    m.RecoverLife(Profile.EffectValue);
                }

                if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.Damege)
                {
                    bool crit = false;
                    int Damege = CalculateMagicDamege(Profile.Attack, owner.Status.MagicPower, Profile.AttackCriticalRate, out crit);
                    if (crit) MessageWindow.instance.ConOut("クリティカル！");
                    MessageWindow.instance.ConOut(m.Status.Name + "に" + Damege + "のダメージ！\n", MessageWindow.ConOutType.Add);
                    m.GetMagicDamege(Damege);
                }
            }
        }

        while (Particle.isPlaying)
        {
            yield return null;
        }
        isWaitAnimation = false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        inverseMoveTime = 1f / moveTime;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
        isMoving = false;
    }

    private int CalculateMagicDamege(int atk, int atk_base, int cri, out bool crit)
    {
        int result = atk_base;
        crit = false;
        result += RogueGeneric.CalculateKeyNo(atk, cri, out crit);
        return result;
    }

}
