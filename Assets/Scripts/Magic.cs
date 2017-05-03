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
    }
    public enum MagicFigureType
    {
        Self, Shot, 
    }

    public string Name = "";
    public string ID = "";
    public string GfxID = "";
    public MagicEffectType EffectTypeValue = MagicEffectType.NoEffect;
    public string EffectType = "NoEffect";
    public MagicFigureType FigureTypeValue;
    public string FigureType = "Self";
    public int UsableTime = 1;
    public int Stack = 1;
    public int EffectValue = 1;
    public int GainFoodValue = 500;
    public string Attribute = "";
    public bool stackable = false;

    public List<string> NGJobIDsList = new List<string>();
    public string NGJobIDs = "";

    // 武器性能
    public int Attack = 0;
    public int AttackCriticalRate = 13;
    public int AttackTimes = 1;
    public int AttackBase = 0;
    public int Aim = 0;

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

        foreach (string id in NGJobIDs.Split(',')) NGJobIDsList.Add(id);

        foreach (string attri in Attribute.Split(','))
        {
            switch (attri)
            {
                case "Stackable":
                    stackable = true;
                    break;
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
        this.UsableTime = source.UsableTime;
        this.Stack = source.Stack;
        this.EffectValue = source.EffectValue;
        this.GainFoodValue = source.GainFoodValue;
        this.stackable = source.stackable;

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
            || this.UsableTime != source.UsableTime
            || this.EffectValue != source.EffectValue
            || this.GainFoodValue != source.GainFoodValue
            || this.stackable != source.stackable

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

    public List<GameObject> others;

    public MagicProfile Profile;

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

    public virtual void CastMagic(Vector2 attackLine, MovingObject owner)
    {
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

        if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.NoEffect)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n", MessageWindow.ConOutType.Add);
        }

        foreach (GameObject other in others){
            MovingObject m;
            if (m  = other.GetComponent<MovingObject>())
            {
                if (Profile.EffectTypeValue == MagicProfile.MagicEffectType.RecoverLife)
                {
                    MessageWindow.instance.ConOut(m.Status.Name + "のHPが" + Profile.EffectValue + "回復した！\n", MessageWindow.ConOutType.Add);
                    owner.RecoverLife(Profile.EffectValue);
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

}
