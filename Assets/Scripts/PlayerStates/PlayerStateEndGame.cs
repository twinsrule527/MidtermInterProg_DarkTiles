using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//State which the player is in at the beginning/end of the game - player can't do anything, and only outside forces can change its state
public class PlayerStateEndGame : PlayerState
{
    private SpriteRenderer PlayerSprite;
    public override void Run() {
        //Doesn't do anything in its run
    }

    public override void Enter()
    {
        //On enter, the player becomes invisible
        PlayerSprite = PlayerControl.Instance.GetComponent<SpriteRenderer>();
        PlayerSprite.enabled = false;
        //Also runs the player's endgame function
        PlayerControl.Instance.EndGame();
    }

    public override void Leave()
    {
        //On leave, player becomes visible
        if(PlayerSprite != null) {
            PlayerSprite.enabled = true;
        }
    }
}
