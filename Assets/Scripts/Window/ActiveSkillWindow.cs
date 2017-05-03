using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillWindow : MenuWindow
{
    public int JobWindowIndex = 0;
    public int PreviousJobWindowIndex = -255;
    public Text JobWindowText;

    public int JobWindowSubIndex = 0;
    
    Player player;

    public string JobActivateTag = "E:";
    public string JobPassiveTag = "P:";

    List<SkillData> SkillList;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void MenuSpecificUpdate()
    {
        this.gameObject.SetActive(true);

        player = Player.instance;

        SkillList = new List<SkillData>();
        SkillList.Clear();

        List<SkillData> UsableSkill = JobManager.instance.GetUsableSkill(player);

        for (int i = 0; i < UsableSkill.Count; i++)
        {
            if (UsableSkill[i].IsActiveSkill) SkillList.Add(UsableSkill[i]);
        }
        for (int i = 0; i < UsableSkill.Count; i++)
        {
            if (!UsableSkill[i].IsActiveSkill) SkillList.Add(UsableSkill[i]);
        }

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
            if (SkillList[JobWindowIndex].IsActiveSkill)
            {
                if (player.ActiveSkillID == SkillList[JobWindowIndex].ID) player.ActiveSkillID = "";
                else player.ActiveSkillID = SkillList[JobWindowIndex].ID;
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

        if (JobWindowIndex > SkillList.Count - 1)
        {
            JobWindowIndex = SkillList.Count - 1;
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
        if (SkillList.Count != 0)
        {
            for (int i = 0; i < SkillList.Count; i++)
            {
                if (SkillList.Count < (i + 1))
                {
                    break;
                }
                if (SkillList[i].IsActiveSkill)
                {
                    if (i == JobWindowIndex)
                    {
                        JobWindowText.text += SelectionTag;
                    }
                    if (SkillList[i].ID == player.ActiveSkillID)
                    {
                        JobWindowText.text += JobActivateTag;
                    }
                    JobWindowText.text += SkillList[i].Name;
                    if (i == JobWindowIndex)
                    {
                        JobWindowText.text += SelectionTagFooter;
                    }
                    JobWindowText.text += "\n";
                }
                else
                {
                    if (i == JobWindowIndex)
                    {
                        JobWindowText.text += SelectionTag;
                    }
                    JobWindowText.text += JobPassiveTag;
                    JobWindowText.text += SkillList[i].Name;
                    if (i == JobWindowIndex)
                    {
                        JobWindowText.text += SelectionTagFooter;
                    }
                    JobWindowText.text += "\n";
                }
            }
        }

    }
}
