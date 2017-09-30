using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------- CHANGE THIS NAME HERE -------
public class TEAM_JORTS_SQUAD : MonoBehaviour
{
    //---------- CHANGE THIS NAME HERE -------
    public static TEAM_JORTS_SQUAD AddYourselfTo(GameObject host)
    {
        //---------- CHANGE THIS NAME HERE -------
        return host.AddComponent<TEAM_JORTS_SQUAD>();
    }

    /*vvvv DO NOT MODIFY vvvvv*/
    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;

    private Transform character1Transform;
    private Transform character2Transform;
    private Transform character3Transform;

    private Dictionary<string, loadout> loadouts = new Dictionary<string, loadout>();
    private Dictionary<string, int> lastHealth = new Dictionary<string, int>();
    private Dictionary<string, int> deltaHealth = new Dictionary<string, int>();
    private static Dictionary<string, Transform> transforms = new Dictionary<string, Transform>();
    private Dictionary<string, LinkedList<Action>> charActions = new Dictionary<string, LinkedList<Action>>();

    private bool character1BeingShot = false;
    private bool character2BeingShot = false;
    private bool character3BeingShot = false;
    private float character1LastShot = -5;
    private float character2LastShot = -5;
    private float character3LastShot = -5;
    private float time = 0;

    private const int MOVE_ACTION = 0;
    private const int WAIT_ACTION = 1;

    private LinkedList<Action> character1Actions;
    private LinkedList<Action> character2Actions;
    private LinkedList<Action> character3Actions;

    private int lastHealth1;
    private int lastHealth2;
    private int lastHealth3;

    static private ObjectiveScript middleObjective;
    static private ObjectiveScript leftObjective;
    static private ObjectiveScript rightObjective;

    static private team ourTeamColor;

    private Action moveToLeft;
    private Action moveToMiddle;
    private Action moveToRight;

    void Start()
    {

        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();

        character1Transform = character1.getPrefabObject().transform;
        character2Transform = character2.getPrefabObject().transform;
        character3Transform = character3.getPrefabObject().transform;

        transforms[character1.name] = character1Transform;
        transforms[character2.name] = character2Transform;
        transforms[character3.name] = character3Transform;


        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        loadouts = new Dictionary<string, loadout>();
        lastHealth = new Dictionary<string, int>();
        deltaHealth = new Dictionary<string, int>();

        moveToLeft = new MoveAction (leftObjective.transform.position);
        moveToMiddle = new MoveAction (middleObjective.transform.position);
        moveToRight = new MoveAction (rightObjective.transform.position);

        ourTeamColor = character1.getTeam ();

        charActions[character1.name] = character1Actions = new LinkedList<Action>();
        charActions[character2.name] = character2Actions = new LinkedList<Action>();
        charActions[character3.name] = character3Actions = new LinkedList<Action>();

		if (Vector3.Distance (character1Transform.position, leftObjective.transform.position) < Vector3.Distance (character1Transform.position, rightObjective.transform.position)) {
			addAction (character1, moveToLeft);
			addAction (character2, moveToLeft);
			addAction (character3, moveToLeft);
            addAction(character1, new CapturePointAction(leftObjective));
            addAction(character2, new CapturePointAction(leftObjective));
            addAction(character3, new CapturePointAction(leftObjective));
        } else {
			addAction (character1, moveToRight);
			addAction (character2, moveToRight);
			addAction (character3, moveToRight);
            addAction(character1, new CapturePointAction(rightObjective));
            addAction(character2, new CapturePointAction(rightObjective));
            addAction(character3, new CapturePointAction(rightObjective));
        }
        

        addAction(character1, moveToMiddle);
        addAction(character2, moveToMiddle);
        addAction(character3, moveToMiddle);
        addAction(character1, new CapturePointAction(middleObjective));
        addAction(character2, new CapturePointAction(middleObjective));
        addAction(character3, new CapturePointAction(middleObjective));

        setLoadout(character1, loadout.SHORT);
        setLoadout(character2, loadout.SHORT);
        setLoadout(character3, loadout.SHORT);
    }

