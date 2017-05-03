using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//MovingObjectを継承
public class Enemy : MovingObject
{
    static public bool allowEnemyMoveAnimation = true;
    public static int Count = 0;

    int TurnCount = 0;

    //MovingObjectのStartメソッドを継承
    protected override void Start()
    {
        //GameManagerスクリプトのEnemyの配列に格納
        GameManager.instance.AddEnemyToList(this);

        Count++;

        //MovingObjectのStartメソッド呼び出し
        base.Start();
    }

    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        return base.AttemptMove<T>(xDir, yDir);
    }

    protected override void Update()
    {
        if (!allowEnemyMoveAnimation)
        {
            return;
        }
        base.Update();
    }

    //敵キャラ移動用メソッド　GameManagerから呼ばれる
    public TurnCommand CommandEnemy()
    {
        SetCommand(TurnCommand.Undef);
        int xDir = 0;
        int yDir = 0;


        if (Status.UsableMagicList.Count > 0) SetCommand(SpecialCommand());

        if (GetCommand() != TurnCommand.Undef)
            print(UseMagic);

        Vector2 distance = GameManager.instance.player.logicalPos - logicalPos;
        if (GetCommand() == TurnCommand.Undef)
        {
            if (distance.x <= 1.0f && distance.x >= -1.0f
                && distance.y <= 1.0f && distance.y >= -1.0f)
            {
                SetDirection(distance);
                SetAttackLine(GetDirection());
                if (AttemptAttack<Wall>().Count == 0)
                {
                    SetCommand(TurnCommand.Attack);
                }
            }
        }

        if (GetCommand() == TurnCommand.Undef)
        {
            Vector2 tmp;
            if (RogueGeneric.Distance(Player.instance.logicalPos, logicalPos) > 5) tmp = RandomWalk();
            else tmp = ApproachWalk(Player.instance.logicalPos);
            xDir = (int)tmp.x;
            yDir = (int)tmp.y;
            if (AttemptMove<Player>(xDir, yDir))
            {
                SetCommand(TurnCommand.Move);
            }
        }

        TurnCount++;
        return GetCommand();
    }

    private TurnCommand SpecialCommand()
    {
        Vector2 TargetPos = GameManager.instance.player.logicalPos;

        if (Status.UsableMagicList.Count != 0)
        {
            foreach (string m in Status.UsableMagicList)
            {
                MagicProfile tmp = MagicManager.instance.GetMagicProfileFromID(m);

                if (tmp.EffectTypeValue == MagicProfile.MagicEffectType.SummonMonster)
                {
                    if (TurnCount > 10)
                    {
                        UseMagic = m;
                        TurnCount -= 10;
                        return TurnCommand.CastMagic;
                    }
                }

                if (tmp.FigureTypeValue == MagicProfile.MagicFigureType.Shot
                    && tmp.EffectTypeValue == MagicProfile.MagicEffectType.Damege)
                {
                    if ((TargetPos - logicalPos).x == (TargetPos - logicalPos).y
                        || (TargetPos - logicalPos).x == 0
                        || (TargetPos - logicalPos).y == 0)
                    {
                        if (CheckHitLine<Player>(logicalPos, TargetPos, AttackableLayer, true))
                        {
                            SetDirection(TargetPos - logicalPos);
                            SetAttackLine(GetDirection());
                            UseMagic = m;
                            return TurnCommand.CastMagic;
                        }
                    }
                }
            }
        }

        return TurnCommand.Undef;
    }

    protected override void OnDestroy()
    {
        Count--;
        base.OnDestroy();
        GameManager.instance.RemoveEnemyToList(this);
    }

    public override void DestroyWithExp()
    {
        base.DestroyWithExp();
    }

    private Vector2 RandomWalk()
    {
        Vector2 Dir = new Vector2();
        if (Random.Range(-1000, 1000) > 0) //x walk
        {
            if (Random.Range(-1000, 1000) > 0) Dir.x = 1;
            else Dir.x = -1;
        }
        else
        {
            if (Random.Range(-1000, 1000) > 0) Dir.y = 1;
            else Dir.y = -1;
        }
        return Dir;
    }

    private Vector2 ApproachWalk(Vector2 dest)
    {
        Vector2 Dir = new Vector2();
        int min = RogueGeneric.Distance(logicalPos, dest);
        for (int i = 1; i <= 9; i++)
        {
            Vector2 tmp = RogueGeneric.GetVectorFromNum(i);
            if (tmp != new Vector2() && AttemptMove<MovingObject>((int)tmp.x, (int)tmp.y))
            {
                int distance = RogueGeneric.Distance(logicalPos + tmp, dest);
                if (distance < min)
                {
                    min = distance;
                    Dir = tmp;
                }
                else if(distance == min)
                {
                    if ((logicalPos + tmp - dest).sqrMagnitude < (logicalPos + Dir - dest).sqrMagnitude)
                    {
                        Dir = tmp;
                    }
                }
            }
        }

        return Dir;
    }

    protected override void OnCantMove<T>(T component)
    {

    }
}
