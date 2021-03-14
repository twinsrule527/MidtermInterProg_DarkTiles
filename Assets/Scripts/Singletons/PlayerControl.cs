using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//A script for all of the player's possible controls: Movement, pickup, drop, and use items.
public class PlayerControl : Singleton<PlayerControl>
{
    [SerializeField]
    private Vector3 StartPosition;
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
    public const float TIMEFORACTION = 0.5f;//How long any given action takes to perform - same for everything
    public const int SLOW_DARK_LEVEL = 10;//The level of darkness a tile needs to be for it to be hard for you to stand on it

    //A set of possible states the player can be in
    public PlayerState stateMoving = new PlayerStateMoving();
    public PlayerState stateTakeAction = new PlayerStateTakeAction();
    public PlayerState stateUsing = new PlayerStateUse();
    public PlayerState stateDropping = new PlayerStateDrop();
    public PlayerState statePickingUp = new PlayerStatePickup();
    public PlayerState statePassTurn = new PlayerStatePassTurn();
    public PlayerState stateRefueling = new PlayerStateRefueling();
    public PlayerState stateNoAction = new PlayerStateNoAction();
    public PlayerState stateEndGame = new PlayerStateEndGame();

    private PlayerState _currentState;//Whatever state the player is currently in - backup variable
    public PlayerState CurrentState {//For encapsulation, has a public property for its state - only can be gotten, will only be changed via ChangeState() script
        get {
            return _currentState;
        }
    }