    /*^^^^ DO NOT MODIFY ^^^^*/

    /* Your code below this line */

    // Update() is called every frame
    void Update()
    {
        Debug.Log (getNumberOfObjectivesTheyHave () + " " + getNumberOfObjectivesWeHave ());
        determineHealthChange();
        isBeingShot();

        if (character1.getHP() > 0)
        {
            handleActions(character1, character1Actions);
        }
        if (deltaHealth[character1.name] == 100)
        {
            onSpawn(character1);
        }

        if (character2.getHP() > 0)
        {
            handleActions(character2, character2Actions);
        }
        if (deltaHealth[character2.name] == 100)
        {
            onSpawn(character2);
        }

        if (character3.getHP() > 0)
        {
            handleActions(character3, character3Actions);
        }
        if (deltaHealth[character3.name] == 100)
        {
            onSpawn(character3);
        }


        if (character1BeingShot)
        {
            fuckUpAttacker(character1);
        }
        else
        {
            spinOrLookAtEnemy(character1, character1Transform, 170f);
        }


        if (character2BeingShot)
        {
            fuckUpAttacker(character2);
        }
        else
        {
            spinOrLookAtEnemy(character2, character2Transform, 170f);
        }

        if (character3BeingShot)
        {
            fuckUpAttacker(character3);
        }
        else
        {
            spinOrLookAtEnemy(character3, character3Transform, 170f);
        }

    }

    void onSpawn(CharacterScript character)
    {
        addAction(character, moveToMiddle);
    }

    // Performs the first action in the action queue, dequeuing actions that are completed
    void handleActions(CharacterScript character, LinkedList<Action> actions)
    {
        pruneList(actions, character);
        if (actions.Count > 0)
        {
            if (actions.First.Value is MoveAction)
            {
                MoveAction move = (MoveAction)actions.First.Value;
                character.MoveChar(move.getLocation());
            }
            else if (actions.First.Value is WaitAction)
            {
                WaitAction wait = (WaitAction)actions.First.Value;
                if (wait.getStartWaitTime() == -1)
                {
                    wait.setStartWaitTime(Time.time);
                }
            }
            if (isActionDone(actions, character))
            {
                actions.RemoveFirst();
            }
        }
    }
	// RETURN SAME ACTIONS IF NO ITEM PRESENT
	LinkedList<Action> getObjectDetourActions(CharacterScript character) {

		LinkedList<Action> detourActions = new LinkedList<Action> ();

        LinkedList<Action> currentActions = charActions[character.name];

        GameObject targetItem = character.FindClosestItem();

        if (currentActions.First.Value is MoveToItemAction) {
            GameObject charPursuingItem = ((MoveToItemAction)currentActions.First.Value).getItem();
            if (charPursuingItem == targetItem)
            {
                return currentActions;
            }
        }
        foreach (Action action in currentActions) {
            detourActions.AddLast (action);
        }

        if (targetItem.transform.position != new Vector3 (0, 0, 0)) {
			MoveAction detourMoveAction = new MoveToItemAction (character.FindClosestItem ());
			detourActions.AddFirst (detourMoveAction);
		}

		return detourActions;
	}



    // Whether or not the current action in this action list is completed
    bool isActionDone(LinkedList<Action> actions, CharacterScript character)
    {
        return actions.First.Value.isComplete(character);
    }

    void addAction(CharacterScript character, Action action) {
        charActions[character.name].AddLast(action);
	}

	/*
	 * Puts the current action at the head of the LinkedList and then sets current action to the passed action.
	 * As a result, the character will go back to its previous action after finishing the current one.
	 * */
	void addPriorityAction(CharacterScript character, Action action) {
        bool waitType = (action is WaitAction);

        LinkedList<Action> actions = charActions[character.name];
        actions.AddFirst(action);
        if (waitType)
        {
            actions.AddFirst(new MoveAction(character1Transform.position));
        }
	}

