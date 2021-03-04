using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State in the PlayerStateMachine for when the player is moving
public class PlayerStateMoving : PlayerState
{
    private Vector3 startPos;//The Player's position before they move
    private Vector3 target;//The position the player is moving to
    private float curTime;//How long the player has been in this state
    public PlayerStateMoving(PlayerControl player): base(player) {
    }
    public override void Run()
    {
        curTime += Time.deltaTime;
        //After the max time, it reaches the end
        if(curTime > myPlayer.TIMEFORACTION) curTime = myPlayer.TIMEFORACTION;

        myPlayer.transform.position = Vector3.Lerp(startPos, target, curTime / myPlayer.TIMEFORACTION);

        //After it reaches its target, it goes to the next state
        if(curTime == myPlayer.TIMEFORACTION) {
            //If the player has no actions, they pass the turn
            if(myPlayer.Actions <= 0) {
                myPlayer.ChangeState(myPlayer.statePassTurn);
            }
            //Otherwise, they just go back to the waiting state
            else {
                myPlayer.ChangeState(myPlayer.stateTakeAction);
            }
        }
    }
    public override void Enter()
    {
        base.Enter();
        startPos = myPlayer.transform.position;
        target = startPos + myPlayer.movementVector;
        curTime = 0;
    }
}
