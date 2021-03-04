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
    public override void Run() {
        //The player can enter almost any action here
        //All actions in this Run function follow the same pattern:
            //Check to see if the correct input key is given
            //Check to see if the player has enough actions
            //call the PlayerControl's function for that action (which will change state)
        //First, the player can enter movement:
        if(Input.GetKeyDown(KeyCode.UpArrow)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.MOVEACTIONS) {
               PlayerControl.Instance.Move(new Vector2Int(0, 1));
            }
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.MOVEACTIONS) {
               PlayerControl.Instance.Move(new Vector2Int(0, -1));
            }
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.MOVEACTIONS) {
               PlayerControl.Instance.Move(new Vector2Int(1, 0));
            }
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.MOVEACTIONS) {
               PlayerControl.Instance.Move(new Vector2Int(-1, 0));
            }
        }
        //Second, the player can use whatever Item they are holding
        else if(Input.GetKeyDown(USEKEY)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.USEACTIONS) {
               PlayerControl.Instance.UseItem();
            }
        }
        //Third, the player can pickup item they are on top of
        else if(Input.GetKeyDown(PICKUPKEY)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.PICKUPACTIONS) {
               PlayerControl.Instance.PickUpItem();
            }
        }
        //Fourth, the player can drop whatever item they are holding onto their tile
        else if(Input.GetKeyDown(DROPKEY)) {
            if(PlayerControl.Instance.Actions >=PlayerControl.Instance.DROPACTIONS) {
               PlayerControl.Instance.DropItem();
            }
        }
        //Lastly, the player can choose to end their turn early
        else if(Input.GetKeyDown(ENDTURNKEY)) {
            //This is something you can only do after you've taken at least 1 action
            if(PlayerControl.Instance.Actions <PlayerControl.Instance.MaxActions) {
                //TODO: Needs to end turn
            }
        }
    }
}
