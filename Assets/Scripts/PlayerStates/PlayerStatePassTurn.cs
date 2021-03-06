using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePassTurn : PlayerState
{
    public override void Run()
    {
       TileManager.Instance.IncreaseDarknessEndTurnTiles(PlayerControl.Instance.transform.position);
       //Between the TileManager and the player acting, the Lantern Upkeep occurs (which determines whether the game ends)
       TileManager.LANTERN.Upkeep();
       PlayerControl.Instance.StartTurn();
       PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
    }
}
