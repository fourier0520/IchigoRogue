using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuWindow : MonoBehaviour {

    public static bool isKeyRepeat = false;

    public GameObject ChildrenWindow;

    public string SelectionTag = "";
    public string SelectionTagFooter = "";

    public static Character player = null;

    public enum WindowState
    {
        Inactive, Active, WaitChildren, Decide
    }

    public WindowState State = WindowState.Inactive;

    public enum WindowTurnCommand
    {
        UseItem, Undef,
        ThrowItem,
        MoveMap,
        PutItem
    }

    static public WindowTurnCommand Command = WindowTurnCommand.Undef;
    static public ItemProfile UseItem;

    public enum MenuWindowMode
    {
        Main, Item, ItemSelect
    }

    public MenuWindowMode Mode = MenuWindowMode.Main;
    public MenuWindowMode PreviousMode = MenuWindowMode.Main;

    private void Awake()
    {
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        if(SelectionTag == "")
            SelectionTag = "<b><color=lime><";
        if(SelectionTagFooter == "")
            SelectionTagFooter = "></color></b>";

        State = WindowState.Inactive;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    protected void Update () {

        if (isKeyRepeat)
            return;

        if (State == WindowState.Active)
        {
            this.gameObject.SetActive(true);
            Command = WindowTurnCommand.Undef;

            MenuSpecificUpdate();
            MenuSpecificReflesh();
        }
        else
        {
            MenuSpecificReflesh();
        }

        if (State == WindowState.Inactive)
        {
            this.gameObject.SetActive(false);
            return;
        }

        if (State == WindowState.Decide)
        {
            this.gameObject.SetActive(false);
            return;
        }

        if (State == WindowState.WaitChildren)
        {
            if (ChildrenWindow.GetComponent<MenuWindow>().State == WindowState.Inactive)
            {
                State = WindowState.Active;
            }

            if (ChildrenWindow.GetComponent<MenuWindow>().State == WindowState.Decide)
            {
                State = WindowState.Decide;
            }
            return;
        }
    }

    protected virtual void MenuSpecificUpdate()
    {
    }

    protected virtual void MenuSpecificReflesh()
    {
    }

    public IEnumerator KeyRepert(float time)
    {
        isKeyRepeat = true;
        yield return new WaitForSeconds(time);
        isKeyRepeat = false;
    }
}
