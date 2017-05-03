using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallData
{
    public Vector2 pos = new Vector2();
}

public class Wall : MonoBehaviour
{

    public WallData data = new WallData();
    public Sprite NormalSprite;
    public Sprite EdgeSprite;

    // Use this for initialization
    void Start () {
        data.pos = transform.position;
        NormalSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        if (!EdgeSprite)
        {
            EdgeSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        }
    }
	
	// Update is called once per frame
	void Update () {
        int DownTile = GameManager.instance.boardScript.GetTileType((int)data.pos.x, (int)data.pos.y - 1);
        if (DownTile != 1)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = EdgeSprite;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = NormalSprite;
        }
    }
}
