using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State for when the player is dropping an item
public class PlayerStateDrop : PlayerState 
{    
    private float curTime;
    public override void Run()
    {
        //At the moment, when you drop something, it just runs through the waitTime
        curTime += Time.deltaTime;
        if(curTime > PlayerControl.TIMEFORACTION) {
            if(PlayerControl.Instance.Actions <= 0) {
                PlayerControl.Instance.ChangeState(PlayerControl.Instance.statePassTurn);
            }
            //Otherwise, they just go back to the waiting state
            else {
                PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        curTime = 0;
    }
}
