using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A script for all of the player's possible controls: Movement, pickup, drop, and use items.
public class PlayerControl : MonoBehaviour
{
    private Stack<Item> Inventory;//The player's inventory is a stack of Items
    [SerializeField]
    private Vector2 cameraOffset;//However much the camera should be offset from the player such that the player looks like they're at the center (because of UI)
    public TileManager myTileManager;//TODO: remove this, and all references to this if the TileManager becomes a Singleton
    private int _actions;//How many actions the player has left - backup variable, but also the one that will be set when used by this script
    //Actions has a getter/setter, because the StateMachine wants to access it - but it can only be set in the PlayerControl
    public int Actions {
        get {
            return _actions;
        }
    }
    [SerializeField]
    private int maxActions;//How many possible actions the player can have - can be increased by certain items

    public const int PICKUPACTIONS = 1;//How many actions picking up an item takes (below is the same for dropping, using, and moving)
    public const int DROPACTIONS = 1;
    public const int USEACTIONS = 2;
    public const int MOVEACTIONS = 1;

    //A set of possible states the player can be in
    private PlayerState stateMoving;
    private PlayerState stateTakeAction;
    
    private PlayerState _currentState;//Whatever state the player is currently in - backup variable
    public PlayerState CurrentState {//For encapsulation, has a public property for its state
        get {
            return _currentState;
        }
        set {
            _currentState = value;
        }
    }
    void Start()
    {
        //Camera's position is set to your position + your offset:
        Camera.main.transform.position = transform.position + new Vector3(cameraOffset.x, cameraOffset.y, -10f);

        Inventory = new Stack<Item>();
        _actions = maxActions;
        //Each of the player's State's are declared in start
        stateMoving = new PlayerStateMoving(this);
        stateTakeAction = new PlayerStateTakeAction(this);

        //Player starts in TakeActionState
        CurrentState = stateTakeAction;
    }

    
    void Update()
    {
        //Camera.main.transform.position = transform.position + new Vector3(cameraOffset.x, cameraOffset.y, -10f);
        CurrentState.Run();
    }

    //Function called when the player attempts to pick up an object
    public void PickUpItem() {
        //Gets the Tile that the player is currently standing on
        Vector2Int tempVector = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        TileTraits currentTile = myTileManager.TileDictionary[tempVector];
        //Get the item on the Tile
        Item itemOnGround = currentTile.placedItem;
        if(itemOnGround != null && itemOnGround.pickupable) {
            //If the item exists and can be picked up, the player picks it up
            Inventory.Push(itemOnGround);
            currentTile.placedItem = null;
            myTileManager.TileDictionary[tempVector] = currentTile;
            itemOnGround.Pickup();
            _actions-= PICKUPACTIONS;
        }
    }

    //Function that drops the top item in your inventory when called
    public void DropItem() {
        //Gets the Tile that the player is standing on
        Vector2Int tempVector = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        TileTraits currentTile = myTileManager.TileDictionary[tempVector];
        //Only if there is no item on the ground there does anything happen
        if(currentTile.placedItem == null && Inventory.Count > 0) {
            //Drops the topmost item in your inventory
            Item droppedItem = Inventory.Pop();
            currentTile.placedItem = droppedItem;
            myTileManager.TileDictionary[tempVector] = currentTile;
            droppedItem.Drop();
            _actions -= DROPACTIONS;
        }
    }

    //Function that uses the player's currently held item, expending it
    public void UseItem() {
        Item usedItem = Inventory.Peek();
        //Only if the item is able to be used will it pop off and be destroyed (mainly matters for Candle, which cannot be placed on top of another item)
        if(usedItem.Usable()) {
            Inventory.Pop();
            usedItem.Use();
            _actions -= USEACTIONS;
        }
    }

    //Function that allows the player to move in the direction they choose
    public void Move(Vector2Int direction) {
        //Gets the position you're trying to move to, to see if anything would stop you
        Vector2Int moveToPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)) + direction;
        int moveCost = 0;
        if(CanMoveTo(myTileManager.TileDictionary[moveToPos], out moveCost)) {
            //If you're able to move to the given position, you do move
            transform.position += new Vector3(direction.x, direction.y, 0f);
        }
    }

    //This bool function checks to see if the given tile is one that it is legal to move to
    private bool CanMoveTo(TileTraits posTile, out int cost) {
        //Three possible options:
        //1: Too high a darkness level, can't move there
        if(posTile.darkLevel == TileManager.MAXDARKLEVEL) {
            cost = 0;
            return false;
        }
        //2: A briar patch, takes 2 movement (or all the player's movement if they only have 1 movement left)
            //Also, only occurs if the player is not carrying a hatchet
        else if(posTile.placedItem.Type == ItemType.Briar && Inventory.Peek().Type != ItemType.Axe) {
            cost = MOVEACTIONS * 2;
            return true;
        }
        //3: No underlying conditions, cost is normal move cost
        cost = MOVEACTIONS;
        return true;
    }
}