    void determineHealthChange()
    {
        lastHealth1 = lastHealth.TryGetValue(character1.name, out lastHealth1) ? lastHealth1 : 100;
        deltaHealth[character1.name] = character1.getHP() - lastHealth1;

        lastHealth2 = lastHealth.TryGetValue(character2.name, out lastHealth2) ? lastHealth2 : 100;
        deltaHealth[character2.name] = character2.getHP() - lastHealth2;

        lastHealth3 = lastHealth.TryGetValue(character3.name, out lastHealth3) ? lastHealth3 : 100;
        deltaHealth[character3.name] = character3.getHP() - lastHealth3;

        lastHealth[character1.name] = character1.getHP();
        lastHealth[character2.name] = character2.getHP();
        lastHealth[character3.name] = character3.getHP();
    }

    // determine if characters are being shot
    void isBeingShot() {
        if (deltaHealth [character1.name] < 0)
        {
            character1LastShot = Time.time;
            character1BeingShot = true;
        }
        else if (Time.time - character1LastShot > 1)
        {
            character1BeingShot = false;
        }

        if (deltaHealth [character2.name] < 0)
        {
            character2LastShot = Time.time;
            character2BeingShot = true;
        }
        else if (Time.time - character2LastShot > 1)
        {
            character2BeingShot = false;
        }

        if (deltaHealth [character3.name] < 0)
        {
            character3LastShot = Time.time;
            character3BeingShot = true;
        }
        else if (Time.time - character3LastShot > 1)
        {
            character3BeingShot = false;
        }
    
    }

    // Whether or not this character was shot in this frame
    bool wasShot(CharacterScript character)
    {
        return deltaHealth[character.name] < 0;
    }

    void fuckUpAttacker(CharacterScript character)
    {
        Vector3 hitLoc = character.attackedFromLocations[character.attackedFromLocations.Count - 1];
        character.SetFacing(hitLoc);
        LinkedList<Action> actions = charActions[character.name];
        if (actions.Count == 0 || !(actions.First.Value is MoveToCoverAction))
        {
            addPriorityAction(character, new MoveToCoverAction(hitLoc));

            if (character1 != character)
            {
                addPriorityAction(character1, new MoveToAssistAction(hitLoc, 35));
            }

            if (character2 != character)
            {
                addPriorityAction(character2, new MoveToAssistAction(hitLoc, 35));
            }

            if (character3 != character)
            {
                addPriorityAction(character3, new MoveToAssistAction(hitLoc, 35));
            }
        }

    }

    // Has a character face its attacker and retreat into cover
    /*void shootAndCover(CharacterScript character)
    {
        character.SetFacing (character.attackedFromLocations [character.attackedFromLocations.Count - 1]);
        LinkedList<Action> actions = charActions[character.name];
        if (actions.Count == 0 || !(actions.First.Value is MoveToCoverAction))
        {
            flank(character, actions, character.attackedFromLocations[character.attackedFromLocations.Count - 1]);
            addPriorityAction(character, new MoveToCoverAction(character.FindClosestCover(character.attackedFromLocations[character.attackedFromLocations.Count - 1])));
        }
    }*/

    // Determines which loadout type a character was shot by during this frame, if they were shot
    // Returns null if none
    loadout? loadoutShotBy(CharacterScript character)
    {
        int loss = -deltaHealth[character.name];
        if (loss == 20)
        {
            return loadout.LONG;
        }
        else if (loss == 25)
        {
            return loadout.MEDIUM;
        }
        else if (loss == 35)
        {
            return loadout.SHORT;
        }
        return null;
    }

    // Has a character spin in a circle until it sees an enemy, in which case it looks at them
    void spinOrLookAtEnemy(CharacterScript character, Transform characterTransform, float angle)
    {
        if (character.visibleEnemyLocations.Count > 0)
        {
            character.SetFacing(character.visibleEnemyLocations[0]);
        }
        else
        {
            faceRelative(character, characterTransform, angle);
        }
    }

