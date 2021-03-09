using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//PURPOSE: Does the animation for the player's Action bar in their UI (goes up and down as player uses/gains action)
public class ActionBar : MonoBehaviour
{
    [SerializeField]
    private Image ActionBarBorder;//The border object for the action charges
    public Sprite[] ActionBarBorderSprite;//The different available sprites for the ActionBarBorder
    [SerializeField]
    private float ActionsChargesImageYBase;//Where the bottom of the Action Charges image should always be
    private const int SPRITE_HEIGHT_PER_ACTION = 72;//How tall every action of the image is - allows for re-fixing its size - reduced using an IENumerator
    
    void Start() {
        //At start, refreshes actions
        StartCoroutine(RefreshActionUI(PlayerControl.Instance.MaxActions, 0, 0.5f));//Fills the entire action bar very quickly
    }
    
    //This Enumerator will refresh the player's action bar or decrease it, over the time it takes for the player to do what they are doing
    public IEnumerator RefreshActionUI(float curActions, float prevActions, float timeTo) {
        float curTime = 0;
        while(curTime < timeTo) {
            curTime += Time.deltaTime;
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(prevActions, curActions, curTime / timeTo), transform.localScale.z);
            //Because the expansion happens in both direcitons, it also needs to fix its position
                //It fixes position by having its y-value be changed by 1/2 of whatever the over change was
            transform.localPosition = new Vector3(transform.localPosition.x, ActionsChargesImageYBase + transform.localScale.y / 2 * SPRITE_HEIGHT_PER_ACTION, transform.localPosition.z);
            yield return 0;
        }
        transform.localScale = new Vector3(transform.localScale.x, curActions, transform.localScale.z);
        transform.localPosition = new Vector3(transform.localPosition.x, ActionsChargesImageYBase + transform.localScale.y / 2 * SPRITE_HEIGHT_PER_ACTION, transform.localPosition.z);
        yield return 0;
    }
    //This function is called at the beginning of the player's turn, to see if the ActionBarBack needs to be changed
    public void CheckActionBar(bool stimulated) {
        //Player inputs whether the player is currently stimulated or not, and that determines which ActionBarBorder is active
        if(stimulated) {
            ActionBarBorder.sprite = ActionBarBorderSprite[1];
        }
        else {
            ActionBarBorder.sprite = ActionBarBorderSprite[0];
        }
    }
}
