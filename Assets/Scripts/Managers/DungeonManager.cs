using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class MapData
{
    public string MapID = "";
    public int level = 1;
    public bool IsAlreadyGenerated = false;
    public int MapXSize = 1;
    public List<int> Walls = new List<int>();
}

[System.Serializable]
class MapEnemyData
{
    public List<CharacterStatus> Enemies = new List<CharacterStatus>();
    public List<Vector2> LogicalPos = new List<Vector2>();
    public List<string> GfxID = new List<string>();
}

[System.Serializable]
class MapItemData
{
    public List<ItemNode> Nodes = new List<ItemNode>();
    public List<Vector2> LogicalPos = new List<Vector2>();
}

public class DungeonManager : MonoBehaviour
{
    // ステージ情報
    private List<Vector3> gridPositions = new List<Vector3>();  //A list of possible locations to place tiles.
    public int columns = 15;                                    //Number of columns in our game board.
    public int rows = 12;                                       //Number of rows in our game board.
    public Transform boardHolder;                              //A variable to store a reference to the transform of our Board object.
    public GameObject[] floorTiles;                             //Array of floor prefabs.
    public GameObject[] outerWallTiles;                         //Array of outer tile prefabs.
    public GameObject[] Stair;
    public GameObject[] ichigoTiles;                            //Array of ichigo tile prefabs.
    public GameObject[] enemyTiles;                             //Array of ichigo tile prefabs.
    public GameObject[] enemyTilesRare;                         //Array of ichigo tile prefabs.

    public GameObject DungeonSetting;

    public bool kakusiflag = false;

    // Playerプレハブ
    public GameObject player;
    public GameObject mainCamera;
    public GameManager gameManager;

    public List<GameObject> Stairs;
    
    public int ichigoCount = 0;

    public string MapID = "";
    public int level = 1;
    public bool IsAlreadyGenerated = false;
    public List<int> Walls = new List<int>();
    public int MapXSize = 20;
    int [,] fieldDataTmp = 
    {   //       0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19
        /* 0 */ {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        /* 1 */ {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1},
        /* 2 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /* 3 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /* 4 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /* 5 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /* 6 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /* 7 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /* 8 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /* 9 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /*10 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /*11 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /*12 */ {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1},
        /*13 */ {1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1},
        /*14 */ {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
        /*15 */ {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    };

    public DungeonProfile Profile;

    private int GetArrayFromList(List<int> list, int x, int y, int size_x)
    {
        int size_y = list.Count / size_x;
        if (list.Count <= (size_y - y - 1) * size_x + x) return -1;
        if (0 > (size_y - y - 1) * size_x + x) return -1;
        return list[(size_y - y - 1) * size_x + x];
    }

    public int GetTileType(int x, int y)
    {
        int size_x = MapXSize;
        int size_y = Walls.Count / size_x;
        if (Walls.Count <= (size_y - y - 1) * size_x + x) return -1;
        if (0 > (size_y - y - 1) * size_x + x) return -1;
        return Walls[(size_y - y - 1) * size_x + x];
    }

    // Update is called once per frame
    public void DungeonTrun()
    {
        if (Enemy.Count < 3 || Enemy.Count < (Ichigo.TotalCount / 5 + 2))
        {
            if (Enemy.Count >= 19)
            {

            }
            else
            {
                InitialiseList();
                GenerateEnemyAtRandom ("Kobold", 1, 1);
            }
        }
    }

    //Clears our list gridPositions and prepares it to generate a new board.
    void InitialiseList()
    {
        //Clear our list gridPositions.
        gridPositions.Clear();

        //Loop through x axis (columns).
        for (int x = 0; x < MapXSize; x++)
        {
            //Within each column, loop through y axis (rows).
            for (int y = 0; y < Walls.Count / MapXSize; y++)
            {
                if (GetArrayFromList(Walls, x, y, MapXSize) == 0)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add(GetCelPos(x, y));
                }
            }
        }
    }

    Vector3 GetCelPos(int x, int y)
    {
        return new Vector3(x, y, 0f);
    }

    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;
        Stairs.Clear();

        for (int x = 0; x < MapXSize; x++)
        {
            for (int y = 0; y < Walls.Count/MapXSize; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                GameObject instance =
                    Instantiate(toInstantiate, GetCelPos(x, y), Quaternion.identity) as GameObject;

                instance.transform.SetParent(boardHolder);

                if (GetArrayFromList(Walls, x, y, MapXSize) == 1)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    instance =
                        Instantiate(toInstantiate, GetCelPos(x, y), Quaternion.identity) as GameObject;

                    instance.transform.SetParent(boardHolder);
                }

                if (GetArrayFromList(Walls, x, y, MapXSize) == 2 && level != Profile.LastLevel)
                {
                    toInstantiate = Stair[0];

                    instance =
                        Instantiate(toInstantiate, GetCelPos(x, y), Quaternion.identity) as GameObject;

                    instance.transform.SetParent(boardHolder);
                    Stairs.Add(instance);
                }
            }
        }
    }

    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    void GenerateEnemyAtRandom(string EnemyID, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = RandomPosition();
            } while (GameManager.instance.ExitstMovingObject(randomPosition));

