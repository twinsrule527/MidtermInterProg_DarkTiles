using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A class used for each object - each type of item inherits from this
public enum ItemType {
    Candle,
    Skull,
    Berry,
    Axe,
    Oil,
    Briar
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
    public bool pickupable;//Whether the object can be picked up in its current form - only true if on the ground, and not been used
    //There are 4 main functions for each Item: Pickup, Drop, Use, and Hold

    //Called when the object is picked up - so far, no objects have special versions of this
    public virtual void Pickup() {
        //Object becomes invisible
        GetComponent<SpriteRenderer>().enabled = false;
    }
    
    //Called when the item is dropped on the ground
    public virtual void Drop() {

    }

    //Called when you use the item - unique to each item type - consumes the item
    public virtual void Use() {

    }
    
    //Called to check whether the item being held is actually usable at the moment (for ex., a Candle must be able to be dropped to be able to be used, or a berry cannot be used while another one's effect is going)
    public virtual bool Usable() {
        return true;
    }

    //Called during update while you are holding the item if the item has a special ability
    public virtual void Hold() {

    }
}
