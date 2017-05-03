using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobWindow : MenuWindow
{
    public int JobWindowIndex = 0;
    public int PreviousJobWindowIndex = -255;
    public Text JobWindowText;
    public GameObject JobWindowSub;
    public Text JobWindowSubText;

    public int JobWindowSubIndex = 0;

    bool LevelUpDecision = false;

    Player player;

    // Use this for initialization
    protected override void Start()
    {
        player = Player.instance;

        //Jobは増えたり減ったりする可能性あり
        List<JobData> EmptyJobList = JobManager.instance.GetEmptyJobList();
        for(int i = 0;  i <EmptyJobList.Count; i++)
        {
            foreach(JobData j2 in player.JobList)
            {
                if (EmptyJobList[i].ID == j2.ID) EmptyJobList[i] = j2;
            }
        }
        player.JobList = EmptyJobList;
        base.Start();
    }

    // Update is called once per frame
    protected override void MenuSpecificUpdate()
    {
        this.gameObject.SetActive(true);

        player = Player.instance;

        int key = 0;

        if (Input.GetAxisRaw("Horizontal") > 0) key = 6;
        else if (Input.GetAxisRaw("Horizontal") < 0) key = 4;
        else if (Input.GetAxisRaw("Vertical") > 0) key = 8;
        else if (Input.GetAxisRaw("Vertical") < 0) key = 2;

        if (!LevelUpDecision)
        {
            // 通常時
            if (Input.GetKeyDown("x"))
            {
                JobWindowSubText.text = "";
                State = WindowState.Inactive;
                return;
            }

            if (Input.GetKeyDown("z"))
            {
                bool Valid;
                player.JobList[JobWindowIndex].GetNextExp(out Valid);

                if (Valid) LevelUpDecision = true;
                else JobWindowSubText.text = "これ以上は上がりません！";
                return;
            }

            if (key == 2)
            {
                JobWindowIndex++;
            }

            if (key == 8)
            {
                JobWindowIndex--;
            }

            if (JobWindowIndex > player.JobList.Count - 1)
            {
                JobWindowIndex = player.JobList.Count - 1;
            }
            if (JobWindowIndex < 0)
            {
                JobWindowIndex = 0;
            }

            if (PreviousJobWindowIndex != JobWindowIndex)
            {
                PreviousJobWindowIndex = JobWindowIndex;
                StartCoroutine(KeyRepert(0.1f));
            }

        }
        else
        {
            // レベルアップ選択時
            if (Input.GetKeyDown("x"))
            {
                JobWindowSubText.text = "";
                LevelUpDecision = false;
                return;
            }

            if (Input.GetKeyDown("z"))
            {
                if(JobWindowSubIndex == 0)
                {
                    bool Valid;
                    int NextExp = player.JobList[JobWindowIndex].GetNextExp(out Valid);

                    if (!Valid) return;

                    if (player.ConsumeJobExp(NextExp))
                    {
                        player.JobList[JobWindowIndex].JobLevelUp();
                        JobWindowSubText.text =
                            player.JobList[JobWindowIndex].Name + "Lvが" + player.JobList[JobWindowIndex].Level + "になった！";
                    }
                    else
                    {
                        JobWindowSubText.text =
                            "技能ポイントが足りません！";
                    }
                    LevelUpDecision = false;
                }
                return;
            }

            if (key == 2)
            {
                JobWindowSubIndex = 1;
            }

            if (key == 8)
            {
                JobWindowSubIndex = 0;
            }
        }

        // サブメニュー
        JobWindowText.text = "技能ポイント : " + player.Status.jobexp + "\n";
        if (player.JobList.Count != 0)
        {
            for (int i = 0; i < player.JobList.Count; i++)
            {
                bool Valid;
                if (player.JobList.Count < (i + 1))
                {
                    break;
                }
                if (i == JobWindowIndex)
                {
                    JobWindowText.text += SelectionTag;
                }
                JobWindowText.text += player.JobList[i].Name;
                JobWindowText.text += " Lv:";
                JobWindowText.text += player.JobList[i].Level;
                JobWindowText.text += " / Next ";
                JobWindowText.text += player.JobList[i].GetNextExp(out Valid);
                JobWindowText.text += "pt";
                if (i == JobWindowIndex)
                {
                    JobWindowText.text += SelectionTagFooter;
                }
                JobWindowText.text += "\n";
            }
        }

        // サブメニュー
        if (LevelUpDecision)
        {
            JobWindowSubText.text = "レベルアップしますか？\n";
            if (JobWindowSubIndex == 0) JobWindowSubText.text += SelectionTag;
            JobWindowSubText.text += "Yes!";
            if (JobWindowSubIndex == 0) JobWindowSubText.text += SelectionTagFooter;
            JobWindowSubText.text += "\n";
            if (JobWindowSubIndex == 1) JobWindowSubText.text += SelectionTag;
            JobWindowSubText.text += "No!\n";
            if (JobWindowSubIndex == 1) JobWindowSubText.text += SelectionTagFooter;
        }

    }
}
