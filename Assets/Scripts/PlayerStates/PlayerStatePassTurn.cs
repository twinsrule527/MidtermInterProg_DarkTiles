using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePassTurn : PlayerState
{
    public override void Run()
    {
       PlayerControl.Instance.StartTurn();
       TileManager.Instance.IncreaseDarknessEndTurnTiles(PlayerControl.Instance.transform.position);
       PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
    }
}
