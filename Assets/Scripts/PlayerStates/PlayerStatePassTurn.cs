using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePassTurn : PlayerState
{
    float curTime;
    public override void Run()
    {
        //Runs the state for a given amount of time
        curTime += Time.deltaTime;
        if(curTime >= PlayerControl.TIMEFORACTION) {
            //Just as a double-check, the player will only change states from this if this is its current state
            if(PlayerControl.Instance.CurrentState == this) {
                PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
            }
        }
    }

    public override void Enter()
    {
        curTime = 0;
        TileManager.Instance.IncreaseDarknessEndTurnTiles(PlayerControl.Instance.transform.position);
        //Between the TileManager and the player acting, the Lantern Upkeep occurs (which determines whether the game ends)
        TileManager.LANTERN.Upkeep();
        PlayerControl.Instance.StartTurn();
    }
}
