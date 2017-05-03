using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ichigo : Item {

    // 生存数
    public static int Count = 0;
    // Score
    public static int TotalCount = 0;
    // Use this for initialization
    protected override void Start () {
        Count++;
        base.Start();
	}

    protected override void OnDestroy()
    {
        TotalCount++;
        Count--;
        //GameManager.instance.ConOut("いちごゲット！ Score : " + Ichigo.TotalCount);
        base.OnDestroy();
    }
}
