using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Berry Item - Serves as a stimulant: if held at the beginning of your turn, you can take an extra move for free during your turn
    //When used, doubles your actions for a certain number of turns - only 1 berry can be active this way at a time
public class ItemBerry : Item
{
    private const int STIMULANT_TURNS = 10;//How many turns the Berry stimulates a player for
    public override void Awake()
    {
        base.Awake();
        pickupable = true;
        Type = ItemType.Berry;
    }

    //Is not usable if you are currently under the effect of another Berry
    public override bool Usable()
    {
        //If under the effect of a berry
        if(PlayerControl.Instance.turnsStimulatedRemaining > 0) {
            return false;
        }
        return true;
    }

    //When used, doubles your number of actions for a certain number of terms (determined by const above)
    public override void Use(float perTime)
    {
        if(perTime == 0) {
            //Make the player stimulated for future turns
            PlayerControl.Instance.turnsStimulatedRemaining = STIMULANT_TURNS;
            
            //Aura grows to be the size of the player before disappearing
            StartCoroutine(ChangeAura(PlayerControl.Instance.transform.position, 0, 1, PlayerControl.TIMEFORACTION, false));
        
            
        }
        else if(perTime == 1) {
            //Then, the berry is destroyed
            Destroy(gameObject);
        }
    }
}
