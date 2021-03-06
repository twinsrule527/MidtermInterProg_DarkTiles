﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Axe Item: While being held, moving through Briar Bushes only takes 1 movement
    //On use: Destroy all Briar bushes and Skulls on your Tile and in adjacent Tiles - Briars destroyed this way have a chance to drop a new positive item
public class ItemAxe : Item
{
    public override void Awake()
    {
        base.Awake();
        pickupable = true;
        Type = ItemType.Axe;
    }

    //When used, destroys all skulls and briar patches on your tile and adjacent tiles
    public override void Use(float perTime)
    {
        if(perTime == 0) {
        //At the moment, does it all at the moment you use it
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        TileTraits[] gottenTiles = TileManager.Instance.GetAdjacency(tempPos);//Holds the 5 tiles you are going to destroy stuff on
        //Cycles through the spots, and destroys each skull and briar patch found
        for(int i = 0; i < gottenTiles.Length; i++) {
            Item tempItem = gottenTiles[i].placedItem;
            if(tempItem != null) {
                //If it's a briar, you destroy the briar, which has a chance of revealing an item
                if(tempItem.Type == ItemType.Briar) {
                    //First, calls the script for when the object is removed
                    tempItem.RemoveFromBoard(gottenTiles[i].position);
                    Destroy(tempItem.gameObject);
                }
                //If it's a skull, the skull is destroyed, and the Tile the skull is on needs to be rechecked
                else if(tempItem.Type == ItemType.Skull) {
                    //First, calls the script for when the object is removed
                    tempItem.RemoveFromBoard(gottenTiles[i].position);
                    Destroy(tempItem.gameObject);
                }
            }
        }
        //And the axe is destroyed
        Destroy(gameObject);
        }
    }
}