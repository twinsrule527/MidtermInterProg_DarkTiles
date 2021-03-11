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
    public const int USE_SPREAD_DARKNESS = 4;//How much the darkness spreads by when you use the USE action
    //TODO: Make all of them Start, rather than Awake?
    public void Start()
    {
        base.Awake();
        pickupable = true;
        Type = ItemType.Skull;
        //The Tile it starts on has its darkness increased
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        TileTraits tempTraits = TileManager.Instance.TileDictionary[tempPos];
        if(tempTraits.darkLevel == 0) {
            tempTraits.darkLevel = 1;
        }
        tempTraits.darkModifier += SELF_DARKNESS;
        TileManager.Instance.TileDictionary[tempPos] = tempTraits;
        //Rest is going to have to be done through the TileManager - Tile is refreshed to match the correct color
        TileManager.Instance.RefreshTile(tempPos);

    }
    public override void Use(float perTime)
    {
        if(perTime == 0) {
        //Choose a random nearby tile (or multiple, and increase its darkness) - Increase darkness by 1 4 times
            //REMOVED IDEA: idea: Increase a random adjacent tile by 4, a random adjacent tile by 3, by 2, by 1
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        TileTraits[] gottenTiles = TileManager.Instance.GetAdjacency(tempPos);
        //Picks a random number from adjacent tiles (excluding its own tile) a number of times  =  to the darknesses potency
       for(int i = 0; i < USE_SPREAD_DARKNESS; i++) {
            //Will not pick the 0th element, because it will not spread darkness on where you're standing
            int rnd = Random.Range(1, gottenTiles.Length);
            gottenTiles[rnd].darkLevel = Mathf.Clamp(gottenTiles[rnd].darkLevel + 1, 0, TileManager.MAXDARKLEVEL);
        }
        //Then, the gotten tiles become the tiles in the dictionary
        for(int i = 1; i < gottenTiles.Length; i ++ ) {
            TileManager.Instance.TileDictionary[gottenTiles[i].position] = gottenTiles[i];
            //Refresh all those tiles
            TileManager.Instance.RefreshTile(gottenTiles[i].position);
        }
        //Aura grows around the player, and then will disappear, as the darkness spreads
        StartCoroutine(ChangeAura(PlayerControl.Instance.transform.position, 0, 4, PlayerControl.TIMEFORACTION, false));
        }
        else if(perTime == 1) {
            //Finally, the Skull is destroyed
            Destroy(gameObject);
        }
    }

    //When this item is dropped, it increases the effective darkness of the tile it is on
    public override void Drop()
    {
        base.Drop();
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        //Same thing occurs as when start is declared
        TileTraits tempTraits = TileManager.Instance.TileDictionary[tempPos];
        if(tempTraits.darkLevel == 0) {
            tempTraits.darkLevel = 1;
        }
        tempTraits.darkModifier += SELF_DARKNESS;
        TileManager.Instance.TileDictionary[tempPos] = tempTraits;
        //Rest is going to have to be done through the TileManager - Tile is refreshed to match the correct color
        TileManager.Instance.RefreshTile(tempPos);

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
        //Tile is then refreshed
        TileManager.Instance.RefreshTile(pos);
    }
}
