using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleDataLoad : MonoBehaviour {

    static public TitleDataLoad instance = null;
    private bool titleIsActive = true;
    private int Select = 0;
    public bool InisialData = false;

    public Image Image1;
    public Image Image2;

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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!titleIsActive) return;

        // 右・左
        int y = (int)Input.GetAxisRaw("Vertical");


        if (y < 0)
        {
            if (Select < 1)
            {
                Select++;
            }
        }
        if (y > 0)
        {
            if (Select > 0)
            {
                Select--;
            }
        }

        GetComponent<Text>().text = "";
        GetComponent<Text>().text += "つづきから\n";
        GetComponent<Text>().text += "はじめから";

        Image1.gameObject.SetActive(false);
        Image2.gameObject.SetActive(false);
        if (Select == 0) Image1.gameObject.SetActive(true);
        if (Select == 1) Image2.gameObject.SetActive(true);
        if (Input.GetKey("z"))
        {
            if (Select == 1) InisialData = true;
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
