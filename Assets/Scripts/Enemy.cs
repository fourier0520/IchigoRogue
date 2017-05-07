using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//MovingObjectを継承
public class Enemy : MovingObject
{
    static public bool allowEnemyMoveAnimation = true;
    public static int Count = 0;

    int TurnCount = 0;

    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);
        Count++;

        base.Start();
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

    protected override void OnCantMove<T>(T component)
    {

    }
}
