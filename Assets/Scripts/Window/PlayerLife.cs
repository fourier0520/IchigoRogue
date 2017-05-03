using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLife : MonoBehaviour {

    private Text PlayerLifeText;
    public Image PlayerLifeBarCurrent;
    public Image PlayerLifeBar;

    public Text PlayerFoodText;
    public Text PlayerGoldText;
    public Text PlayerLevelText;
    public Text PlayerFloorText;

    // Use this for initialization
    void Start () {
        PlayerLifeText = this.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {

        if (!Player.instance)
        {
            return;
        }

        // Update Life
        float LifeRate = (float)Player.instance.Status.life / (float)Player.instance.Status.life_max;
        if (LifeRate < 0f) LifeRate = 0f;
        if (LifeRate > 1f) LifeRate = 1f;
        PlayerLevelText.text = "Lv : " + Player.instance.Status.level;
        PlayerLifeText.text = "HP : " + Player.instance.Status.life + " / " + Player.instance.Status.life_max;
        PlayerLifeBarCurrent.rectTransform.localScale = new Vector3(LifeRate, 1, 1);

        PlayerFoodText.text = Math.Ceiling(Player.instance.Status.food/100f) + "%";
        PlayerGoldText.text = Player.instance.Status.gold + "G";
        PlayerFloorText.text = GameManager.instance.boardScript.level + "階";
    }
}
