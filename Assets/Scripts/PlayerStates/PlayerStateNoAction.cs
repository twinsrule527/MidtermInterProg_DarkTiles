using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This state occurs when the player attempts to perform an action they are not able to perform
public class PlayerStateNoAction : PlayerState
{
    public const float TIME_TO_WAIT = 0.25f;//How long the player has to wait after attempting a not-allowed action
    private float curTime;
    public override void Run() {
        curTime += Time.deltaTime;
        if(curTime >= TIME_TO_WAIT) {
            PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateTakeAction);
        }
    }

    public override void Enter()
    {
        //When it enters this phase, it starts the Action Bar Coroutine to flash, and its time is set to 0
        curTime = 0f;
        PlayerControl.Instance.myActionBar.StartCoroutine(PlayerControl.Instance.myActionBar.UnavailableAction(TIME_TO_WAIT));
    }
}
