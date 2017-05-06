using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI; //UI用に宣言
using System.Collections.Generic;
using System.Linq;

public class NPC : Character {


    int TurnCount = 0;

    protected override void Start()
    {
        GameManager.instance.AddNPCToList(this);
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.instance.RemoveNPCToList(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    //敵キャラ移動用メソッド　GameManagerから呼ばれる
    public TurnCommand CommandNPC()
    {
        SetCommand(TurnCommand.Undef);
        int xDir = 0;
        int yDir = 0;

        if (Status.UsableMagicList.Count > 0) SetCommand(SpecialCommand());

        if (GetCommand() == TurnCommand.Undef)
        {
            Vector2 distance;
            List<Enemy> enemies = GameManager.instance.GetEnemyList();

            foreach (Enemy e in enemies)
            {
                distance = e.logicalPos - logicalPos;
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
        }

        if (GetCommand() == TurnCommand.Undef)
        {
            Vector2 tmp;
            tmp = ApproachWalk(Player.instance.logicalPos);
            List<Enemy> enemies = GameManager.instance.GetEnemyList();
            foreach (Enemy e in enemies)
            {
                if (RogueGeneric.Distance(e.logicalPos, logicalPos) < 5)
                {
                    tmp = ApproachWalk(e.logicalPos);
                }
            }
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
        Vector2 TargetPos;

        List<Enemy> enemies = GameManager.instance.GetEnemyList();
        foreach (Enemy e in enemies)
        {
            TargetPos = e.logicalPos;

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
                            if (RogueGeneric.Distance(TargetPos, logicalPos) > tmp.Range) continue;
                            if (CheckHitLine<Enemy>(logicalPos, TargetPos, AttackableLayer, true))
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
        }

        return TurnCommand.Undef;
    }

    public override bool HoldCharacterData()
    {
        JsonFile = Status.ID + ".json";
        return base.HoldCharacterData();
    }

    public override bool LoadCharacterData()
    {
        JsonFile = Status.ID + ".json";
        return base.LoadCharacterData();
    }
}
