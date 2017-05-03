using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    // Playerプレハブ
    public GameObject player;
    public GameObject mainCamera;

    // ステージ情報
    private List<Vector3> gridPositions = new List<Vector3>();  //A list of possible locations to place tiles.
    public int columns = 20;                                     //Number of columns in our game board.
    public int rows = 16;                                        //Number of rows in our game board.
    private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
    public GameObject[] floorTiles;                                 //Array of floor prefabs.
    public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
    public GameObject[] ichigoTiles;                                //Array of ichigo tile prefabs.

    public int ichigoCount = 0;

    private int[,] fieldData = new int[,]
    {   //       0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19
        /* 0 */ {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        /* 1 */ {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
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

    // Use this for initialization
    void Start()
    {
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {

        if (Ichigo.Count < 5)
        {
            //Reset our list of gridpositions.
            InitialiseList();

            LayoutObjectAtRandom(ichigoTiles, 1, 1);
        };
    }

    //Clears our list gridPositions and prepares it to generate a new board.
    void InitialiseList()
    {
        //Clear our list gridPositions.
        gridPositions.Clear();

        //Loop through x axis (columns).
        for (int x = 0; x < columns ; x++)
        {
            //Within each column, loop through y axis (rows).
            for (int y = 0; y < rows ; y++)
            {
                if (fieldData[y, x] == 0)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add(GetCelPos(x, y));
                }
            }
        }
    }

    Vector3 GetCelPos(int x, int y)
    {
        // 画面左下のワールド座標をビューポートから取得
        Vector2 posWorldMin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        // 画面右上のワールド座標をビューポートから取得
        Vector2 posWorldMax = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

        float celUnitSizeX = (posWorldMax.x - posWorldMin.x) / ((float)columns);
        float celUnitSizeY = (posWorldMax.y - posWorldMin.y) / ((float)rows);
 
        Vector3 pos = new Vector3(0f, 0f, 0f);
        pos.x = celUnitSizeX * ((float)x) + celUnitSizeX / 2.0f;
        pos.y = celUnitSizeY * ((float)y) + celUnitSizeY / 2.0f;
        return pos;
    }


    //Sets up the outer walls and floor (background) of the game board.
    void BoardSetup()
    {
        //Instantiate Board and set boardHolder to its transform.
        boardHolder = new GameObject("Board").transform;

        //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
        for (int x = 0; x < columns ; x++)
        {
            //Loop along y axis, starting from -1 to place floor or outerwall tiles.
            for (int y = 0; y < rows ; y++)
            {
                //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                GameObject instance =
                    Instantiate(toInstantiate, GetCelPos(x, y), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardHolder);

                //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                if (fieldData[y,x] == 1)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    instance =
                        Instantiate(toInstantiate, GetCelPos(x, y), Quaternion.identity) as GameObject;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }
    }

    //RandomPosition returns a random position from our list gridPositions.
    Vector3 RandomPosition()
    {
        //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
        int randomIndex = Random.Range(0, gridPositions.Count);

        //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
        Vector3 randomPosition = gridPositions[randomIndex];

        //Remove the entry at randomIndex from the list so that it can't be re-used.
        gridPositions.RemoveAt(randomIndex);

        //Return the randomly selected Vector3 position.
        return randomPosition;
    }

    //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //Choose a random number of objects to instantiate within the minimum and maximum limits
        int objectCount = Random.Range(minimum, maximum + 1);

        //Instantiate objects until the randomly chosen limit objectCount is reached
        for (int i = 0; i < objectCount; i++)
        {
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();

            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    void GameStart()
    {
        // ゲームスタート時に、タイトルを非表示にしてプレイヤーを作成する
        Instantiate(player, player.transform.position, player.transform.rotation);

        //Creates the outer walls and floor.
        BoardSetup();
    }
}
