using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Oil Item - When used, creates a flash of light, halving the light value of the tile you're on as well as all adjacent tiles
    //While being held, if you walk onto the Lantern, you immediately drop it and refill the Lantern
public class ItemOil : Item
{
    public override void Awake()
    {
        NameText = "OIL CAN";
        AbilityText = "While held, refuels your Lantern if it is walked over (but this is destroyed in the process). When used, brightens nearby spaces.";
        base.Awake();
        pickupable = true;
        Type = ItemType.Oil;
    }

    //When used, halves all darkness on your tile, as well as all adjacent tiles - does not affect modifiers
    public override void Use(float perTime)
    {
        if(perTime == 0) {
        
        //Gets all the adjacent tiles (including the tile you're standing on)
        Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        TileTraits[] gottenTiles = TileManager.Instance.GetAdjacency(tempPos);//Holds the 5 tiles you are going to destroy stuff on
        //Runs through all the gotten tiles, halving their darkness, then adding them back to the TileManager
        for(int i = 0; i <gottenTiles.Length; i ++) {
            gottenTiles[i].darkLevel /= 2;
            TileManager.Instance.TileDictionary[gottenTiles[i].position] = gottenTiles[i];
        }
        //Then, the oil itself is destroyed
        Destroy(gameObject);

        }
    }

    //A Coroutine for when you refuel your lantern with this Oil can, so that it is destroyed after a moment of time
    public IEnumerator Refueled() {
        //It starts an Aura growth
        StartCoroutine(ChangeAura(PlayerControl.Instance.transform.position, 0f, 2f, PlayerControl.TIMEFORACTION, false));
        yield return new WaitForSeconds(PlayerControl.TIMEFORACTION);
        Destroy(gameObject);
    }
}
