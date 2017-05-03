using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour {

    public static MessageWindow instance = null;

    public enum ConOutType
    {
        Set, Clear, Add
    }
    ConOutType ConOutTypeValue;

    List<string> MessageStack = new List<string>();

    int time = 0;
    int PreviousMessages = 0;
    int MaxMessages = 10;

    public Text MessageText; //レベルテキスト

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
    }

    void Start () {
        ClearWindow();
        MessageStack.Clear();
    }

    void Update () {
        if (time > 0)
        {
            time--;
        }
        if (time == 0)
        {
            ClearWindow();
        }
        
        if (MessageStack.Count > MaxMessages
            || MessageStack.Count != PreviousMessages)
        {
            MessageText.text = "";
            for (int i = 0; i < MessageStack.Count; i++)
            {
                MessageText.text += MessageStack[i];
            }
            if (MessageStack.Count > MaxMessages)
            {
                MessageStack.RemoveAt(0);
            }
            PreviousMessages = MessageStack.Count;
            DrawWindow();

            time = 60 * 3;
            return;
        }
    }

    public void ConOut(string Text, ConOutType Type = ConOutType.Set, float StartDelay = 0f)
    {
        MessageStack.Add(Text);
    }

    IEnumerator ConOutDelay(float StartDelay)
    {
        yield return new WaitForSeconds(StartDelay);
    }

    void ClearWindow()
    {
        gameObject.GetComponent<Image>().enabled = false;
        MessageText.enabled = false;
    }

    void DrawWindow()
    {
        gameObject.GetComponent<Image>().enabled = true;
        MessageText.enabled = true;
    }

}
