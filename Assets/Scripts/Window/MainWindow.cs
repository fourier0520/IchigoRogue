using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainWindow : MenuWindow
{

    public GameObject ItemWindow;
    public GameObject CurrentJobWindow;
    public GameObject JobWindow;
    public GameObject ActiveSkillWindow;
    public Text MainText;

    public int MainWindowIndex = 0;
    public int PreviousWindowIndex = 0;

    // Use this for initialization
    protected override void Start () {
        base.Start();
	}

    public string ItemMenuString = "アイテム";
    public string MoveMenuString = "階段";
    public string CurrentJobMenuString = "メイン技能";
    public string JobLevelMenuString = "技能レベル";
    public string ActiveSkillMenuString = "アクティブスキル";
    public string SaveMenuString = "セーブ";
    public string CloseMenuString = "閉じる";

    // Update is called once per frame
    protected override void MenuSpecificUpdate()
    {
        int key = 0;

        List<string> MainSelectCommands = new List<string>();
        MainSelectCommands.Clear();
        MainSelectCommands.Add(ItemMenuString);
        MainSelectCommands.Add(MoveMenuString);
        MainSelectCommands.Add(CurrentJobMenuString);
        MainSelectCommands.Add(JobLevelMenuString);
        MainSelectCommands.Add(ActiveSkillMenuString);
        MainSelectCommands.Add(SaveMenuString);
        MainSelectCommands.Add(CloseMenuString);

        if (Input.GetAxisRaw("Horizontal") > 0) key = 6;
        else if (Input.GetAxisRaw("Horizontal") < 0) key = 4;
        else if (Input.GetAxisRaw("Vertical") > 0) key = 8;
        else if (Input.GetAxisRaw("Vertical") < 0) key = 2;

        if (Input.GetKeyDown("x"))
        {
            State = WindowState.Inactive;
            this.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKeyDown("z"))
        {
            if (MainSelectCommands[MainWindowIndex] == ItemMenuString)
            {
                ChildrenWindow = ItemWindow;
                ChildrenWindow.SetActive(true);
                ChildrenWindow.GetComponent<MenuWindow>().State = WindowState.Active;
                State = WindowState.WaitChildren;
            }

            if (MainSelectCommands[MainWindowIndex] == MoveMenuString)
            {
                if (GameManager.instance.IsOnStair(Player.instance))
                {
                    MenuWindow.Command = WindowTurnCommand.MoveMap;
                    State = WindowState.Inactive;
                    this.gameObject.SetActive(false);
                }
                else
                {
                    MessageWindow.instance.ConOut("足元に階段がありません。");
                }
            }

            if (MainSelectCommands[MainWindowIndex] == CurrentJobMenuString)
            {
                ChildrenWindow = CurrentJobWindow;
                ChildrenWindow.SetActive(true);
                ChildrenWindow.GetComponent<MenuWindow>().State = WindowState.Active;
                State = WindowState.WaitChildren;
            }

            if (MainSelectCommands[MainWindowIndex] == JobLevelMenuString)
            {
                ChildrenWindow = JobWindow;
                ChildrenWindow.SetActive(true);
                ChildrenWindow.GetComponent<MenuWindow>().State = WindowState.Active;
                State = WindowState.WaitChildren;
            }

            if (MainSelectCommands[MainWindowIndex] == ActiveSkillMenuString)
            {
                ChildrenWindow = ActiveSkillWindow;
                ChildrenWindow.SetActive(true);
                ChildrenWindow.GetComponent<MenuWindow>().State = WindowState.Active;
                State = WindowState.WaitChildren;
            }

            if (MainSelectCommands[MainWindowIndex] == SaveMenuString)
            {
                //セーブ実行前に現在マップのデータをHOLDしておく
                Player.instance.HoldCharacterData();
                GameManager.instance.boardScript.HoldCurrentMapData();
                SystemManager.HoldSystemGameData();
                SaveManager.SaveAllJson();
                MessageWindow.instance.ConOut("セーブしました！");
            }

            if (MainSelectCommands[MainWindowIndex] == CloseMenuString)
            {
                State = WindowState.Inactive;
                this.gameObject.SetActive(false);
            }
            return;
        }

        if (key == 2)
            MainWindowIndex++;

        if (key == 8)
            MainWindowIndex--;

        if (MainWindowIndex > MainSelectCommands.Count - 1)
        {
            MainWindowIndex = MainSelectCommands.Count - 1;
        }

        if (MainWindowIndex < 0)
        {
            MainWindowIndex = 0;
        }

        if (PreviousWindowIndex != MainWindowIndex)
        {
            PreviousWindowIndex = MainWindowIndex;
            StartCoroutine(KeyRepert(0.1f));
        }

        MainText.text = "";
        for (int i = 0; i < MainSelectCommands.Count; i++)
        {
            if (i == MainWindowIndex) MainText.text += SelectionTag;
            MainText.text += MainSelectCommands[i];
            if (i == MainWindowIndex) MainText.text += SelectionTagFooter;
            MainText.text += "\n";
        }

    }
}