            string id = Profile.GetRandomEnemyID(level);
            EnemyManager.instance.GenerateEnemyFromID(id, randomPosition);
        }
    }

    void GenerateItemAtRandom(int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = RandomPosition();
            } while (GameManager.instance.ExitstItem(randomPosition));

            string id = Profile.GetRandomItemID(level);
            ItemProfile p = ItemManager.instance.GetItemProfileFromID(id);
            ItemNode node = new ItemNode(id, p.Stack, p.UsableTime);
            ItemManager.instance.GenerateItemFromNode(node, randomPosition);
            //ItemManager.instance.GenerateRandomItem(randomPosition);
        }
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //Choose a random number of objects to instantiate within the minimum and maximum limits
        int objectCount = Random.Range(minimum, maximum + 1);

        //Instantiate objects until the randomly chosen limit objectCount is reached
        for (int i = 0; i < objectCount; i++)
        {
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition;
            do
            {
                randomPosition = RandomPosition();
            } while (GameManager.instance.ExitstMovingObject(randomPosition));

            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    public void SetupScene(string reqMapID, int reqLevel)
    {
        bool GenerateFirstTime = false;
        gameManager = GameObject.FindObjectOfType<GameManager>();

        if (!LoadMapData(reqMapID, reqLevel))
        {
            MapID = reqMapID;
            level = reqLevel;

            Layer2D map = DungeonSetting.GetComponent<DgGenerator>().Generate();

            MapXSize = map.Width;
            Walls = new List<int>();
            Walls.Clear();
            int floorCnt = 0;
            for (int i= 0; i < map.Height; i++)
            {
                for (int j = 0; j < map.Width; j++)
                {
                    if (map.Get(i, j) == 0) floorCnt++;
                    Walls.Add(map.Get(i,j));
                }
            }

            floorCnt = Random.Range(0, floorCnt);
            for (int i = 0; i < Walls.Count; i++)
            {
                if (Walls[i] == 0)
                {
                    if (floorCnt == 0)
                    {
                        Walls[i] = 2;
                        break;
                    }
                    floorCnt--;
                }
            }
            IsAlreadyGenerated = true;
            GenerateFirstTime = true;
        }

        //Creates the outer walls and floor.
        BoardSetup();
        InitialiseList();

        // Load Player Data
        GameObject playerinstance;
        if (Player.instance == null)
        {
            playerinstance = Instantiate(player, player.transform.position, player.transform.rotation);
            playerinstance.GetComponent<Player>().LoadCharacterData();
        }
        else
        {
            playerinstance = Player.instance.gameObject;
        }
        if (playerinstance.GetComponent<Player>().logicalPos == new Vector2())
        {
            playerinstance.GetComponent<Player>().logicalPos = RandomPosition();
            print(playerinstance.GetComponent<Player>().logicalPos);
        }
        playerinstance.transform.position = playerinstance.GetComponent<Player>().logicalPos;


        LoadEnemyData();
        LoadItemData();
        
        string DungeonProfileJson = SaveManager.LoadFixedJson("_Data/Dungeon/" + MapID);
        JsonUtility.FromJsonOverwrite(DungeonProfileJson, Profile);
        //NKTextMan.saveText("/DungeonProfile.json", JsonUtility.ToJson(Profile, true));

        if (GenerateFirstTime)
        {
            InitialiseList();
            GenerateItemAtRandom(3, 7);
            InitialiseList();
            GenerateEnemyAtRandom("", 2, 4);
            InitialiseList();
            if (level == Profile.LastLevel) EnemyManager.instance.GenerateEnemyFromID(Profile.BossID, RandomPosition());
        }

        SystemManager.HoldSystemGameData();
    }

    public bool HoldMapData()
    {
        MapData save = new MapData();
        save.MapID = MapID;
        save.level = level;
        save.IsAlreadyGenerated = IsAlreadyGenerated;
        save.Walls = Walls;
        save.MapXSize = MapXSize;

        SaveManager.HoldJson("/_Save/JsonData/Map/" + MapID + "/" + level, "/Map.json",
            JsonUtility.ToJson(save));
        return true;
    }

    public bool LoadMapData(string reqMapID, int reqLevel)
    {
        MapData save = new MapData();
        string json = SaveManager.LoadJson("/_Save/JsonData/Map/" + reqMapID + "/" + reqLevel, "/Map.json");
        if (json == "") return false;
        save = JsonUtility.FromJson<MapData>(json);
        if (!save.IsAlreadyGenerated) return false;

        MapID = save.MapID;
        level = save.level;
        IsAlreadyGenerated = save.IsAlreadyGenerated;
        Walls = save.Walls;
        MapXSize = save.MapXSize;
        print(json);
        return true;
    }

    public bool HoldEnemyData()
    {
        MapEnemyData save = new MapEnemyData();
        foreach (Enemy e in GameManager.instance.GetEnemyList())
        {
            save.Enemies.Add(e.Status);
            save.LogicalPos.Add(e.logicalPos);
        }

        SaveManager.HoldJson("/_Save/JsonData/Map/" + MapID + "/" + level, "/Enemies.json",
            JsonUtility.ToJson(save, true));
        return true;
    }

    public bool LoadEnemyData()
    {
        if (!EnemyManager.instance) return false;
        MapEnemyData save = new MapEnemyData();
        string json = SaveManager.LoadJson("/_Save/JsonData/Map/" + MapID + "/" + level, "/Enemies.json");
        if (json == "") return false;
        save = JsonUtility.FromJson<MapEnemyData>(json);

        for (int i = 0; i < save.Enemies.Count; i++)
        {
            EnemyManager.instance.GenerateEnemyFromStatus(save.Enemies[i], save.LogicalPos[i]);
        }

        return true;
    }

    public bool HoldItemData()
    {
        MapItemData save = new MapItemData();
        foreach (Item i in GameManager.instance.GetItemList())
        {
            save.Nodes.Add(i.Node);
            save.LogicalPos.Add(i.logicalPos);
        }

        SaveManager.HoldJson("/_Save/JsonData/Map/" + MapID + "/" + level, "/Items.json",
            JsonUtility.ToJson(save, true));
        return true;
    }

    public bool LoadItemData()
    {
        if (!ItemManager.instance) return false;
        MapItemData save = new MapItemData();
        string json = SaveManager.LoadJson("/_Save/JsonData/Map/" + MapID + "/" + level, "/Items.json");
        if (json == "") return false;
        save = JsonUtility.FromJson<MapItemData>(json);

        for (int i = 0; i < save.Nodes.Count; i++)
        {
            ItemManager.instance.GenerateItemFromNode(save.Nodes[i], save.LogicalPos[i]);
        }

        return true;
    }

    public bool HoldCurrentMapData()
    {
        if (!HoldMapData()) return false;
        if (!HoldEnemyData()) return false;
        if (!HoldItemData()) return false;
        return true;
    }

}

