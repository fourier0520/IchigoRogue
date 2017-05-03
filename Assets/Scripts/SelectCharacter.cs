using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectCharacter : MonoBehaviour {

    public GameObject[] Characters;

    public int CurrentCharacter = 0;

    public GameObject Player = null;
    static public SelectCharacter instance = null;

    private bool titleIsActive = true;

    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        Camera.main.transform.position = new Vector3(
                gameObject.transform.position.x,
                gameObject.transform.position.y,
                gameObject.transform.position.z - 10);
    }

    void Start () {

    }

    // Update is called once per frame
    void Update () {
        if (!titleIsActive) return;

        // 右・左
        int x = (int)Input.GetAxisRaw("Horizontal");


        if (x > 0)
        {
            if (CurrentCharacter < Characters.Length - 1)
            {
                CurrentCharacter++;
            }
        }
        if (x < 0)
        {
            if (CurrentCharacter > 0)
            {
                CurrentCharacter--;
            }
        }

        Player = Characters[CurrentCharacter];

        if (Player)
        {
            Player = Characters[CurrentCharacter];
            GetComponent<SpriteRenderer>().sprite = Player.GetComponent<SpriteRenderer>().sprite;
        }

        if (Input.GetKey("z"))
        {
            titleIsActive = false;
            StartCoroutine(Wait1Sec());
        }
    }

    IEnumerator Wait1Sec()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Dungeon");
    }
}
