using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State in the PlayerStateMachine for when the Player is able to take an action
public class PlayerStateTakeAction : PlayerState
{
    //Has a set of constant KeyCodes for whatever should be the Inputs for various actions
    private const KeyCode USEKEY = KeyCode.Z;
    private const KeyCode PICKUPKEY = KeyCode.X;
    private const KeyCode DROPKEY = KeyCode.C;
    private const KeyCode ENDTURNKEY = KeyCode.Return;
    public PlayerStateTakeAction(PlayerControl player): base(player) {

    }
    public override void Run() {
        //The player can enter almost any action here
        //All actions in this Run function follow the same pattern:
            //Check to see if the correct input key is given
            //Check to see if the player has enough actions
            //call the PlayerControl's function for that action (which will change state)
        //First, the player can enter movement:
        if(Input.GetKeyDown(KeyCode.UpArrow)) {
            if(myPlayer.Actions >= myPlayer.MOVEACTIONS) {
                myPlayer.Move(new Vector2Int(0, 1));
            }
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)) {
            if(myPlayer.Actions >= myPlayer.MOVEACTIONS) {
                myPlayer.Move(new Vector2Int(0, -1));
            }
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)) {
            if(myPlayer.Actions >= myPlayer.MOVEACTIONS) {
                myPlayer.Move(new Vector2Int(1, 0));
            }
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            if(myPlayer.Actions >= myPlayer.MOVEACTIONS) {
                myPlayer.Move(new Vector2Int(-1, 0));
            }
        }
        //Second, the player can use whatever Item they are holding
        else if(Input.GetKeyDown(USEKEY)) {
            if(myPlayer.Actions >= myPlayer.USEACTIONS) {
                myPlayer.UseItem();
            }
        }
        //Third, the player can pickup item they are on top of
        else if(Input.GetKeyDown(PICKUPKEY)) {
            if(myPlayer.Actions >= myPlayer.PICKUPACTIONS) {
                myPlayer.PickUpItem();
            }
        }
        //Fourth, the player can drop whatever item they are holding onto their tile
        else if(Input.GetKeyDown(DROPKEY)) {
            if(myPlayer.Actions >= myPlayer.DROPACTIONS) {
                myPlayer.DropItem();
            }
        }
        //Lastly, the player can choose to end their turn early
        else if(Input.GetKeyDown(ENDTURNKEY)) {
            //This is something you can only do after you've taken at least 1 action
            if(myPlayer.Actions < myPlayer.MaxActions) {
                //TODO: Needs to end turn
            }
        }
    }
}
