using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MagicProfile
{
    public enum ItemType
    {
        NoEffect, RecoverLife,
        GainFood,
    }
    public enum EquipType
    {
        None, OneHand, TwoHand, Armor, OneHandDual
    }

    public string Name = "";
    public string ID = "";
    public string GfxID = "";
    public ItemType UseEffectTypeValue = ItemType.NoEffect;
    public string UseEffectType = "NoEffect";
    public ItemType ThrowEffectTypeValue = ItemType.NoEffect;
    public string ThrowEffectType = "NoEffect";
    public EquipType EqTypeValue;
    public string EqType = "None";
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
        if (UseEffectType != "")
        {
            UseEffectTypeValue = (ItemType)System.Enum.Parse(typeof(ItemType), UseEffectType);
        }
        if (ThrowEffectType != "")
        {
            ThrowEffectTypeValue = (ItemType)System.Enum.Parse(typeof(ItemType), ThrowEffectType);
        }
        if (EqType != "")
        {
            EqTypeValue = (EquipType)System.Enum.Parse(typeof(EquipType), EqType);
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
        this.UseEffectTypeValue = source.UseEffectTypeValue;
        this.ThrowEffectTypeValue = source.ThrowEffectTypeValue;
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
            || this.UseEffectTypeValue != source.UseEffectTypeValue
            || this.ThrowEffectTypeValue != source.ThrowEffectTypeValue
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

    public LayerMask ThrowBlockingLayer;

    public Vector2 logicalPos;

    public MovingObject other;

    public MagicProfile Profile;

    // Use this for initialization
    void Start () {

        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        Particle = GetComponent<ParticleSystem>();
        logicalPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        StartCoroutine(SmoothMovement(logicalPos));
    }

    public virtual void Throw(Vector2 attackLine, MovingObject owner)
    {
        isThrown = true;
        other = null;
        StartCoroutine(ThrowProcess(attackLine, owner));
    }

    IEnumerator ThrowProcess(Vector2 attackLine, MovingObject owner)
    {
        Vector2 newAttackLine = new Vector2(0, 0);
        Vector2 lastAttackLine = new Vector2(0, 0);

        float LineLength = Mathf.Max(Mathf.Abs(attackLine.x), Mathf.Abs(attackLine.y));
        for (int i = 0; i <= (int)LineLength; i++)
        {
            newAttackLine.x = attackLine.x * System.Math.Abs(i / LineLength);
            newAttackLine.y = attackLine.y * System.Math.Abs(i / LineLength);
            if (CollisionDetect<Wall>(newAttackLine, owner))
            {
                break;
            }
            else if (other = CollisionDetect<MovingObject>(newAttackLine, owner))
            {
                lastAttackLine = newAttackLine;
                break;
            }
            else
            {
                lastAttackLine = newAttackLine;
            }
        }

        logicalPos += lastAttackLine;
        Particle.Play();
        GetComponent<Renderer>().enabled = true;
        StartCoroutine(SmoothMovement(logicalPos));
        while (isMoving)
        {
            yield return null;
        }
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

    public IEnumerator UseEffect(MovingObject owner)
    {
        isWaitAnimation = true;

        MessageWindow.instance.ConOut(owner.Status.Name + "は" + Profile.Name + "を使った！\n");
        yield return new WaitForSeconds(0.5f);

        if (Profile.UseEffectTypeValue == MagicProfile.ItemType.NoEffect)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n");
        }

        if (Profile.UseEffectTypeValue == MagicProfile.ItemType.RecoverLife)
        {
            MessageWindow.instance.ConOut("HPが" + Profile.EffectValue + "回復した！\n");
            owner.RecoverLife(Profile.EffectValue);
        }

        if (Profile.UseEffectTypeValue == MagicProfile.ItemType.GainFood)
        {
            MessageWindow.instance.ConOut("お腹が膨れた！\n");
        }

        if (owner.GetComponent<Player>()) owner.GetComponent<Player>().GainFood(Profile.GainFoodValue);

        yield return new WaitForSeconds(0.5f);
        isWaitAnimation = false;
    }

    public IEnumerator ThrowEffect(MovingObject owner)
    {
        isWaitAnimation = true;

        MessageWindow.instance.ConOut(Profile.Name + "は" + owner.Status.Name + "に当たった！\n");
        yield return new WaitForSeconds(0.5f);

        if (Profile.ThrowEffectTypeValue == MagicProfile.ItemType.NoEffect)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n", MessageWindow.ConOutType.Add);
        }

        if (Profile.ThrowEffectTypeValue == MagicProfile.ItemType.RecoverLife)
        {
            MessageWindow.instance.ConOut("HPが" + Profile.EffectValue + "回復した！\n", MessageWindow.ConOutType.Add);
            owner.RecoverLife(Profile.EffectValue);
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
