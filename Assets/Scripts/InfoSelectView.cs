using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//This script changes the Text on the Player's HighlightInfo Text Object
    //So that it highlights whatever the player is holding their mouse down on
public class InfoSelectView : MonoBehaviour
{
    [SerializeField]
    private Text InfoText;//The text object which will be changed

    private string baseText;//The basic text this object starts with and reverts to

    //The frame upon which the TopInventoryItem appears, so that it can be highlighted (this is a bit of a mess, but I didn't have time to do it in a better way)
    private readonly Vector3 INV_ITEM_START_POS = new Vector3(-8.5f, -5f, 0f);
    private readonly Vector3 INV_ITEM_END_POS = new Vector3(-6f, -3f, 0f);
    void Start()
    {
        baseText = InfoText.text;
    }

    
    void Update()
    {
        //If mouse is down, it checks which object its hovering on, and might change the Text
        if(Input.GetMouseButton(0)) {
            //Get the mouse position
            Vector3 myMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Checks to see if its going to view a Tile or part of the UI
            if(myMousePosition.x > PlayerControl.Instance.transform.position.x - (TileManager.SCREENRADIUSTILES -1f)) {
                Vector2Int posCheck = new Vector2Int(Mathf.FloorToInt(myMousePosition.x), Mathf.FloorToInt(myMousePosition.y));
                TileTraits SpotCheck = TileManager.Instance.TileDictionary[posCheck];
                string newText = "";
                //If the position has an item, it shows the item
                if(SpotCheck.placedItem != null) {
                    newText = SpotCheck.placedItem.NameText + "\n" +
                              SpotCheck.placedItem.AbilityText;
                }
                //Otherwise, it shows the Tile's traits
                else {
                    //Shows something different depending on whether it has a darkmodifier
                    if(SpotCheck.darkModifier == 0) {
                        newText = "TILE \n" +
                              "X: " + SpotCheck.position.x.ToString() + "\n" +
                              "Y: " + SpotCheck.position.y.ToString() + "\n" +
                              "Darkness: " + Mathf.FloorToInt(SpotCheck.darkLevel).ToString() + "/15";
                    }
                    else {
                        newText = "TILE \n" +
                              "X: " + SpotCheck.position.x.ToString() + "\n" +
                              "Y: " + SpotCheck.position.y.ToString() + "\n" +
                              "Darkness: (" + Mathf.FloorToInt(SpotCheck.darkLevel).ToString() + "+" + SpotCheck.darkModifier.ToString() +")/15";
                    }
                    if(SpotCheck.darkLevel + SpotCheck.darkModifier >= TileManager.MAXDARKLEVEL) {
                        newText = newText + "\nCan't be walked over!";
                    }
                    else if(SpotCheck.darkLevel + SpotCheck.darkModifier >= PlayerControl.SLOW_DARK_LEVEL) {
                        newText = newText + "\nHard to walk over!";
                    }
                }
                
                InfoText.text = newText;
            }
            //Otherwise, if its covering the TopInventory item, it follows its stats
            else {
                string newText = "";
                //The borders for the Inventory Item
                Vector3 lowerPos = PlayerControl.Instance.transform.position + INV_ITEM_START_POS;
                Vector3 higherPos = PlayerControl.Instance.transform.position + INV_ITEM_END_POS;
                if(myMousePosition.x > lowerPos.x && myMousePosition.x < higherPos.x && myMousePosition.y > lowerPos.y && myMousePosition.y < higherPos.y) {
                    Item tempItem = PlayerControl.Instance.InventoryPeek;
                    if(tempItem.Type != ItemType.Null) {
                        newText = "HELD: " + tempItem.NameText + "\n" +
                              tempItem.AbilityText;
                    }
                    else {
                        newText = baseText;
                    }
                }
                else {
                    newText = baseText;
                }
                InfoText.text = newText;
            }
        }
        else {
            InfoText.text = baseText;
        }
    }
}
