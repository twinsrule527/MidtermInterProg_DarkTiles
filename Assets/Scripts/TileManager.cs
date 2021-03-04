using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//A Manager that has various functions to be called regarding changes made to the Tilemap

[System.Serializable]//TODO: Might remove this, if unneeded
public struct TileTraits {//This Struct is a way to have attributes and functions attached to each Tile in the game
    public Vector2Int position;//The position of this given Tile - Used for reference - because of the Tile Dictionary, isn't completely needed
    public TileBase thisTile;//The Specific Tile from the Tile Palette that this Tile is
    public float darkLevel;//The level of darkness the tile currently has
    public int darkModifier;//Any modifiers applied to the tile are added here - The Tile's effective darkness = darkLevel + darkModifier (clamped at 0 and 15)
    public Item placedItem;//If there is an item on the tile, this is the Item on the Tile

}

//Made this a Singleton, because there are several things that need to reference it - It possibly could be abstract - Need to ask Prof
public class TileManager : Singleton<TileManager>
{
    public readonly int NUMOFITEMS = 7;//How many different items there are
    
    [SerializeField]
    private List<Item> possibleItems;//A list of all possible objects (0 = nothing, 1 = briar bush, 2 = oil, 3 = candle, 4 = berry, 5 = axe, 6 = skull)
    [SerializeField]
    private List<float> itemBasePercent;//Probability for each item to spawn at origin chunk
    [SerializeField]
    private List<float> itemIncreasePercent;//How much probability for each item changes by as you move away from origin
    private List<spawnProb> SpawnChunks = new List<spawnProb>();//A list of the probabilities for generating chunks, depending on how far away it is from the origin
    public const int FURTHESTCHUNKS = 8;//Furthest chunks from the start which have their own spawn probability
    public const int MAXDARKLEVEL = 15;//How high the darkness level can reach for a tile
    [SerializeField]
    private List<TileBase> DarknessLevelTile;//A list of Tiles of different darkness level, from 1-15 - Is serialized in the Inspector
    [SerializeField]
    private Tilemap myTilemap;
    private Dictionary<Vector2Int, bool> ExistingChunks = new Dictionary<Vector2Int, bool>();//This dictionary contains a list of all chunks which have been generated
    public Dictionary<Vector2Int, TileTraits> TileDictionary = new Dictionary<Vector2Int, TileTraits>();//This dictionary serves as a 2D array, but can go into negatives - and is full of existing tiles
   
    void Start() {
        for(int i = 0; i < FURTHESTCHUNKS; i++) {
            spawnProb tempProb;
            tempProb.darkTileProb = Mathf.Sqrt(2*(i+1));//Probability for a dark tile is a square root function
            tempProb.darkestNormTile = i + 4;//The darkest tile in each chunk is 4 darker than its distance from the origin
            tempProb.itemSpawnProb = new float[NUMOFITEMS];
            float temp = 0;
            for(int j = 0; j < NUMOFITEMS; j++) {
                tempProb.itemSpawnProb[j] = itemBasePercent[j] + itemIncreasePercent[j] * i;//The probability of each item is dependent on the base percentage plus a multiple of the increase percent
                temp += tempProb.itemSpawnProb[j];
            }
            tempProb.totalItemSpawnProb = temp;
            //Finally, it is added to the list of chunk info
            SpawnChunks.Add(tempProb);
        }
        GenerateChunk(new Vector2Int(0, 0));
    }

    
    public const int CHUNKWIDTH = 25;//The width of a chunk is a constant 25 tiles (same for height)

    //Meant to replace the Grid generation code. Generates groups in chunks, such that each chunk is 25x25
        //The input pos is the Chunk position. Multiplying it by 25 gets the center of this new chunk
    public void GenerateChunk(Vector2Int pos) {
        //Position is gotten as the bottom left corner of the chunk
        int xStart = pos.x * CHUNKWIDTH - (CHUNKWIDTH - 1) / 2;
        int yStart = pos.y * CHUNKWIDTH - (CHUNKWIDTH - 1) / 2;
        int distFromOrigin = Mathf.Abs(pos.x) + Mathf.Abs(pos.y);
        //Run through all tiles in the chunk
        for(int x = xStart; x < xStart + CHUNKWIDTH; x++) {
            for(int y = yStart; y < yStart + CHUNKWIDTH; y++) {
                //Get the current position
                Vector2Int tempVector = new Vector2Int(x, y);
                //Check it against the dictionary of tiles, to see if a new one has to be added
                if(!TileDictionary.ContainsKey(tempVector) ) {
                    GenerateTile(tempVector, distFromOrigin);
                }
            }
        }
    }

    //A function that generates a single tile at the given position - can only be done throught the GenerateGrid code
    private void GenerateTile(Vector2Int pos, int chunkDist) {
        
        //Type of Tile generated is random, but depends in part on position in relation to (0, 0):
            //Further away = More dangerous tile, but also higher chance of having an object on the Tile
        float rnd = Random.Range(0f, 100f);
        TileTraits tempTile;
        int darkTemp = 0;//Placeholder variable used to determine darkness
        //If it is a high enough probability (very low chance), it generates a super dark tile

        if(rnd > 100f - SpawnChunks[chunkDist].darkTileProb) {
            darkTemp = Random.Range(12, 16);
        }
        else {
            //Otherwise, it is the int value of the remainder of rnd when divided by the darkest normal color
            darkTemp = Mathf.FloorToInt(rnd % SpawnChunks[chunkDist].darkestNormTile);
        }
        //Sets the Tile and its value in the Dictionary
        myTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), DarknessLevelTile[darkTemp]);
        tempTile.thisTile = DarknessLevelTile[darkTemp];
        tempTile.position = pos;
        tempTile.darkLevel = darkTemp;
        tempTile.darkModifier = 0;
        tempTile.placedItem = null;//Sets the placed item as null as a precaution
        //Now, determine what item is on the chunk, if any
        rnd = Random.Range(0f, SpawnChunks[chunkDist].totalItemSpawnProb);
        float temp = 0;
        //Cycles through item probabilities, to see if it has one
        for(int i = 0; i < NUMOFITEMS; i ++) {
            temp += SpawnChunks[chunkDist].itemSpawnProb[i];
            //If this goes past the needed number, it either generates (or doesn't generate) the corresponding item
            if(temp >= rnd) {
                //If no item exists at that spot, nothing happens
                if(possibleItems[i] == null) {}
                //Otherwise, an item is generated, and is added to the TileAttributes
                else {
                    Item tempItem = Instantiate(possibleItems[i], new Vector3(pos.x +0.5f, pos.y +0.5f, -1), Quaternion.identity);
                    tempTile.placedItem = tempItem;
                    //Act as if the item was just dropped, in case it needs that - TODO?
                    //tempItem.Drop();
                }
                //Eitherway, this leads to breaking out of the for loop
                break;
            }
        }
        TileDictionary.Add(pos, tempTile);
    }

    public struct spawnProb {//This struct contains the set of data needed to generate a tile in a given chunk
        public float darkTileProb;//Probability of spawning an extremely dark tile (level 12-15)
        public int darkestNormTile;//The darkest tile that will spawn normally (not as an extremely dark tile) in this chunk
        public float[] itemSpawnProb;//Probability of different items spawning at this distance fromt the origin
        public float totalItemSpawnProb;//The total of all the item spawn probabilities
    }

    /*REMOVED THIS FUNCTION, because I replaced it with the ChunkGenerating one
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
    }*/
}
