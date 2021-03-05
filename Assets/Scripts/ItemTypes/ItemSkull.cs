using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//The Skull Item - The only negative Item in the game
    //While being held, increases darkness of adjacent tiles
    //When used, keeps a sharp boost in darkness to some random adjacent tiles
    //While dropped, increases the darkness of the Tile it is sitting on
public class ItemSkull : Item
{
    public const int SELF_DARKNESS = 4;//How much darkness is added to the Tile this object is on top of
    public const int USE_SPREAD_DARKNESS = 4;//How much the darkness spreads by at first when you use the USE action
    //TODO: Make all of them Start, rather than Awake?
    public void Start()
    {
        base.Awake();
        pickupable = true;
        Type = ItemType.Skull;
        //The Tile it starts on has its darkness increased
        TileTraits tempTraits = TileManager.Instance.TileDictionary[new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y))];
        if(tempTraits.darkLevel == 0) {
            tempTraits.darkLevel = 1;
        }
        tempTraits.darkModifier += SELF_DARKNESS;
        TileManager.Instance.TileDictionary[new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y))] = tempTraits;
        //Rest is going to have to be done through the TileManager
        //TODO: Need a function to deal with this

    }
    public override void Use(float perTime)
    {
        if(perTime == 0) {
        //Choose a random nearby tile (or multiple, and increase its darkness)
            //Current idea: Increase a random adjacent tile by 4, a random adjacent tile by 3, by 2, by 1
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        TileTraits[] gottenTiles = TileManager.Instance.GetAdjacency(tempPos);
        //Picks a random number from adjacent tiles (excluding its own tile) 4 times, each time, with decreasing potency of the darkness it gives - until potency = 0
        int darkPotent = USE_SPREAD_DARKNESS;
        while(darkPotent > 0) {
            //Will not pick the 0th element, because it will not spread darkness on where you're standing
            int rnd = Random.Range(1, gottenTiles.Length);
            gottenTiles[rnd].darkLevel = Mathf.Clamp(gottenTiles[rnd].darkLevel + darkPotent, 0, TileManager.MAXDARKLEVEL);
            //Potency of the darkness spreading decreases
            darkPotent--;
        }
        //Then, the gotten tiles become the tiles in the dictionary
        for(int i = 1; i < gottenTiles.Length; i ++ ) {
            TileManager.Instance.TileDictionary[gottenTiles[i].position] = gottenTiles[i];
        }
        
        //Finally, the Skull is destroyed
        Destroy(gameObject);
        }
    }

    //When this item is dropped, it increases the effective darkness of the tile it is on
    public override void Drop()
    {
        base.Drop();
    }

    //When this is removed from the void, it resets the Darkness Modifier of where it was
        //This is called when it is picked up or destroyed with an Axe
    public override void RemoveFromBoard(Vector2Int pos)
    {
        TileTraits tempTraits = TileManager.Instance.TileDictionary[pos];
        tempTraits.darkModifier -= SELF_DARKNESS;
        //Will need to change the sprite as required
        //Also, remove the item from the TileDictionary
        tempTraits.placedItem = null;
        TileManager.Instance.TileDictionary[pos] = tempTraits;
    }
}
