using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//A script for all of the player's possible controls: Movement, pickup, drop, and use items.
public class PlayerControl : Singleton<PlayerControl>
{
    private Stack<Item> Inventory;//The player's inventory is a stack of Items
    public Item InventoryPeek {//This allows any script that wants to to Peek at the top of the InventoryStack
        get {
            return Inventory.Peek();
        }
    }
    public List<Item> UpkeepItems = new List<Item>();//A list of items with an upkeep value - Used by the PassTurnState
    
    [SerializeField]
    private Vector3 _cameraOffset;//However much the camera should be offset from the player such that the player looks like they're at the center (because of UI)
            //Needs to have a -10 value in the z, or the camera will be directly on top the player
    public Vector3 CameraOffset {//This property is needed because the Movement State needs to be able to move the camera
        get {
            return _cameraOffset;
        }
    }
    private int _actions;//How many actions the player has left - backup variable, but also the one that will be set when used by this script
    //Actions has a getter/setter, because the StateMachine wants to access it - but it can only be set in the PlayerControl
    public int Actions {
        get {
            return _actions;
        }
    }
    [SerializeField]
    private int maxActions;//How many possible actions the player can have - can be increased by certain items
    //This can also be accessed through a getter by other functions (mainly states)
    public int MaxActions {
        get {
            return maxActions;
        }
    }
    private bool freeActionMove;//Whether or not the player has a free Move action available - granted by Holding the Berry
    public int turnsStimulatedRemaining;//How much longer the player's actions should be doubled due to a Berry (if < 0, they are no longer doubled)
    private const int ACTIONS_UNSTIMULATED = 3;//How many actions the player should have if not stmulated
    private const int ACTIONS_STIMULATED = 6;//How many actions the player should have if stimulated

    public const int PICKUPACTIONS = 1;//How many actions picking up an item takes (below is the same for dropping, using, and moving)
    public const int DROPACTIONS = 1;
    public const int USEACTIONS = 2;
    public const int MOVEACTIONS = 1;
    public const float TIMEFORACTION = 0.25f;//How long any given action takes to perform - same for everything

    //A set of possible states the player can be in
    public PlayerState stateMoving = new PlayerStateMoving();
    public PlayerState stateTakeAction = new PlayerStateTakeAction();
    public PlayerState stateUsing = new PlayerStateUse();
    public PlayerState stateDropping = new PlayerStateDrop();
    public PlayerState statePickingUp = new PlayerStatePickup();
    public PlayerState statePassTurn = new PlayerStatePassTurn();
    public PlayerState stateRefueling = new PlayerStateRefueling();

    private PlayerState _currentState;//Whatever state the player is currently in - backup variable
    public PlayerState CurrentState {//For encapsulation, has a public property for its state - only can be gotten, will only be changed via ChangeState() script
        get {
            return _currentState;
        }
    }
    void Start()
    {
        //Camera's position is set to your position + your offset:
        Camera.main.transform.position = transform.position + CameraOffset;

        Inventory = new Stack<Item>();
        Inventory.Push(GetComponentInChildren<NullItem>());
        _actions = maxActions;

        //Player starts in TakeActionState
        ChangeState(stateTakeAction);
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
        TileTraits currentTile = TileManager.Instance.TileDictionary[tempVector];
        //Get the item on the Tile
        Item itemOnGround = currentTile.placedItem;
        if(itemOnGround != null && itemOnGround.pickupable) {
            //If the item exists and can be picked up, the player picks it up
            Inventory.Push(itemOnGround);
            currentTile.placedItem = null;
            TileManager.Instance.TileDictionary[tempVector] = currentTile;
            itemOnGround.Pickup();
            _actions-= PICKUPACTIONS;
            ChangeState(statePickingUp);
        }
    }

    //Function that drops the top item in your inventory when called
    public void DropItem() {
        //Gets the Tile that the player is standing on
        Vector2Int tempVector = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        TileTraits currentTile = TileManager.Instance.TileDictionary[tempVector];
        //Only if there is no item on the ground there does anything happen
        if(currentTile.placedItem == null && Inventory.Peek().Type != ItemType.Null) {
            //Drops the topmost item in your inventory
            Item droppedItem = Inventory.Pop();
            currentTile.placedItem = droppedItem;
            TileManager.Instance.TileDictionary[tempVector] = currentTile;
            droppedItem.Drop();
            _actions -= DROPACTIONS;
            ChangeState(stateDropping);
        }
    }

