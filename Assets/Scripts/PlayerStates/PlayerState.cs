using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A base for the Player State Machine - Based off of the BlobState from Homework 4: BadProj
public abstract class PlayerState
{
    protected PlayerControl myPlayer;
    public virtual void Enter() {

    }

    public virtual void Leave() {

    }
    //Every PlayerState must have a Run function (serves as an update function, because this is all run through the PlayerControl Script)
    public abstract void Run();
    //PlayerState needs to have access to a player
    public PlayerState(PlayerControl player) {
        myPlayer = player;
    }
}
