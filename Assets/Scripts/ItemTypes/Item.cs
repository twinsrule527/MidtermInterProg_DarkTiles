﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A class used for each object - each type of item inherits from this

//In addition to the Item class, each Item has an enum Type to make it easier to reference
public enum ItemType {
    
    Null,
    Briar,
    Oil,
    Candle,
    Berry,
    Axe,
    Skull,
    Lantern
}
public class Item : MonoBehaviour
{
    private ItemType _type;//The object's type - Set through a property, so that it's safer
    public ItemType Type {
        get {
            return _type;
        }
        set {
            _type = value;
        }
    }
    protected SpriteRenderer mySpriteRenderer;
    public SpriteRenderer MySpriteRenderer {
        get {
            return mySpriteRenderer;
        }
    }
    public virtual void Awake() {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }
    public bool pickupable;//Whether the object can be picked up in its current form - only true if on the ground, and not been used
    //There are 4 main functions for each Item: Pickup, Drop, Use, and Hold

    //Called when the object is picked up - so far, no objects have special versions of this
    public virtual void Pickup() {
        //Object becomes invisible
        mySpriteRenderer.enabled = false;
    }
    
    //Called when the item is dropped on the ground
    public virtual void Drop() {
        mySpriteRenderer.enabled = true;
        transform.position = PlayerControl.Instance.transform.position + new Vector3(0, 0, 1);
    }

    //Called when you use the item - unique to each item type - consumes the item
    public virtual void Use(float perTime) {//perTime means PercentTime - It refers to how long the Use state has been going, as a percentage of the longest Time it can go
                                            //Many Items, especially if they have mini-animations when you you play them will want this
                                            //For example, this allows the candle to spread out quickly from its start, but not all at once
    }
    
    //Called to check whether the item being held is actually usable at the moment (for ex., a Candle must be able to be dropped to be able to be used, or a berry cannot be used while another one's effect is going)
    public virtual bool Usable() {
        return true;
    }

    //Called at the end of turn (most objects don't have this, so its only called if they're in a list in the PlayerControl's EndTurn State)
    public virtual void Upkeep() {

    }

    //Called when the object is removed from something - mainly used for when Briar Patches are destroyed/Skulls are destroyed/removed
    public virtual void RemoveFromBoard(Vector2Int pos) {//Needs input of the tile's current position

    }

    //Whether the object is placed on the ground (not dropped) - at the current only matters for the Candle
    public virtual bool Placed() {
        return false;
    }
}
