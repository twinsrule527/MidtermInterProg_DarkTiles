using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCandle : Item
{
    public override void Use() {

    }
    //Candle is usable only if there is no item where you are standing
    public override bool Usable() {
        return true;
    }
}