    // returns the number of objectives our team controls
    static int getNumberOfObjectivesWeHave() {
        int count = 0;
        if (leftObjective.getControllingTeam () == ourTeamColor) {
            count++;
        }
        if (middleObjective.getControllingTeam () == ourTeamColor) {
            count++;
        }
        if (rightObjective.getControllingTeam () == ourTeamColor) {
            count++;
        }
        return count;
    }

    // returns the number of objectives the other team controls
    static int getNumberOfObjectivesTheyHave() {
        int count = 0;
        team otherTeamColor = (ourTeamColor == team.blue) ? team.red : team.blue;
        if (leftObjective.getControllingTeam () == otherTeamColor) {
            count++;
        }
        if (middleObjective.getControllingTeam () == otherTeamColor) {
            count++;
        }
        if (rightObjective.getControllingTeam () == otherTeamColor) {
            count++;
        }
        return count;
    }


    // Faces a character in a direction relative to their current facing. Use 179 to spin fast.
    void faceRelative(CharacterScript character, Transform characterTransform, float angle)
    {
        character.SetFacing(characterTransform.position + Quaternion.AngleAxis(angle, Vector3.up) * characterTransform.forward);
    }

    // Use to set a character's loadout - allows us to internally track loadout assignments
    void setLoadout(CharacterScript character, loadout loadout)
    {
        character.setLoadout(loadout);
        loadouts[character.name] = loadout;
    }

    // Gets a character's damage based on their loadout. Does not take into account damage powerups
    int getDamage(CharacterScript character)
    {
        // Assume the default is the medium loadout
        if (!loadouts.ContainsKey(character.name))
        {
            return 25;
        }

        loadout load;
        loadouts.TryGetValue(character.name, out load);
        if (load == loadout.LONG) {
            return 20;
        }
        else if (load == loadout.MEDIUM)
        {
            return 25;
        }
        else
        {
            return 35;
        }
    }

    // Returns the range of our character
    int getRange(CharacterScript character)
    {
        // Assume the default is the medium loadout
        if (!loadouts.ContainsKey(character.name))
        {
            return 25;
        }

        loadout load;
        loadouts.TryGetValue(character.name, out load);
        if (load == loadout.LONG) {
            return 35;
        }
        else if (load == loadout.MEDIUM)
        {
            return 25;
        }
        else
        {
            return 15;
        }
    }

    void pruneList(LinkedList<Action> actions, CharacterScript character)
    {
        bool resetMove = false;
        var node = actions.First;
        while (node != null)
        {
            var nextNode = node.Next;
            if (node.Value.shouldPrune(character))
            {
                actions.Remove(node);
                if (node.Value is MoveAction)
                {
                    resetMove = true;
                }
            }
            node = nextNode;
        }

        if (resetMove)
        {
            actions.AddFirst(new MoveAction(transforms[character.name].position));
        }
    }

    // Returns the best (as of now, most time efficient) action list
    LinkedList<Action> compareActionLists(LinkedList<Action> option1, LinkedList<Action> option2, CharacterScript character)
    {
        float time1 = getActionsTime(option1, character);
        float time2 = getActionsTime(option2, character);

        return time1 < time2 ? option1 : option2;
    }

    // Gets the total number of seconds needed to complete the given set of actions
    float getActionsTime(LinkedList<Action> actions, CharacterScript character)
    {
        float totalTime = 0;
        bool first = true;
        Vector3 beginPos = character1Transform.position;
        foreach (Action action in actions)
        {
            float timeThisAction = action.timeToComplete(character, first, beginPos);
            if (action is MoveAction)
            {
                beginPos = ((MoveAction)action).getLocation();
            }
            totalTime += timeThisAction;

            first = false;
        }

        return totalTime;
    }

    abstract class Action
    {
        // Whether or not the action can be considered completed
        abstract public bool isComplete(CharacterScript character);

        // How many seconds left needed to finish this action, if active, or total time if not active
        abstract public float timeToComplete(CharacterScript character, bool isCurrentAction, Vector3 previousPos);

        // Whether or not this action is useless and should be pruned from the action list
        abstract public bool shouldPrune(CharacterScript character);

    }

