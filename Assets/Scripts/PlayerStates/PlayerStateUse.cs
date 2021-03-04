using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State for when the player is using an item
public class PlayerStateUse : PlayerState
{
    private float curTime;
    private Item UsedItem;//The item being used by the player
    public override void Run()
    {
        //Increases time, and leaves state if been going long enough
        curTime+=Time.deltaTime;
        if(curTime > PlayerControl.Instance.TIMEFORACTION) {
            
            UsedItem.Use(1);
            if(PlayerControl.Instance.Actions <= 0) {
                PlayerControl.Instance.ChangeState(PlayerControl.Instance.statePassTurn);
            }
            //Otherwise, they just go back to the waiting state
            else {
                PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
            }
        }
        //Otherwise, it calls the Object's use script, in case it has something to do
        else {
            UsedItem.Use(curTime / PlayerControl.Instance.TIMEFORACTION);
        }
    }
    public override void Enter()
    {
        base.Enter();
        curTime = 0;
        UsedItem = PlayerControl.Instance.UsedItem;
        Debug.Log(UsedItem);
    }
}
