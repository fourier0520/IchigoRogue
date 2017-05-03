using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour {

    public float moveTime = 0.05f;
    public float inverseMoveTime;

    // 一時パラメータ
    public bool isThrown = false;
    public bool isMoving = false;
    public bool isWaitAnimation = false;


    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    public LayerMask ThrowBlockingLayer;
    public LayerMask MoveBlockingLayer;
    public LayerMask DropBlockingLayer;
    public LayerMask DropAvailableLayer;

    public Vector2 logicalPos;

    public MovingObject other;

    public ItemProfile Profile;
    public ItemNode Node = null;

    public int amount = 1;

    // Use this for initialization
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        logicalPos = transform.position;

        GameManager.instance.AddItemToList(this);

        if (Node == null) Node = new ItemNode(Profile.ID, 1, Profile.UsableTime);
    }

    protected virtual void OnDestroy()
    {
        GameManager.instance.RemoveItemToList(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        StartCoroutine(SmoothMovement(logicalPos));
    }

    public virtual bool AvailableThrow()
    {
        return true;
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
        if (attackLine.x != 0)
        {
            for (int i = 0; i <= (int)System.Math.Abs(attackLine.x); i++)
            {
                newAttackLine.x = attackLine.x * System.Math.Abs(i / attackLine.x);
                newAttackLine.y = attackLine.y * System.Math.Abs(i / attackLine.x);
                if (CollisionDetect<Wall>(newAttackLine, owner))
                {
                    break;
                }
                else if (other = CollisionDetect<MovingObject>(newAttackLine, owner))
                {
                    break;
                }
                else
                {
                    lastAttackLine = newAttackLine;
                }
            }
        }
        if (attackLine.y != 0)
        {
            for (int i = 0; i <= (int)System.Math.Abs(attackLine.y); i++)
            {
                newAttackLine.y = attackLine.y * System.Math.Abs(i / attackLine.y);
                newAttackLine.x = attackLine.x * System.Math.Abs(i / attackLine.y);
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
        }
        logicalPos += lastAttackLine;
        GetComponent<Renderer>().enabled = true;
        StartCoroutine(SmoothMovement(logicalPos));
        while (isMoving)
        {
            yield return null;
        }

        //GetComponent<Renderer>().enabled = false;
        isThrown = false;
    }

    IEnumerator ThrowAnimation(Vector2 end)
    {
        isMoving = true;
        yield return null;
        isMoving = false;
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

    public IEnumerator UseEffect(MovingObject owner)
    {
        isWaitAnimation = true;

        MessageWindow.instance.ConOut(owner.Status.Name + "は" + Profile.Name + "を使った！\n");
        yield return new WaitForSeconds(0.5f);

        if (Profile.UseEffectTypeValue == ItemProfile.ItemType.NoEffect)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n");
            Node.UsableTime--;
        }

        if (Profile.UseEffectTypeValue == ItemProfile.ItemType.RecoverLife)
        {
            MessageWindow.instance.ConOut("HPが" + Profile.EffectValue + "回復した！\n");
            owner.RecoverLife(Profile.EffectValue);
            Node.UsableTime--;
        }

        if (Profile.UseEffectTypeValue == ItemProfile.ItemType.GainFood)
        {
            MessageWindow.instance.ConOut("お腹が膨れた！\n");
            Node.UsableTime--;
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

        if (Profile.ThrowEffectTypeValue == ItemProfile.ItemType.NoEffect)
        {
            MessageWindow.instance.ConOut("しかし何も起きなかった…\n", MessageWindow.ConOutType.Add);
            Node.UsableTime--;
        }

        if (Profile.ThrowEffectTypeValue == ItemProfile.ItemType.RecoverLife)
        {
            MessageWindow.instance.ConOut("HPが" + Profile.EffectValue + "回復した！\n", MessageWindow.ConOutType.Add);
            owner.RecoverLife(Profile.EffectValue);
            Node.UsableTime--;
        }
        yield return new WaitForSeconds(0.5f);
        isWaitAnimation = false;
    }

    static public Item GenerateItemFromNode(ItemNode node, Vector2 pos)
    {
        return ItemManager.instance.GenerateItemFromNode(node, pos);
    }
}