[System.Serializable]
public class DungeonProfile
{
    [System.Serializable]
    public class MonsterTable
    {
        public int StartLevel = 0;
        public int EndLevel = 0;
        public int TotalRate = 0;
        public List<MonsterRate> Rates = new List<MonsterRate>();
        [System.Serializable]
        public class MonsterRate
        {
            public string MonsterID = "";
            public int Rate = 0;
        }

        public int GetTotalRate()
        {
            if (TotalRate != 0) return TotalRate;

            TotalRate = 0;
            foreach (MonsterTable.MonsterRate r in Rates)
            {
                TotalRate += r.Rate;
            }
            return TotalRate;
        }
    }

    [System.Serializable]
    public class ItemTable
    {
        public int StartLevel = 0;
        public int EndLevel = 0;
        public int TotalRate = 0;
        public List<ItemRate> Rates = new List<ItemRate>();

        [System.Serializable]
        public class ItemRate
        {
            public string ItemID = "";
            public int Rate = 0;
        }

        public int GetTotalRate()
        {
            if (TotalRate != 0) return TotalRate;

            TotalRate = 0;
            foreach (ItemTable.ItemRate r in Rates)
            {
                TotalRate += r.Rate;
            }
            return TotalRate;
        }
    }

    public string BossID = "Mary";
    public int LastLevel = 15;
    public List<MonsterTable> Moster = new List<MonsterTable>();
    public List<ItemTable> Item = new List<ItemTable>();

    public string GetRandomItemID(int level)
    {
        List<ItemTable.ItemRate> Rates = new List<ItemTable.ItemRate>();

        int TotalRate = 0;
        foreach (ItemTable t in Item)
        {
            if (t.StartLevel <= level && t.EndLevel >= level)
            {
                foreach (ItemTable.ItemRate r in t.Rates)
                {
                    Rates.Add(r);
                }
                TotalRate += t.GetTotalRate();
            }
        }

        int random = Random.Range(0, TotalRate);
        foreach (ItemTable.ItemRate r in Rates)
        {
            if (random < r.Rate) return r.ItemID;
            random -= r.Rate;
        }
        return Rates[Rates.Count - 1].ItemID;
    }

    public string GetRandomEnemyID(int level)
    {
        List<MonsterTable.MonsterRate> Rates = new List<MonsterTable.MonsterRate>();

        int TotalRate = 0;
        foreach (MonsterTable t in Moster)
        {
            if (t.StartLevel <= level && t.EndLevel >= level)
            {
                foreach (MonsterTable.MonsterRate r in t.Rates)
                {
                    Rates.Add(r);
                }
                TotalRate += t.GetTotalRate();
            }
        }

        int random = Random.Range(0, TotalRate);
        foreach (MonsterTable.MonsterRate r in Rates)
        {
            if (random < r.Rate) return r.MonsterID;
            random -= r.Rate;
        }
        return "";
    }
}
