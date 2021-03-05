using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//The Briar bush is an object which appears on the ground.
    //It cannot be picked up, so other objects cannot be placed where it is
    //It cannot be held, so it also cannot be used or dropped.
    //Movement into a spot with a BriarBush is more difficult (takes 2 actions).
    //Can be destroyed by the Axe's Use Action. When this is done, a new positive item might appear where the BriarBush is.
public class ItemBriar : Item
{
    //probability Briar will not create an item when destroyed 0.4is the first element of the list below
    [SerializeField]
    private List<float> probItemSpawn;//A list of probabilities of items this can spawn - based off of the TileManager PossibleItems list
    private float totalProbSpawn;//The total of the probabilities in the list above
    public override void Awake()
    {
        base.Awake();
        pickupable = false;
        Type = ItemType.Briar;
        for(int i = 0; i < probItemSpawn.Count; i++) {
            totalProbSpawn += probItemSpawn[i];
        }
    }

    //When the Briar is destroyed, it has a chance of dropping an object
    public override void RemoveFromBoard(Vector2Int pos)
    {
        float rnd = Random.Range(0f, totalProbSpawn);
        float tempCount = 0;
        for(int i = 0; i < probItemSpawn.Count; i++) {
            tempCount += probItemSpawn[i];
            //If it's high enough, will generate the given item
            if(tempCount >= rnd) {
                Item createdItem = TileManager.Instance.PossibleItems[i];
                if(createdItem != null) {
                    //Creates the given item, but only if it exists
                    createdItem = Instantiate(createdItem, transform.position, Quaternion.identity);
                    //Adds the created item to the position
                    TileTraits tempTrait = TileManager.Instance.TileDictionary[pos];
                    tempTrait.placedItem = createdItem;
                    TileManager.Instance.TileDictionary[pos] = tempTrait;
                }
                //regardless, breaks after the tempCount overpasses the percent
                break;
            }
        }
    }

}
