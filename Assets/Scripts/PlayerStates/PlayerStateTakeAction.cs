using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State in the PlayerStateMachine for when the Player is able to take an action
public class PlayerStateTakeAction : PlayerState
{
    public PlayerStateTakeAction(PlayerControl player): base(player) {

    }
    public override void Run() {
        //The player can enter almost any action here

        //First, the player can enter movement:
        if(Input.GetKeyDown(KeyCode.UpArrow)) {
            myPlayer.Move(new Vector2Int(0, 1));
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)) {
            myPlayer.Move(new Vector2Int(0, -1));
        }
    }
}
