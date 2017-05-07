using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI; //UI用に宣言
using System.Collections.Generic;
using System.Linq;

public class Player : Character
{
    public static Player instance = null;

    public Camera mainCamera;
    private int cameraYOffset = -1;
    private Animator playerAnimator;
    public bool isKeyRepeat = false;
    private bool isKeyRepeatReserve = false;

    protected override void Start()
    {
        base.Start();

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

        playerAnimator.SetInteger("Direction", GetDirectionNumber());
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
                if (Input.GetKey("c"))
                {
                    //振り向きのみ
                }
                else if (AttemptMove<Wall>(x, y))
                {
                    SetCommand(TurnCommand.Move);
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    MovingObject obj = GameManager.instance.ExitstMovingObject(logicalPos + GetDirection());
                    if (obj != null)
                    {
                        NPC npc = obj.gameObject.GetComponent<NPC>();
                        if (npc != null)
                        {
                            destPos += GetDirection();
                            npc.SwapDestPos = logicalPos;
                            SetCommand(TurnCommand.Move);
                            npc.swapFlag = true;
                        }
                    }
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

        /*
        if (GetCommand() == TurnCommand.Undef)
        {
            isAttack = Input.GetKey("f");
            if (isAttack)
            {

                SetAttackLine(GetDirection() * 5);
                SetCommand(TurnCommand.CastMagic);
            }
        }*/

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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override bool HoldCharacterData()
    {
        JsonPath = "/_Save/JsonData";
        JsonFile = "Player.json";
        return base.HoldCharacterData();
    }

    public override bool LoadCharacterData()
    {
        JsonPath = "/_Save/JsonData";
        JsonFile = "Player.json";
        return base.LoadCharacterData();
    }

    public IEnumerator KeyRepert(float time)
    {
        isKeyRepeat = true;
        yield return new WaitForSeconds(time);
        isKeyRepeat = false;
    }

    protected override void OnCantMove<T>(T component)
    {
    }
}