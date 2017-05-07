using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentJobWindow : MenuWindow
{
    public int JobWindowIndex = 0;
    public int PreviousJobWindowIndex = -255;
    public Text JobWindowText;

    public int JobWindowSubIndex = 0;

    public string JobActivateTag = "E:";

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void MenuSpecificUpdate()
    {
        this.gameObject.SetActive(true);

        int key = 0;

        if (Input.GetAxisRaw("Horizontal") > 0) key = 6;
        else if (Input.GetAxisRaw("Horizontal") < 0) key = 4;
        else if (Input.GetAxisRaw("Vertical") > 0) key = 8;
        else if (Input.GetAxisRaw("Vertical") < 0) key = 2;

        if (Input.GetKeyDown("x"))
        {
            State = WindowState.Inactive;
            return;
        }

        if (Input.GetKeyDown("z"))
        {
            if (player.CurrentJob.ID != player.JobList[JobWindowIndex].ID)
            {
                player.ResetEquip();
                player.CurrentJob = player.JobList[JobWindowIndex];
                MessageWindow.instance.ConOut("メイン技能を変更しました。装備を全て解除しました。");
            }
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

        JobWindowText.text = "";
        if (player.JobList.Count != 0)
        {
            for (int i = 0; i < player.JobList.Count; i++)
            {
                if (player.JobList.Count < (i + 1))
                {
                    break;
                }
                if (i == JobWindowIndex)
                {
                    JobWindowText.text += SelectionTag;
                }
                if (player.JobList[i].ID == player.CurrentJob.ID)
                {
                    JobWindowText.text += JobActivateTag;
                }
                JobWindowText.text += player.JobList[i].Name;
                JobWindowText.text += " Lv:";
                JobWindowText.text += player.JobList[i].Level;
                if (i == JobWindowIndex)
                {
                    JobWindowText.text += SelectionTagFooter;
                }
                JobWindowText.text += "\n";
            }
        }

    }
}
