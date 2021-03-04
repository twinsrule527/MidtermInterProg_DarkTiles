using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePassTurn : PlayerState
{
    public override void Run()
    {
       PlayerControl.Instance.StartTurn();
       PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
    }
}
