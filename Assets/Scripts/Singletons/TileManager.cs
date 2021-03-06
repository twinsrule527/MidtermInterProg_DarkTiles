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
    public float futureDarkness;//How dark the Tile will be after the current action is over
    public Item placedItem;//If there is an item on the tile, this is the Item on the Tile
    public bool darkCause;//Whether this Tile creates darkness on nearby darkness when first generated (only referenced once)
    public int chunk;//Which chunk the Tile is in relative to the origin
}

//Made this a Singleton, because there are several things that need to reference it - It possibly could be abstract - Need to ask Prof
public class TileManager : Singleton<TileManager>
{
    //The TileManager spawns a Lantern at the beginning of the game
        //That lantern then becomes call-able by any script
    [SerializeField]
    private Lantern lanternPrefab;
    public static Lantern LANTERN;
    public readonly int NUMOFITEMS = 7;//How many different items there are
    public const int SCREENRADIUSTILES = 7;//How many away from the edge of the screen is the player in horizontal/vertical directions (including the player themself)
    
    [SerializeField][Header("0Null 1Briar 2Oil 3Candle 4Berry 5Axe 6Skull")]//This header is so that you can see what each element corresponds to in the 3 item lists
    private List<Item> possibleItems;//A list of all possible objects (0 = nothing, 1 = briar bush, 2 = oil, 3 = candle, 4 = berry, 5 = axe, 6 = skull)
    //Has a getter in case other objects want to access the prefab list (so it only has to appear once)
    public List<Item> PossibleItems {
        get {
            return possibleItems;
        }
    }
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
        //Instantiates the Lantern at (0, 0), which will change the light value of nearby objects
        LANTERN = Instantiate(lanternPrefab, new Vector3(0.5f, 0.5f, -1f), Quaternion.identity);
    }

    
    public const int CHUNKWIDTH = 25;//The width of a chunk is a constant 25 tiles (same for height)

    //Meant to replace the Grid generation code. Generates groups in chunks, such that each chunk is 25x25
        //The input pos is the Chunk position. Multiplying it by 25 gets the center of this new chunk
    public void GenerateChunk(Vector2Int pos) {
        //First, because I forgot to do this earlier, this chunk is added to the List of Chunks
        ExistingChunks.Add(pos, true);

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
        //Then for each tile, if its a dark enough tile, it darkens adjacent tiles
        for(int x = xStart; x < xStart + CHUNKWIDTH; x++) {
            for(int y = yStart; y < yStart + CHUNKWIDTH; y++) {
                //Get the current position
                Vector2Int tempVector = new Vector2Int(x, y);
                //If the chosen position is a DarkCause, it creates adjacent tiles on all possible adjacent ones
                if(TileDictionary[tempVector].darkCause) {
                    TileTraits[] tempTraits = GetAdjacency(tempVector);
                    for(int i = 1; i < tempTraits.Length; i++) {
                        //Only spreads to lower level darkness
                        if(!tempTraits[i].darkCause) {
                            tempTraits[i].darkLevel++;
                            TileDictionary[tempTraits[i].position] = tempTraits[i];
                        }
                    }
                }
            }
        }
        //finally, every tile is refreshed in case its sprite needs to change
        for(int x = xStart; x < xStart + CHUNKWIDTH; x++) {
            for(int y = yStart; y < yStart + CHUNKWIDTH; y++) {
                RefreshTile(new Vector2Int(x, y));
            }
        }
    }

    //A function that generates a single tile at the given position - can only be done throught the GenerateGrid code
    private void GenerateTile(Vector2Int pos, int chunkDist) {
        //If the chunk is fare enough away, it just uses the furthest chunk's distance (currently 8)
        chunkDist = Mathf.Clamp(chunkDist, 0, FURTHESTCHUNKS-1);
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
        tempTile.futureDarkness = darkTemp;
        tempTile.darkCause = false;
        tempTile.chunk = chunkDist;
        //If the tile is one of the super dark tiles, it is considered a cause of darkness
        if(darkTemp >=12) {
            tempTile.darkCause = true;
        }
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
                if(PossibleItems[i] == null) {}
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

    //THIS ISN"T QUITE WORKING RIGHT NOW
    //Gets the player position and the direction they're heading, to check to see if a new chunk needs to be generated
    public void CheckChunkExists(Vector3 pos, Vector3 direction) {
        Vector3 posToCheck = pos + direction * (SCREENRADIUSTILES) - new Vector3(0.5f, 0.5f, 0);//The last vector 3 is to make it centered on a tile
        //Turns the single check into 2 checks, one for each corner:
        Vector3 posToCheck1 = posToCheck;
        Vector3 posToCheck2 = posToCheck;
        //If the direction is in the y-direction, goes in both x-directions
        if(direction.x == 0) {
            posToCheck1 += new Vector3(1f, 0f, 0f) * SCREENRADIUSTILES;
            posToCheck2 -= new Vector3(1f, 0f, 0f) * SCREENRADIUSTILES;
        }
        else {//Otherwise, goes in both y-directions
            posToCheck1 += new Vector3(0f, 1f, 0f) * SCREENRADIUSTILES;
            posToCheck2 -= new Vector3(0f, 1f, 0f) * SCREENRADIUSTILES;
        }
        //Now, turns the position into a chunk (first, posToCheck1, then posToCheck2)
            //Shifting indeces to origin, so that when divided by 25, each chunk fits evenly
        int xChunk = Mathf.CeilToInt((posToCheck1.x - 12) / 25);
        int yChunk = Mathf.CeilToInt((posToCheck1.y - 12) / 25);
        Vector2Int chunkToCheck = new Vector2Int(xChunk, yChunk);
        //If the chunk does not exist, create the chunk
        if(!ExistingChunks.ContainsKey(chunkToCheck)) {
            GenerateChunk(chunkToCheck);
        }
        //Now for Pos2
        xChunk = Mathf.CeilToInt((posToCheck2.x - 12) / 25);
        yChunk = Mathf.CeilToInt((posToCheck2.y - 12) / 25);
        chunkToCheck = new Vector2Int(xChunk, yChunk);
        //If the chunk does not exist, create the chunk
        if(!ExistingChunks.ContainsKey(chunkToCheck)) {
            GenerateChunk(chunkToCheck);
        }

    }

    //Input a tile, and the TileManager checks its darkLevel to see if it needs to change sprites
    public void RefreshTile(Vector2Int pos) {
        TileTraits tempTraits = TileDictionary[pos];
        int darkTemp = Mathf.Clamp(Mathf.FloorToInt(tempTraits.darkLevel + tempTraits.darkModifier), 0, MAXDARKLEVEL);
        tempTraits.thisTile = DarknessLevelTile[darkTemp];
        myTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), DarknessLevelTile[darkTemp]);
        TileDictionary[pos] = tempTraits;
    }

    //This function is called at the end of each turn. Does the darkness spread that occurs at the end of every turn
    public void IncreaseDarknessEndTurnTiles(Vector3 playerPos) {
        //Get the bottom corner of the screen position
            //While most things Floor, this Ceilings, because the distance is actually 1 less than the screen radius, rather the actual screen radius
        Vector2Int startPos = new Vector2Int(Mathf.CeilToInt(playerPos.x - SCREENRADIUSTILES), Mathf.CeilToInt(playerPos.y - SCREENRADIUSTILES));
        //Reset all dark modifiers in the 13 x 13 grid, and then modifiers are reset by objects ontop/near them
        List<Item> ItemsOnBoard = new List<Item>();//This dictionary gets all items on the board with attributes that matter, so that dark/lightness can be affected
        //Runs through all correct objects in the TileDictionary
        TileTraits tempTraits;//Declared early, so that it only is declared once
        for(int x = startPos.x; x < startPos.x + (SCREENRADIUSTILES *2 - 1); x++) {
            for(int y = startPos.y; y < startPos.y + (SCREENRADIUSTILES *2 - 1); y++) {
                tempTraits = TileDictionary[new Vector2Int(x, y)];
                tempTraits.darkModifier = 0;
                //Future darkness is also set to be its current darkness
                tempTraits.futureDarkness = tempTraits.darkLevel;
                //if it has a placed item, and the item is a PLACED CANDLE or a Skull, its added to the list
                if(tempTraits.placedItem != null) {
                    if(tempTraits.placedItem.Type == ItemType.Skull) {
                        ItemsOnBoard.Add(tempTraits.placedItem);
                    }
                    else if(tempTraits.placedItem.Type == ItemType.Candle && tempTraits.placedItem.Placed()) {
                        //If it's a candle, and it has been placed on the ground through the Use option, it is added to the list
                        ItemsOnBoard.Add(tempTraits.placedItem);
                        
                    }
                }
                TileDictionary[tempTraits.position] = tempTraits;
            }
        }
        //Next, it runs through the List of items, changing dark modifiers as needed
        for(int i = 0; i < ItemsOnBoard.Count; i++) {
            Vector2Int tempPos = new Vector2Int(Mathf.FloorToInt(ItemsOnBoard[i].transform.position.x), Mathf.FloorToInt(ItemsOnBoard[i].transform.position.y));
            tempTraits = TileDictionary[tempPos];
            //For skull, increase the dark modifier of the tile it is on
            if(ItemsOnBoard[i].Type == ItemType.Skull) {
                tempTraits.darkModifier += ItemSkull.SELF_DARKNESS;
                TileDictionary[tempTraits.position] = tempTraits;
            }
            else if(ItemsOnBoard[i].Type == ItemType.Candle) {
                //Candle will run through its decreasing light script, which includes setting darkness
                //It falls under the candle's "Use" Function at an int value of 2
                ItemsOnBoard[i].Use(2);
            }
        }
        //Next, if the player is holding a Candle or a Skull, it changes the darkmodifier of adjacent tiles
        TileTraits[] playerPosArray = GetAdjacency(new Vector2Int(Mathf.FloorToInt(playerPos.x), Mathf.FloorToInt(playerPos.y)));
        if(PlayerControl.Instance.InventoryPeek.Type == ItemType.Skull) {
            //if holding a skull, increases adjacent tiles darkness by one
            for(int i = 1; i < playerPosArray.Length; i ++) {
                playerPosArray[i].darkModifier++;
                TileDictionary[playerPosArray[i].position] = playerPosArray[i];
            }
        }
        else if(PlayerControl.Instance.InventoryPeek.Type == ItemType.Candle) {
            //If holding a candle, decreases adjacent tile darkness by 1
            for(int i = 1; i < playerPosArray.Length; i ++) {
                playerPosArray[i].darkModifier--;
                TileDictionary[playerPosArray[i].position] = playerPosArray[i];
            }
        }

        //Goes through every tile, updating its darkness level, and then refreshing the tile
        for(int x = startPos.x; x < startPos.x + (SCREENRADIUSTILES *2 - 1); x++) {
            for(int y = startPos.y; y < startPos.y + (SCREENRADIUSTILES *2 - 1); y++) {
                //Gets the current tile
                //Updates the tile's darkness
                TileUpdateDarkness(new Vector2Int(x, y));
                    //Doesn't need to do the usual setting TileDictionary = tempTraits, bc the two Functions refresh everything needed
                //TileDictionary[tempTraits.position] = tempTraits;
            }
        }
        //Gets the playerPos as a 2DVector int so that the Tile the player is standing on doesn't change
        Vector2Int playerPos2Int = new Vector2Int(Mathf.FloorToInt(PlayerControl.Instance.transform.position.x), Mathf.FloorToInt(PlayerControl.Instance.transform.position.y));
        //Lastly, all Tiles have their future darkness become their current darkness, then they are refreshed
        for(int x = startPos.x; x < startPos.x + (SCREENRADIUSTILES *2 - 1); x++) {
            for(int y = startPos.y; y < startPos.y + (SCREENRADIUSTILES *2 - 1); y++) {
                //Sets the Tile to its new darkness level
                tempTraits = TileDictionary[new Vector2Int(x, y)];
                //Only changes the tile itself if it is not where the player is standing
                if(tempTraits.position != playerPos2Int) {
                    tempTraits.darkLevel = Mathf.Clamp(tempTraits.futureDarkness, tempTraits.darkLevel, MAXDARKLEVEL);//The new darkness is clamped by its current dark level and the max dark level
                    TileDictionary[tempTraits.position] = tempTraits;
                    //Then, refreshes its tile sprite
                    RefreshTile(tempTraits.position);
                }
            }
        }

    }


    public const int DARK_SPREAD_TIMES = 4;//How many times the darkness will spread if it is at a level of 10 or higher
    //Called as part of the end turn function - increases darkness depending on the tile's darkness level
        //Doesn't actually update the tile at the moment, just sets its future darkness to be updated
    public void TileUpdateDarkness(Vector2Int pos) {
        TileTraits tempTraits = TileDictionary[pos];
        float combinedDark = tempTraits.darkLevel + tempTraits.darkModifier;
        //At 10 or above, darkness spreads to adjacent tiles
        if(combinedDark >= 10) {
            TileTraits[] chooseTiles = GetAdjacency(tempTraits.position);
            //Increases 4 times, each at a power of 1/4 Sqrt(combinedDark - 9)
            for(int i = 0; i < DARK_SPREAD_TIMES; i++) {
                int rnd = Random.Range(1, chooseTiles.Length);
                chooseTiles[rnd].futureDarkness += (Mathf.Sqrt(combinedDark - 9) / DARK_SPREAD_TIMES);
            }
            //Add each tile gotten through the adjacency back to the TileDictionary
            for(int i = 0; i < chooseTiles.Length; i++) {
                TileDictionary[chooseTiles[i].position] = chooseTiles[i];
            }
        }
        //At 5 or above, darkness spreads to itself - doesn't at 10 or above - TESTING THIS
        else if(combinedDark >= 5) {
            //Spreads at a cubic root of the remainder of its current darkness +1 divided by 5
            tempTraits.futureDarkness += Mathf.Pow((combinedDark + 1) % 5, 1f / 3f );
        }
        //Then, the future darkness is changed depending on the position's relation to the origin:
        //Each chunk away from the origin subtracts a decreasing amount from their future darkness
        //This part depends on how well fueled the Lantern is - making it beneficial to keep the Lantern at a high fuel amount
        tempTraits.futureDarkness -= LANTERN.Fuel / (Lantern.FUEL_BASE * Mathf.Sqrt(1f + tempTraits.chunk));
        //If it is in the "0" chunk, there is an additional effect:
        if(tempTraits.chunk == 0) {
            //Small decrease in future darkness depending on position relative to Lantern
            tempTraits.futureDarkness -= 0.25f / (Mathf.Abs(tempTraits.position.x) + Mathf.Abs(tempTraits.position.y) + 0.5f);
        }
        TileDictionary[tempTraits.position] = tempTraits;
    }

    [SerializeField][Header("Tiles adjacent to chosen Tile")]
    private Vector2Int[] AdjacencyArray;//GIves the adjacency relation of all tiles adjacent to a given tile
    //This public function allows any object to get all squares adjacent to a specific spot
    public TileTraits[] GetAdjacency(Vector2Int pos) {
        //Gets the tiles - If they do not exist, you generate them
        TileTraits[] tempArray = new TileTraits[AdjacencyArray.Length];
        for(int i = 0; i < AdjacencyArray.Length; i++) {
            //For each tile, it first checks if it exists
            if(TileDictionary.ContainsKey(pos + AdjacencyArray[i])) {
                tempArray[i] = TileDictionary[pos + AdjacencyArray[i]];
            }
            //Otherwise, it generates the tile, but at and then adds it to the dictionary - but at the adjacent tile's chunk
            else {
                GenerateTile(pos + AdjacencyArray[i], TileDictionary[pos].chunk);
                tempArray[i] = TileDictionary[pos + AdjacencyArray[i]];
            }
        }
        return tempArray;
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