    class MoveAction : Action
    {
        private Vector3 location;
		private bool isItemDest;

        public MoveAction(Vector3 _location)
        {
            location = _location;
			isItemDest = false;
        }

		public MoveAction(Vector3 _location, bool isItem) {

			location = _location;
			if(isItem) {
				isItemDest = true;
			}
		}

        // Whether or not the action can be considered completed
        public override bool isComplete(CharacterScript character)
        {
            return character.isDoneMoving(.5f);
        }

        public Vector3 getLocation()
        {
            return location;
        }

		public bool isItem() {
			return isItemDest;
		}

        // How many seconds left needed to finish this action, if active, or total time if not active
        public override float timeToComplete(CharacterScript character, bool isCurrentAction, Vector3 previousPos)
        {
            float distance = (location - previousPos).magnitude;
            return distance / getCharSpeed(character);
        }

        int getCharSpeed(CharacterScript character)
        {
            // TODO: Account for speed powerup
            return 10;
        }

        // Whether or not this action is useless and should be pruned from the action list
        public override bool shouldPrune(CharacterScript character)
        {
            return false;
        }
    }

    class MoveToCoverAction : MoveAction
    {
        public MoveToCoverAction(Vector3 _location) : base(_location) {}
    }

    class MoveToAssistAction : MoveAction
    {
        private float maxAssistDistance;

        public MoveToAssistAction(Vector3 _location, float _maxAssistDistance) : base(_location)
        {
            maxAssistDistance = _maxAssistDistance;
        }

        public override bool shouldPrune(CharacterScript character)
        {
            return (transforms[character.name].position - getLocation()).magnitude > maxAssistDistance;
        }
    }

    class MoveToItemAction : MoveAction
    {
        private GameObject item;

        public MoveToItemAction(GameObject _item) : base(_item.transform.position)
        {
            item = _item;
        }

        // Whether or not this action is useless and should be pruned from the action list
        public override bool shouldPrune(CharacterScript character)
        {
            return item == null;
        }

        public GameObject getItem()
        {
            return item;
        }
    }

    class WaitAction : Action
    {
        private float waitTime;
        private float startWaitTime;

        public WaitAction(float _waitTime)
        {
            waitTime = _waitTime;
            startWaitTime = -1;
        }

        public float getWaitTime()
        {
            return waitTime;
        }

        // Start wait time is the game time at which this wait action began
        // -1 until this wait action actually starts executing
        public float getStartWaitTime()
        {
            return startWaitTime;
        }

        public void setStartWaitTime(float _startWaitTime)
        {
            startWaitTime = _startWaitTime;
        }

        // Whether or not the action can be considered completed
        public override bool isComplete(CharacterScript character)
        {
            return Time.time - startWaitTime >= waitTime;
        }

        // How many seconds left needed to finish this action, if active, or total time if not active
        public override float timeToComplete(CharacterScript character, bool isCurrentAction, Vector3 previousPos)
        {
            if (isCurrentAction)
            {
                return waitTime - (Time.time - startWaitTime);
            }
            return waitTime;
        }

        // Whether or not this action is useless and should be pruned from the action list
        public override bool shouldPrune(CharacterScript character)
        {
            return false;
        }
    }

    class CapturePointAction : WaitAction
    {
        private ObjectiveScript objective;

        public CapturePointAction(ObjectiveScript _objective) : base(4)
        {
            objective = _objective;
        }

        // Whether or not this action is useless and should be pruned from the action list
        public override bool shouldPrune(CharacterScript character)
        {
            return objective.getControllingTeam() == character.getTeam();
        }
    }

    class WaitUntilLosingAction : Action
    {
        public override bool isComplete(CharacterScript character)
        {
            return getNumberOfObjectivesTheyHave() >= getNumberOfObjectivesWeHave();
        }

        public override float timeToComplete(CharacterScript character, bool isCurrentAction, Vector3 previousPos)
        {
            return 0;
        }

        public override bool shouldPrune(CharacterScript character)
        {
            return false;
        }
    }
}
