using UnityEngine;
using System.Collections;
using System.Collections.Generic; //Listを使う時に宣言
using System;
using UnityEngine.UI; //UI用に宣言
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public DungeonManager boardScript;

    private List<Enemy> enemies; //Enemyクラスの配列
    private List<NPC> NPCs = new List<NPC>(); //Enemyクラスの配列
    private List<MovingObject> movingObjects; //movingObjectsクラスの配列
    public List<Item> items;

    private bool playerAttacking = false; //Enemyのターン中true
    private bool waitAnimation = false; //Enemyのターン中true
    public float turnDelay = .2f; //Enemyの動作時間(0.1秒)
    public float delay100ms = .1f; // delay 100ms

    public MenuWindow MainWindow; 

    public Player player;

    private int enemyAttackCunter = 0;

    private bool playerMoved = false;
    private bool enemyMoved = false;
    private bool enemyAttacked = false;

    public bool isProhibitAnimation = false;

    private bool AllowEnemyTurnDelay = true;

    [HideInInspector] public bool playersTurn = true;

    public enum GamePhase
    {
        StartTurn,
        PlayerAttack,
        PlayerMove,
        PlayerThrowItem,
        PlayerGetItem,
        PlayerUseItem,
        EnemyCommandDecide,
        EnemyMove,
        EnemyAttack,
        CheckEnemyAct,
        DungeonAct,
        WaitPlayerTurn,
        VerifyThrownItemPos,
        EndTurnProcess,
        MoveMap,
        CheckPlayerAct,
        PlayerPutItem,
        PlayerCastMagic,
    };

    public GamePhase phase = GamePhase.PlayerAttack;

    //Awake : Sceneを移動した時即座に実行される
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //Enemyを格納する配列の作成
        enemies = new List<Enemy>();

        //movingObjectsを格納する配列の作成
        movingObjects = new List<MovingObject>();

        //BoardManager取得
        boardScript = GetComponent<DungeonManager>();
        InitGame();
    }

    void InitGame()
    {
        UnityEngine.Random.InitState((int)Time.time);
        //EnemyのList(配列)を初期化
        enemies.Clear();

        SystemManager.LoadSystemGameData();
        string StartMapID = "TestDungeon";
        int StartMapLevel = 1;
        if (SystemManager.GameData.PlayerMapID != "") StartMapID = SystemManager.GameData.PlayerMapID;
        if (SystemManager.GameData.PlayerMapLevel != 1) StartMapLevel = SystemManager.GameData.PlayerMapLevel;
        boardScript.SetupScene(StartMapID, StartMapLevel);

        player = GameObject.FindObjectOfType<Player>();
    }

    void MoveMap()
    {
        isProhibitAnimation = true;
        player.HoldCharacterData();
        boardScript.HoldCurrentMapData();
        player.gameObject.transform.position = new Vector2();
        player.logicalPos = new Vector2();
        foreach (Enemy e in enemies)
        {
            Destroy(e.gameObject);
        }
        foreach (Item i in items)
        {
            Destroy(i.gameObject);
        }
        Destroy(boardScript.boardHolder.gameObject);

        string StartMapID = boardScript.MapID;
        int StartMapLevel = boardScript.level + 1;
        
        boardScript.SetupScene(StartMapID, StartMapLevel);
        isProhibitAnimation = false;
    }

    void Update()
    {

        StartGameManager:

        //Restart
        if (!Player.instance)
        {
            if (!Input.GetKey("r")) return;
            // 現在のシーン番号を取得
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            // 現在のシーンを再読込する
            SceneManager.LoadScene(sceneIndex);
        }

        //プレイヤーのターンかEnemyが動いた後ならUpdateしない
        if (playersTurn || waitAnimation || playerAttacking)
        {
            return;
        }

        if (phase == GamePhase.StartTurn)
        {
            phase = GamePhase.CheckPlayerAct;
            // 単位時間の経過
            player.remainingActionCommandTime += 60;
            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].remainingActionCommandTime += 60;
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].remainingActionCommandTime += 60;
            }
        }

        if (phase == GamePhase.CheckPlayerAct)
        {
            phase = GamePhase.PlayerAttack;
            //プレイヤーのコマンドを待つか
            if (player.CanAction())
            {
                player.remainingActionCommandTime -= player.GetActionCommandTime();
                playersTurn = true;
                AllowEnemyTurnDelay = true;
                return;
            }
        }

        //↓プレイヤーの行動が決まったら以下の処理へ
        if (phase == GamePhase.PlayerAttack)
        {
            phase = GamePhase.MoveMap;
            //プレイヤーの攻撃処理
            if (player.GetCommand() == MovingObject.TurnCommand.Attack)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                AttackMovingObject(player);
                // Enemyが死ぬ可能性がある場合は一旦Returnしてコルーチンの処理を待つ
                return;
            }
        }

        if (phase == GamePhase.MoveMap)
        {
            if(player.GetCommand() == MovingObject.TurnCommand.MoveMap)
            {
                if (player.isMoving)
                {
                    return;
                }
                if (boardScript.Stairs[0].GetComponent<Wall>().data.pos == player.logicalPos)
                {
                    MoveMap();
                }
                player.SetCommand(MovingObject.TurnCommand.Undef);
            }
            phase = GamePhase.PlayerThrowItem;
        }

        if (phase == GamePhase.PlayerThrowItem)
        {
            phase = GamePhase.PlayerPutItem;
            if (player.GetCommand() == MovingObject.TurnCommand.ThrowItem)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                ThrowMovingObject(player);
                // Enemyが死ぬ可能性がある場合は一旦Returnしてコルーチンの処理を待つ
                phase = GamePhase.VerifyThrownItemPos;
                return;
            }
        }

        if (phase == GamePhase.VerifyThrownItemPos)
        {
            phase = GamePhase.PlayerUseItem;
        }

        if (phase == GamePhase.PlayerPutItem)
        {
            phase = GamePhase.PlayerUseItem;
            if (player.GetCommand() == MovingObject.TurnCommand.PutItem)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                Item tmp = Item.GenerateItemFromNode(player.PutItem, player.logicalPos);
                player.Inventory.RemoveItem(player.PutItem, player.PutItem.stack);
                phase = GamePhase.VerifyThrownItemPos;
            }
        }

        if (phase == GamePhase.PlayerUseItem)
        {
            phase = GamePhase.PlayerCastMagic;
            if (player.GetCommand() == MovingObject.TurnCommand.UseItem)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                PlayerUseItem();
                return;
            }
        }

        if (phase == GamePhase.PlayerCastMagic)
        {
            phase = GamePhase.PlayerMove;
            if (player.GetCommand() == MovingObject.TurnCommand.CastMagic)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                CastMagicMovingObject(player);
                // Enemyが死ぬ可能性がある場合は一旦Returnしてコルーチンの処理を待つ
                return;
            }
        }

        if (phase == GamePhase.PlayerMove)
        {
            playerMoved = false;
            phase = GamePhase.EnemyCommandDecide;
            //プレイヤーの移動処理
            if (player.GetCommand() == MovingObject.TurnCommand.Move)
            {
                player.SetCommand(MovingObject.TurnCommand.Undef);
                player.Move();
                playerMoved = true;
                return;
            }
        }

        if (phase == GamePhase.EnemyCommandDecide)
        {

            enemyMoved = false;
            enemyAttacked = false;
            if (CheckCanActionEnemy())
            {
                for (int i = 0; i < NPCs.Count; i++)
                {
                    if (NPCs[i].CanAction())
                    {
                        NPCs[i].remainingActionCommandTime -= NPCs[i].GetActionCommandTime();
                        NPCs[i].CommandNPC();
                        if (NPCs[i].GetCommand() == MovingObject.TurnCommand.Move)
                        {
                            NPCs[i].Move();
                            enemyMoved = true;
                            NPCs[i].SetCommand(MovingObject.TurnCommand.Undef);
                        }
                    }
                }
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].CanAction())
                    {
                        enemies[i].remainingActionCommandTime -= enemies[i].GetActionCommandTime();
                        enemies[i].CommandEnemy();
                        if (enemies[i].GetCommand() == MovingObject.TurnCommand.Move)
                        {
                            enemies[i].Move();
                            enemyMoved = true;
                            enemies[i].SetCommand(MovingObject.TurnCommand.Undef);
                        }
                    }
                }

                //攻撃目標再設定
                for (int i = 0; i < NPCs.Count; i++)
                {
                    if (NPCs[i].GetCommand() != MovingObject.TurnCommand.Undef)
                    {
                        NPCs[i].CommandNPC();
                    }
                    if (NPCs[i].GetCommand() == MovingObject.TurnCommand.Attack
                        || NPCs[i].GetCommand() == MovingObject.TurnCommand.CastMagic)
                    {
                        enemyAttacked = true;
                    }
                }
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].GetCommand() != MovingObject.TurnCommand.Undef)
                    {
                        enemies[i].CommandEnemy();
                    }
                    if (enemies[i].GetCommand() == MovingObject.TurnCommand.Attack
                        || enemies[i].GetCommand() == MovingObject.TurnCommand.CastMagic)
                    {
                        enemyAttacked = true;
                    }
                }

                phase = GamePhase.EnemyMove;
            }
            else
            {
                phase = GamePhase.PlayerGetItem;
            }
        }

        if (phase == GamePhase.EnemyMove)
        {
            enemyAttackCunter = 0;
            phase = GamePhase.EnemyAttack;
            if (enemyMoved)
            {
                TurnEnemies();
            }
            return;
        }

        if (phase == GamePhase.EnemyAttack)
        {
            if (enemyAttacked)
            {
                if (player.isMoving)
                {
                    return;
                }
                enemyAttackCunter = 0;
                for (int i = enemyAttackCunter; i < NPCs.Count; i++)
                {
                    enemyAttackCunter = i;
                    if (NPCs[i].isMoving)
                    {
                        return;
                    }
                    if (NPCs[i].GetCommand() == MovingObject.TurnCommand.Attack)
                    {
                        NPCs[i].SetCommand(MovingObject.TurnCommand.Undef);
                        AttackMovingObject(NPCs[i]);
                        enemyAttackCunter++;
                        return;
                    }
                    if (NPCs[i].GetCommand() == MovingObject.TurnCommand.CastMagic)
                    {
                        NPCs[i].SetCommand(MovingObject.TurnCommand.Undef);
                        CastMagicMovingObject(NPCs[i]);
                        enemyAttackCunter++;
                        return;
                    }
                }
                enemyAttackCunter = 0;
                for (int i = enemyAttackCunter; i < enemies.Count; i++)
                {
                    enemyAttackCunter = i;
                    if (enemies[i].isMoving)
                    {
                        return;
                    }
                    if (enemies[i].GetCommand() == MovingObject.TurnCommand.Attack)
                    {
                        enemies[i].SetCommand(MovingObject.TurnCommand.Undef);
                        AttackMovingObject(enemies[i]);
                        enemyAttackCunter++;
                        return;
                    }
                    if (enemies[i].GetCommand() == MovingObject.TurnCommand.CastMagic)
                    {
                        enemies[i].SetCommand(MovingObject.TurnCommand.Undef);
                        CastMagicMovingObject(enemies[i]);
                        enemyAttackCunter++;
                        return;
                    }
                }
            }
            phase = GamePhase.PlayerGetItem;
        }

        if (phase == GamePhase.PlayerGetItem)
        {
            if (playerMoved)
            {
                if (player.isMovingPre)
                {
                    return;
                }
                Item tmp = player.GetItemFromTile(items);
                if (tmp)
                {
                    if (player.Inventory.AddItem(new ItemNode(tmp.Node)))
                    {
                        MessageWindow.instance.ConOut(tmp.Node.GetDisplayName() + "を拾った。\n");
                        Destroy(tmp.gameObject);
                    }
                    else
                    {
                        MessageWindow.instance.ConOut("持ち物がいっぱいで拾えない。" + tmp.Node.GetDisplayName() + "の上に乗った。\n");
                    }
                }
            }
            phase = GamePhase.CheckEnemyAct;
        }

        if (phase == GamePhase.CheckEnemyAct)
        {
            if (CheckCanActionEnemy() || player.CanAction())
            {
                phase = GamePhase.CheckPlayerAct;
                goto StartGameManager;
            }
            else
            {
                phase = GamePhase.DungeonAct;
            }
        }

        if (phase == GamePhase.DungeonAct)
        {
            boardScript.DungeonTrun();
            phase = GamePhase.EndTurnProcess;
        }

        if (phase == GamePhase.EndTurnProcess)
        {
            player.EndTurnProcess();
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].EndTurnProcess();
            }
            phase = GamePhase.WaitPlayerTurn;
        }

        if (phase == GamePhase.WaitPlayerTurn)
        {
            StartCoroutine(TurnDelay());
        }
    }

    private bool CheckCanActionEnemy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].CanAction())
            {
                return true;
            }
        }
        return false;
    }

    private void TurnEnemies()
    {
        if (AllowEnemyTurnDelay)
        {
            AllowEnemyTurnDelay = false;
            StartCoroutine(MoveEnemies());
        }
    }
    protected IEnumerator MoveEnemies()
    {
        waitAnimation = true;
        float MaxEnemySpeed = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (MaxEnemySpeed < (player.GetActionCommandTime() / enemies[i].GetActionCommandTime()))
            {
                MaxEnemySpeed = player.GetActionCommandTime() / enemies[i].GetActionCommandTime();
            }
        }
        if (MaxEnemySpeed > 1)
        {
            yield return null; //new WaitForSeconds((turnDelay / MaxEnemySpeed)-.2f);
        }
        else
        {
            yield return new WaitForSeconds(turnDelay);
        }
        waitAnimation = false;
    }

    protected IEnumerator TurnDelay()
    {
        if (enemyMoved)
        {
            enemyMoved = false;
            while (player.isMovingPre || player.isAttackingAnimation)
            {
                yield return null;
            }
        }
        else
        {
            while (player.isMovingPre || player.isAttackingAnimation)
            {
                yield return null;
            }
        }
        phase = GamePhase.StartTurn;
    }

    void AttackMovingObject(MovingObject obj)
    {
        StartCoroutine(AttackMovingObjectRoutine(obj));
    }

    void ThrowMovingObject(MovingObject obj)
    {
        StartCoroutine(ThrowMovingObjectRoutine(obj));
    }

    void CastMagicMovingObject(MovingObject obj)
    {
        StartCoroutine(CastMagicMovingObjectRoutine(obj));
    }

    protected IEnumerator AttackMovingObjectRoutine(MovingObject obj)
    {
        playerAttacking = true;

        StartCoroutine(obj.Attack<MovingObject>());
        do
        {
            yield return null;
        } while (obj.waitAttackingProcess);
        playerAttacking = false;
    }

    protected IEnumerator ThrowMovingObjectRoutine(MovingObject obj)
    {
        playerAttacking = true;

        StartCoroutine(obj.Throw<MovingObject>());
        do
        {
            yield return null;
        } while (obj.waitAttackingProcess);

        if (obj.ThrowItemInstance != null)
        {
            Vector2 dest = new Vector2();
            if (CheckItemEmptyCell(obj.ThrowItemInstance, out dest))
            {
                obj.ThrowItemInstance.logicalPos = dest;
            }
            else
            {
                Destroy(obj.ThrowItemInstance.gameObject);
            }
        }
        playerAttacking = false;
    }

    protected IEnumerator CastMagicMovingObjectRoutine(MovingObject obj)
    {
        playerAttacking = true;

        StartCoroutine(obj.CastMagic<MovingObject>());
        do
        {
            yield return null;
        } while (obj.waitAttackingProcess);

        playerAttacking = false;
    }

    bool CheckItemEmptyCell(Item item, out Vector2 dest)
    {
        dest = item.logicalPos;
        int size = 1;
        int cnt = 0;
        int dir = 2;
        for (int i = 0; i < 25; i++)
        {

            RaycastHit2D hit;
            item.GetComponent<BoxCollider2D>().enabled = false;
            hit = Physics2D.Linecast(dest, dest, item.DropBlockingLayer);
            item.GetComponent<BoxCollider2D>().enabled = true;
            if (!hit.transform)
            {
                item.GetComponent<BoxCollider2D>().enabled = false;
                hit = Physics2D.Linecast(dest, dest, item.DropAvailableLayer);
                item.GetComponent<BoxCollider2D>().enabled = true;
                if (hit.transform)
                {
                    item.GetComponent<BoxCollider2D>().enabled = false;
                    hit = Physics2D.Linecast(item.logicalPos, dest, item.MoveBlockingLayer);
                    item.GetComponent<BoxCollider2D>().enabled = true;
                    if (!hit.transform) return true;
                }
            }

            if (dir == 2)
            {
                dest.y--;
                if (cnt % size == 0) dir = 4;
            }
            else if (dir == 4)
            {
                dest.x--;
                if (cnt % size == 0) dir = 8;
            }
            else if (dir == 8)
            {
                dest.y++;
                if (cnt % size == 0) dir = 6;
            }
            else if (dir == 6)
            {
                dest.x++;
                if (cnt % size == 0) dir = 2;
            }
            if (cnt == size * 2)
            {
                cnt = 0;
                size++;
            }
            cnt++;
        }
        return false;
    }

    void PlayerUseItem()
    {
        StartCoroutine(PlayerUseItemCoroutine());
    }

    protected IEnumerator PlayerUseItemCoroutine()
    {
        playerAttacking = true;

        Item tmp = Item.GenerateItemFromNode(player.UseItem, player.logicalPos);
        tmp.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        player.Inventory.RemoveItem(player.UseItem, 1);

        StartCoroutine(tmp.UseEffect(player));
        do
        {
            yield return null;
        } while (tmp.isWaitAnimation);

        if(tmp.Node.UsableTime > 0 || tmp.Node.UsableTime < 0)
        {
            tmp.Node.stack = 1;
            player.Inventory.AddItem(new ItemNode(tmp.Node));
        }
        Destroy(tmp.gameObject);
        playerAttacking = false;
    }

    //--------------------------------------------------------
    //
    // NPC List Function
    //
    //--------------------------------------------------------
    public List<NPC> GetNPCList()
    {
        return NPCs;
    }

    public void AddNPCToList(NPC script)
    {
        NPCs.Add(script);
    }

    public void RemoveNPCToList(NPC script)
    {
        NPCs.Remove(script);
    }

    //--------------------------------------------------------
    //
    // Enemy List Function
    //
    //--------------------------------------------------------
    public List<Enemy> GetEnemyList()
    {
        return enemies;
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    public void RemoveEnemyToList(Enemy script)
    {
        enemies.Remove(script);
    }

    //--------------------------------------------------------
    //
    // Item List Function
    //
    //--------------------------------------------------------
    public List<Item> GetItemList()
    {
        return items;
    }

    public void AddItemToList(Item script)
    {
        items.Add(script);
    }

    public void RemoveItemToList(Item script)
    {
        items.Remove(script);
    }

    public Item ExitstItem(Vector2 dest)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].logicalPos == dest)
            {
                return items[i];
            }
        }
        return null;
    }

    public bool CanPutItem(MovingObject p)
    {
        foreach (Item i in items)
        {
            if (i.logicalPos == p.logicalPos)
            {
                return false;
            }
        }
        return true;
    }

    //--------------------------------------------------------
    //
    // MovingObject List Function
    //
    //--------------------------------------------------------
    public void AddMovingObjectToList(MovingObject script)
    {
        movingObjects.Add(script);
    }

    public void RemoveMovingObjectToList(MovingObject script)
    {
        movingObjects.Remove(script);
    }

    public MovingObject ExitstMovingObject(Vector2 dest)
    {
        for (int i = 0; i < movingObjects.Count; i++)
        {
            if (movingObjects[i].logicalPos == dest)
            {
                return movingObjects[i];
            }
        }
        return null;
    }

    public bool IsOnStair(MovingObject p)
    {
        if (boardScript.Stairs[0].GetComponent<Wall>().data.pos == p.logicalPos)
        {
            return true;
        }
        return false;
    }

}