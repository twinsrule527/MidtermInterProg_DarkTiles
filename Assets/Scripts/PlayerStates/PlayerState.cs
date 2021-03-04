using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A base for the Player State Machine - Based off of the BlobState from Homework 4: BadProj
public abstract class PlayerState
{
    public virtual void Enter() {

    }

    public virtual void Leave() {

    }
    //Every PlayerState must have a Run function (serves as an update function, because this is all run through the PlayerControl Script)
    public abstract void Run();
    
    //I decided to make the PlayerControl a Singleton, which removed the necessity to know the specific script every is being called from
    /*
    //PlayerState needs to have access to a player
    public PlayerState(PlayerControl player) {
        myPlayer = player;
    }
    */
}
