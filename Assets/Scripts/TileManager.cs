﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//A Manager that has various functions to be called regarding changes made to the Tilemap

[System.Serializable]//TODO: Might remove this, if unneeded
public struct TileTraits {//This Struct is a way to have attributes and functions attached to each Tile in the game
    public Vector2Int position;//The position of this given Tile - Used for reference - because of the Tile Dictionary, isn't completely needed
    public TileBase thisTile;//The Specific Tile from the Tile Palette that this Tile is
    public float darkLevel;//The level of darkness the tile currently has
    public Item placedItem;//If there is an item on the tile, this is the Item on the Tile

}

//TODO: Make Singleton?
public class TileManager : MonoBehaviour
{
    [SerializeField]
    private List<TileBase> DarknessLevelTile;//A list of Tiles of different darkness level, from 1-15 - Is serialized in the Inspector
    [SerializeField]
    private Tilemap myTilemap;
    public Dictionary<Vector2Int, TileTraits> TileDictionary = new Dictionary<Vector2Int, TileTraits>();//This dictionary serves as a 2D array, but can go into negatives - and is full of existing tiles
    
    //A function that randomly generates a grid of Tiles inside a defined area
    public void GenerateGrid(int xStart, int yStart, int width, int height) {//Fills in all unfilled tiles in the given grid
        //Runs through every tile in the grid
        for(int x = xStart; x < xStart + width; x++) {
            for(int y = yStart; y < yStart + height; y++) {
                //Uses this tempVector because it will be referenced several times, so I don't want to recreate it several times
                Vector2Int tempVector = new Vector2Int(x, y);
                //Only does something if the given spot in the tileDictionary does not exist
                if(!TileDictionary.ContainsKey(tempVector)) {
                    GenerateTile(tempVector);
                }
            }
        }
    }
    
    //A function that generates a single tile at the given position - can only be done throught the GenerateGrid code
    private void GenerateTile(Vector2Int pos) {
        //Type of Tile generated is random, but depends in part on position in relation to (0, 0):
            //Further away = More dangerous tile, but also higher chance of having an object on the Tile
        int rnd = Random.Range(0, 16);
        myTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), DarknessLevelTile[rnd]);
        TileTraits tempTile;
        tempTile.position = pos;
        tempTile.thisTile = DarknessLevelTile[rnd];
        tempTile.darkLevel = rnd;
        tempTile.placedItem = null;
        TileDictionary.Add(pos, tempTile);
    }
    void Start() {
        GenerateGrid(-8, -5, 16, 10);
    }
}
