using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This null item starts in the player's inventory - Its only purpose is so that the player's inventory is technically never empty, making calling the Inventory more elegant
//The item has a sprite renderer, which isn't used, so to not disrupt any existing Item scripts
public class NullItem : Item
{
    public override void Awake()
    {
        base.Awake();
        Type = ItemType.Null;
        pickupable = false;
    }
    public override bool Usable()
    {
        return false;
    }
}
