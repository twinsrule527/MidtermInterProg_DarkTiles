using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//The Lantern is an item at the origin (0, 0) that cannot be picked up.
    //The game ends when the lantern runs out of fuel. It can be refueled with the Oil Item.
    //The Lantern slows darkness spread in accordance with how much it has been fueled.
public class Lantern : Item
{
    private const int START_LIGHT_RADIUS = 4;//The starting radius at which the Lantern applies effects to when it is created
    private const float START_FUEL = 15;//How much fuel the Lantern starts with
    private const float FUEL_UPKEEP = 0.4f;//How much fuel the Lantern loses per upkeep step
    public const float FUEL_BASE = 60;//Inverse multiplier for the Lantern's effect on decreasing darkness
    public const float FUEL_PER_OIL = 5;//How much fuel each Oil used gives to the Lantern
    public float Fuel;//How much fuel is currently in the Lantern
    public Text LevelIndicator;//Text object that shows light level - Assigned by TileManager
    public Image DarkAura;//This image is a darkness which covers the entire screen, becoming more visible with a lower Lantern Level
    private const int BASE_FUEL = 15;//Base fuel is used for when the DarkAura appears
    private const float MAX_AURA_ALPHA = 0.3f;//How dark the Lantern's aura can get
    
    [SerializeField]
    private Sprite[] MySprite;//Has an array of possible sprites, for different Lantern Light Levels
    //On start, will lower the darkness value of nearby Tiles
    
    void Start() {
        base.Awake();
        NameText = "LANTERN";
        AbilityText = "Your home base. The more fuel it has, the slower the darkness spreads. Don't let it run out!";
        Fuel = START_FUEL;
        pickupable = false;
        Type = ItemType.Lantern;
        //The 9x9 square around the Lantern must have dark values maxed by their distance from the Lantern
            //Also, no skulls can spawn in that area
        int xStart = Mathf.FloorToInt(transform.position.x - START_LIGHT_RADIUS);
        int yStart = Mathf.FloorToInt(transform.position.y - START_LIGHT_RADIUS);
        //Set in advance for ease of access
        TileTraits tempTraits;
        for(int x = xStart; x < (xStart + START_LIGHT_RADIUS * 2 + 1); x ++) {
            for(int y = yStart; y < (yStart + START_LIGHT_RADIUS * 2 + 1); y ++) {
                tempTraits = TileManager.Instance.TileDictionary[new Vector2Int(x, y)];
                //the Tile's darkness is clamped by its taxi-cab distance from (0, 0)
                tempTraits.darkLevel = Mathf.Clamp(tempTraits.darkLevel, 0, Mathf.Abs(x) + Mathf.Abs(y));
                //If the Tile has a skull, destroy that skull
                if(tempTraits.placedItem != null && tempTraits.placedItem.Type == ItemType.Skull) {
                    Destroy(tempTraits.placedItem.gameObject);
                }
                //Set back into the TileDictionary
                TileManager.Instance.TileDictionary[tempTraits.position] = tempTraits;
                //Tile is refreshed
                TileManager.Instance.RefreshTile(tempTraits.position);
            }
        }
        //Next, the Tile the Lantern is on itself is set to 0, and other items there are removed:
        tempTraits = TileManager.Instance.TileDictionary[new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y))];
        if(tempTraits.placedItem != null) {
            Destroy(tempTraits.placedItem.gameObject);
        }
        //Added to the Tile
        tempTraits.placedItem = this;
        //Dark level set to 0
        tempTraits.darkLevel = 0;
        TileManager.Instance.TileDictionary[tempTraits.position] = tempTraits;
        //Tile is refreshed
        TileManager.Instance.RefreshTile(tempTraits.position);
        //its own light level is also refreshed
        RefreshLevel();
    }

    //if the game somehow glitches such that you end up carrying the Lantern, this stops you from using it
    public override bool Usable()
    {
        return false;
    }

    //Counts as a "Placed Item" for the sake of things that care about Dropped Items
    public override bool Placed()
    {
        return true;
    }
    
    //The lantern's upkeep is that it loses fuel, decreasing its overall power - when it reaches 0, the player loses
    public override void Upkeep()
    {
        Fuel -= FUEL_UPKEEP;
        //DarkAura is shown depending on your Lantern's light level
        if(Fuel < BASE_FUEL) {
            float alpha = Mathf.Lerp(0, MAX_AURA_ALPHA, BASE_FUEL - Fuel);
            DarkAura.color = new Color(DarkAura.color.r, DarkAura.color.g, DarkAura.color.b, alpha);
            //Sprite is set proportionally to the amount of light you have
            int levelProportion = Mathf.CeilToInt(Fuel / ((float)BASE_FUEL / (float)MySprite.Length));
            Debug.Log(levelProportion);
            mySpriteRenderer.sprite = MySprite[MySprite.Length - (levelProportion)];
        }
        else {
            DarkAura.color = new Color(DarkAura.color.r, DarkAura.color.g, DarkAura.color.b, 0);
            mySpriteRenderer.sprite = MySprite[0];
        }
        RefreshLevel();
        if(Fuel <= 0.5f) {
            //Game ends
            StartCoroutine(FadeToBlack());

        }
    }

    //Refreshes the Text that shows the light level
    public void RefreshLevel() {
        string levelText = "Lantern Light: \n" +
                           Mathf.FloorToInt(Fuel).ToString();
        LevelIndicator.text = levelText;
    }

    //This float plus the Enumerator below make the DarkAura fade to black when the game ends
    private const float FADE_TIME = 5f;
    private IEnumerator FadeToBlack() {
        float currentTime = 0;
        while(currentTime < FADE_TIME) {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(MAX_AURA_ALPHA, 1, currentTime / FADE_TIME);
            DarkAura.color = new Color(DarkAura.color.r, DarkAura.color.g, DarkAura.color.b, alpha);
            yield return null;
        }
        //Then, the ENDGAME occurs
        TileManager.Instance.EndGame();
        //The player enters their endgame state
        PlayerControl.Instance.ChangeState(PlayerControl.Instance.stateEndGame);
        yield return null;
    }
}
