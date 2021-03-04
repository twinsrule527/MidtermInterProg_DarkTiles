using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Skull Item - The only negative Item in the game
    //While being held, increases darkness of adjacent tiles
    //When used, keeps a sharp boost in darkness to some random adjacent tiles
    //While dropped, increases the darkness of the Tile it is sitting on
public class ItemSkull : Item
{
    public override void Awake()
    {
        base.Awake();
        pickupable = true;
    }
    public override void Use(float perTime)
    {
        //Choose a random nearby tile (or multiple, and increase its darkness)
            //Current idea: Increase a random adjacent tile by 4, a random adjacent tile by 3, by 2, by 1
    }

    //When this item is dropped, it increases the effective darkness of the tile it is on
    public override void Drop()
    {
        base.Drop();
    }
}
