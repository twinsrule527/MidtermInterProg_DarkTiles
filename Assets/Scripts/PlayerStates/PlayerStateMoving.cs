using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State in the PlayerStateMachine for when the player is moving
public class PlayerStateMoving : PlayerState
{
    private Vector3 startPos;//The Player's position before they move
    private Vector3 target;//The position the player is moving to
    private float curTime;//How long the player has been in this state
    public override void Run()
    {
        curTime += Time.deltaTime;
        //After the max time, it reaches the end
        if(curTime > PlayerControl.TIMEFORACTION) curTime = PlayerControl.TIMEFORACTION;

        PlayerControl.Instance.transform.position = Vector3.Lerp(startPos, target, curTime / PlayerControl.TIMEFORACTION);
        Camera.main.transform.position = PlayerControl.Instance.transform.position + PlayerControl.Instance.CameraOffset;
        //After it reaches its target, it goes to the next state
        if(curTime == PlayerControl.TIMEFORACTION) {
            //Check to see if the player is on (0, 0) and is holding an Oil - If they are, they go into the Loop state of placing Oil in the Lantern
            if(Mathf.FloorToInt(target.x) == 0 && Mathf.FloorToInt(target.y) == 0) {
                Item tempItem = PlayerControl.Instance.InventoryPeek;
                if(tempItem.Type == ItemType.Oil) {
                    //Go into the Player's state of dropping Oil into the lantern
                    PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateRefueling);
                    return;
                }
            }
            //If the player has no actions, they pass the turn
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
        startPos = PlayerControl.Instance.transform.position;
        target = startPos + PlayerControl.Instance.movementVector;
        curTime = 0;
        //TODO: Add something that if the player is moving in a direction that does not have tiles, it needs to generate a new column/row
    }
}