    public Item UsedItem;//A reference variable for the currently variable being used for the State Machine

    //Function that uses the player's currently held item, expending it
    public void UseItem() {
        UsedItem = Inventory.Peek();
        //Only if the item is able to be used will it pop off and be destroyed (mainly matters for Candle, which cannot be placed on top of another item)
        if(UsedItem.Usable()) {
            Inventory.Pop();
            UsedItem.Use(0);
            _actions -= USEACTIONS;
            ChangeState(stateUsing);
        }
    }


    public Vector3 movementVector;//Used as a placeholder, which is called by the PlayerStateMoving when you move into its state
    //Function that allows the player to move in the direction they choose
    public void Move(Vector2Int direction) {
        //Gets the position you're trying to move to, to see if anything would stop you
        Vector2Int moveToPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)) + direction;
        int moveCost = 0;
        if(CanMoveTo(TileManager.Instance.TileDictionary[moveToPos], out moveCost)) {
            //If you're able to move to the given position, you do move
            movementVector = new Vector3(direction.x, direction.y, 0f);
            //Lose actions equal to the movecost, but clamped at 0
            _actions = Mathf.Clamp(_actions -= moveCost, 0, maxActions);
            //If you have a free move action because you're carrying a berry, this action costs 1 less to do
            if(freeActionMove) {
                freeActionMove = false;
                _actions++;
            }
            //Whenever you move, you need to see what Chunks are visible - If any do not exist yet, create said chunk
            TileManager.Instance.CheckChunkExists(transform.position, movementVector);
            //Change states to the movement state
            ChangeState(stateMoving);
        }
    }

    //This bool function checks to see if the given tile is one that it is legal to move to
    private bool CanMoveTo(TileTraits posTile, out int cost) {
        //Three possible options:
        //1: Too high a darkness level, can't move there
        if(posTile.darkLevel + posTile.darkModifier >= TileManager.MAXDARKLEVEL) {
            cost = 0;
            return false;
        }
        //2: A briar patch, takes 2 movement (or all the player's movement if they only have 1 movement left)
            //Also, only occurs if the player is not carrying a hatchet
        else if(posTile.placedItem != null) {
            if(posTile.placedItem.Type == ItemType.Briar) {
                //If the player is not holding an axe, it takes 2 movement
                if(Inventory.Count == 0 || Inventory.Peek().Type != ItemType.Axe) {
                    cost = MOVEACTIONS * 2;
                    return true;
                }
            }
        }
        //3: No underlying conditions, cost is normal move cost
        cost = MOVEACTIONS;
        return true;
    }

    //Change State Function for the StateMachine:
    public void ChangeState(PlayerState newState) {
        if(CurrentState != null) {
            _currentState.Leave();
        }
        _currentState = newState;
        _currentState.Enter();
    }

    //Function called by the PassTurn state when the player's turn starts, letting the PlayerControl know it has to reset variables
    public void StartTurn() {
        
        //If being stimulated by a berry, the player's number of actions are doubled
        if(turnsStimulatedRemaining > 0) {
            turnsStimulatedRemaining--;
            maxActions = ACTIONS_STIMULATED;
        }
        else {
            maxActions = ACTIONS_UNSTIMULATED;
        }
        //If the player is holding a berry, they get one free action
        if(Inventory.Peek().Type == ItemType.Berry) {
            freeActionMove = true;//The first time you take a move action this turn, it doesn't cost anything
        }
        else {
            freeActionMove = false;
        }
        //Upkeep items are kept up with
        for(int i = 0; i< UpkeepItems.Count; i++) {
            UpkeepItems[i].Upkeep();
        }
        //Actions are refreshed at beginning of turn
        _actions = maxActions;
    }

    //This function pops the top item in your inventory and refuels the Lantern
        //Returns true if that all occurs
    public void Refuel( out bool fueled) {
        //If the top of the Inventory is Oil, it refuels
        if(Inventory.Peek().Type == ItemType.Oil) {
            Inventory.Pop();
            TileManager.LANTERN.Fuel += Lantern.FUEL_PER_OIL;
            fueled = true;
        }
        //Otherwise it lets you know it failed to refuel
        else {
            fueled = false;
        }
    }
}
