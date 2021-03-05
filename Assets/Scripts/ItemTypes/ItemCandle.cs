using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Candle Item: While being held, adjacent tiles have their darkness temporarily decreased
    //When used, you place it on your tile, and it lights stuff up for several turns
public class ItemCandle : Item
{
    //How much light each of its three radius' provide to nearby tiles, both as starting Constants and over time
    private const int MAX2LIGHT = 4;
    private const int MAX1LIGHT = 8;
    private const int MAX0LIGHT = 12;
    private int radius2Light;//Squares 2 steps away
    private int radius1Light;//Adjacent squares
    private int radius0Light;//Its own square
    public override void Use(float perTime) {
        //When used, the Candle is placed on the Tile you are standing on, and it brightens up that Tile and nearby ones
        //At the moment, only does stuff on the first call - TODO: do it quickly over time
        if(perTime == 0) {
            //Add this to the List in the PlayerControl's end turn List of Upkeep Items
            PlayerControl.Instance.UpkeepItems.Add(this);
            //Goes to the position of the Player and becomes visible again
            transform.position = PlayerControl.Instance.transform.position + new Vector3(0f, 0f, 1f);
            mySpriteRenderer.enabled = true;
            //Each light value is set to its maxValue
            radius2Light = MAX2LIGHT;
            radius1Light = MAX1LIGHT;
            radius0Light = MAX0LIGHT;
            //Still needs to actually assign light values to those tiles
        }
    }
    //Candle is usable only if there is no item where you are standing
    public override bool Usable() {
        //To check if it's usable, we need to get the Tile the player is standing 
        Vector2Int tempVector = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));//This gets the player's position in from of a Vector2Int
        TileTraits TileCheck = TileManager.Instance.TileDictionary[tempVector];
        //If there is no object on that square, it is usable, otherwise it isn't
        if(TileCheck.placedItem == null) {
            return true;
        }
        else {
            return false;
        }
    }

    //The candle has an upkeep after it has been used, because it stays in play losing power over time
    public override void Upkeep()
    {
        base.Upkeep();
        //Light levels go down during Upkeep
        radius0Light--;
        //Larger radius light only goes down if it is above 0
        if(radius1Light > 0) {
            radius1Light--;
        }
        if(radius2Light > 0) {
            radius2Light--;
        }
        //When radius0 = 0, the object is destroyed
        if(radius0Light == 0) {
            Destroy(gameObject);
        }
    }

    //This is a pickupable Item
    public override void Awake() {
        base.Awake();
        pickupable = true;
        Type = ItemType.Candle;
    }
}