    private int _turns;//How many turns the game has lasted
    public int Turns {
        get {
            return _turns;
        }
    }
    //Has a few UI elements that it keeps track of
    public ActionBar myActionBar;//Keeps track of the Action bar, so it can call its functions
    [SerializeField]
    private Text playerPosText;
    void Start()
    {
        //Starts in EndGame state to stop null responses
        _currentState = stateEndGame;
        //This is all moved to the STARTGAME() function, so that the game can be restarted
        /*
        //Camera's position is set to your position + your offset:
        Camera.main.transform.position = transform.position + CameraOffset;

        Inventory = new Stack<Item>();
        Inventory.Push(GetComponentInChildren<NullItem>());
        maxActions = ACTIONS_UNSTIMULATED;
        _actions = maxActions;

        //Player starts in TakeActionState
        ChangeState(stateTakeAction);
        string playerPos = "Position: \n" +
                            "X: 0 \n" +
                            "Y: 0";
                            
        playerPosText.text = playerPos;
        */
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
            //The Action bar moves down
            StartCoroutine(myActionBar.RefreshActionUI(_actions, _actions + PICKUPACTIONS, TIMEFORACTION));
            
            ChangeState(statePickingUp);
        }
        else {
            //Otherwise, it changes to the quick noActionState, where the action bar flashes red
            ChangeState(stateNoAction);
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
            //The Action bar moves down
            StartCoroutine(myActionBar.RefreshActionUI(_actions, _actions + DROPACTIONS, TIMEFORACTION));
            
            ChangeState(stateDropping);
        }
        else {
            //Otherwise, it changes to the quick noActionState, where the action bar flashes red
            ChangeState(stateNoAction);
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
            //The Action bar moves down
            StartCoroutine(myActionBar.RefreshActionUI(_actions, _actions + USEACTIONS, TIMEFORACTION));
            
            ChangeState(stateUsing);
        }
        else {
             //Otherwise, it changes to the quick noActionState, where the action bar flashes red
            ChangeState(stateNoAction);
        }
    }


    public Vector3 movementVector;//Used as a placeholder, which is called by the PlayerStateMoving when you move into its state
    //Function that allows the player to move in the direction they choose
        //Moving is the only action that takes half the normal time
    public void Move(Vector2Int direction) {
        //Gets the position you're trying to move to, to see if anything would stop you
        Vector2Int moveToPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)) + direction;
        int moveCost = 0;
        if(CanMoveTo(TileManager.Instance.TileDictionary[moveToPos], out moveCost)) {
            //If you're able to move to the given position, you do move
            movementVector = new Vector3(direction.x, direction.y, 0f);
            int actionsTemp = _actions;//Needs a temp variable to check how many actions it has before changes are made
            //Lose actions equal to the movecost, but clamped at 0
            _actions = Mathf.Clamp(_actions -= moveCost, 0, maxActions);
            //If you have a free move action because you're carrying a berry, this action costs 1 less to do
            if(freeActionMove) {
                freeActionMove = false;
                _actions++;
            }
            //Player's position as shown on UI is updated
            RefreshPlayerPosText(transform.position + movementVector);
            //The Action bar moves down
            StartCoroutine(myActionBar.RefreshActionUI(_actions, actionsTemp, TIMEFORACTION / 2f));
            //Whenever you move, you need to see what Chunks are visible - If any do not exist yet, create said chunk
            TileManager.Instance.CheckChunkExists(transform.position, movementVector);
            //Change states to the movement state
            ChangeState(stateMoving);
        }
        else {
             //Otherwise, it changes to the quick noActionState, where the action bar flashes red
            ChangeState(stateNoAction);
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
        //2: high dark level, but not too high: Movement cose extra
        else if(posTile.darkLevel + posTile.darkModifier >= SLOW_DARK_LEVEL) {
            cost = MOVEACTIONS * 2;
            return true;
        }
        //3: A briar patch, takes 2 movement (or all the player's movement if they only have 1 movement left)
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
        //4: No underlying conditions, cost is normal move cost
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
        //When you change state, the game also checks the Top Item in the Inventory, in case it needs to be changed:
        TileManager.Instance.RefreshTopInventory();
    }

    //Function called by the PassTurn state when the player's turn starts, letting the PlayerControl know it has to reset variables
    public void StartTurn() {
        
        //If being stimulated by a berry, the player's number of actions are doubled
        if(turnsStimulatedRemaining > 0) {
            turnsStimulatedRemaining--;
            maxActions = ACTIONS_STIMULATED;
            //Makes sure the player has the right ActionBar
            myActionBar.CheckActionBar(true);
        }
        else {
            maxActions = ACTIONS_UNSTIMULATED;
            myActionBar.CheckActionBar(false);
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
        //Action bar is refreshes with actions
        StartCoroutine(myActionBar.RefreshActionUI(maxActions, _actions, TIMEFORACTION));
        //Actions are refreshed at beginning of turn
        _actions = maxActions;
        //Turn counter increases by 1
        _turns++;
            
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

    //Changes the position recorded in the positionText UI element
    private void RefreshPlayerPosText(Vector3 pos) {
        Vector2Int playerPos2Int = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        string myText = "Position: \n" +
                        "X: " + playerPos2Int.x.ToString() + "\n" +
                        "Y: " + playerPos2Int.y.ToString();
        playerPosText.text = myText;
    }

    //This function runs when you press the button to begin the game (everything in it is stuff that used to be in start, plus some refreshing of info)
    public void StartGame() {
        transform.position = StartPosition;
        //Camera's position is set to your position + your offset:
        Camera.main.transform.position = transform.position + CameraOffset;
        
        //Inventory is reset
        Inventory = new Stack<Item>();
        Inventory.Push(GetComponentInChildren<NullItem>());
        maxActions = ACTIONS_UNSTIMULATED;
        _actions = maxActions;
        turnsStimulatedRemaining = 0;//Resets whether the player has super-long action turns
        //Player starts in TakeActionState
        ChangeState(stateTakeAction);
        string playerPos = "Position: \n" +
                            "X: 0 \n" +
                            "Y: 0";
                            
        playerPosText.text = playerPos;
        //Number of turns that have passed is reset
        _turns = 0;
        //Destroy all upkeep items, then reset the list
        foreach(Item i in UpkeepItems) {
            Destroy(i);
        }
        UpkeepItems = new List<Item>();
        StartTurn();
        //Game begins at the beginning of your turn
        ChangeState(stateTakeAction);

    }

    //This function is called when the game ends, destroying everything in your inventory
    public void EndGame() {
        while(Inventory.Count > 1) {//Greater than 1 means it leaves the Null Item in
            //Pops the item from the inventory and then destroys it
            Destroy(Inventory.Pop());
        }
    }
}
