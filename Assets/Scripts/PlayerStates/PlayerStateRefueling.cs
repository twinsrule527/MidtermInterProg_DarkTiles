using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//THis state occurs when the player stands on the Lantern while holding an Oil - It empties their Oils until they're holding no more at the top of their inventory
public class PlayerStateRefueling : PlayerState
{
    private float curTime;
    public override void Run()
    {
        //This still isn't happening to the degree it should
        curTime += Time.deltaTime;
        if(curTime >= PlayerControl.TIMEFORACTION) {
            bool checkRefuel = false;
            PlayerControl.Instance.Refuel(out checkRefuel);
            TileManager.LANTERN.RefreshLevel();
            //if they do refuel, curTime is set back to 0
            if(checkRefuel) {
                curTime = 0;
            }
            //Otherwise, go to next state
            else {
                //If no actions left, go to end of turn
                if(PlayerControl.Instance.Actions <= 0) {
                    PlayerControl.Instance.ChangeState(PlayerControl.Instance.statePassTurn);
                }
                //Otherwise, they just go back to the waiting state
                else {
                    PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
                }
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        curTime = 0;
        //At enter, pops the topmost Oil, refueling the Lantern
        bool checkRefuel = false;
        PlayerControl.Instance.Refuel(out checkRefuel);
    }
}
