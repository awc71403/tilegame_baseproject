using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class TileBehavior : MonoBehaviour, IPointerClickHandler {
    #region Selection Variables
    static List<GameObject> highlightedTiles = new List<GameObject>();
    public static GameObject selectedUnit;
    public static GameObject selectedTile;
    static string selectionState;
    #endregion

    #region UI Variables    
    public static float tileDim;
    public static Button attackButton;
    public static Button abilityButton;
    public static Button useRollButton;
    public static GameObject diceAttack;
    #endregion

    #region Instance Variables
    bool highlighted;
    GameObject myUnit;
    public int movementCost = 1;
    public int xPosition;
    public int yPosition;
    public string tileType;

    [SerializeField]
    GameObject tileHighlighter;
    Animator tileHighlighterAnimator;
    public float playerOpacity;
    public float enemyOpacity;

    float stepDuration = 0.1f;
    #endregion

    #region Initialization
    void Awake() {
        tileHighlighter.transform.position = transform.position;
        tileHighlighterAnimator = tileHighlighter.GetComponent<Animator>();
        setHighlightOpacity(playerOpacity);
    }
    #endregion

    #region Opacity
    private void setHighlightOpacity(float opacity) {
        Color c = tileHighlighter.GetComponent<Renderer>().material.color;
        c.a = opacity;
        tileHighlighter.GetComponent<Renderer>().material.color = c;
    }
    #endregion

    #region Unit Functions
    public void PlaceUnit(GameObject unit) {
        unit.GetComponent<Character>().SetAnimVar();
        myUnit = unit;
        myUnit.transform.position = transform.position - new Vector3(0, 0, 0);
        myUnit.GetComponent<Character>().RecalculateDepth();
        myUnit.GetComponent<Character>().SetOccupiedTile(gameObject);
    }

    public bool HasUnit() {
        return myUnit != null;
    }

    public GameObject GetUnit() {
        return myUnit;
    }

    public void ClearUnit() {
        myUnit = null;
    }
    #endregion

    #region Variable Functions
    public static string GetSelectionState() {
        return selectionState;
    }

    public static void SetSelectionState(string s) {
        selectionState = s;
    }

    public int GetXPosition() {
        return xPosition;
    }

    public int GetYPosition() {
        return yPosition;
    }

    public void SetSelectedTile(GameObject unit) {
        selectedTile = unit;
    }
    #endregion

    #region Highlight Functions
    public void HighlightCanMove(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", enemySelect);
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity);
        }
        else {
            setHighlightOpacity(playerOpacity);
        }
        highlighted = true;
    }

    public void HighlightCanMoveCovered(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", enemySelect);
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity / 2f);
        }
        else {
            setHighlightOpacity(playerOpacity / 2f);
        }
        highlighted = true;
    }

    public void HighlightCanAttack(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = true;
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity + 0.1f);
        }
        else {
            setHighlightOpacity(playerOpacity + 0.1f);
        }
    }

    public void HighlightCanAttackEmpty(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        setHighlightOpacity(playerOpacity / 2f);
        highlighted = true;
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity / 2f);
        }
        else {
            setHighlightOpacity(playerOpacity / 2f);
        }
    }

    public void HighlightCanSpawn() {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = true;
        setHighlightOpacity(playerOpacity);
    }

    public void HighlightSelected() {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", true);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        setHighlightOpacity(playerOpacity);
    }

    public void Dehighlight() {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = false;
        setHighlightOpacity(playerOpacity);
    }
    #endregion

    #region Highlight Valid Tiles Functions
    void HighlightMoveableTiles(int moveEnergy, bool enemySelect = false) {
        // Don't do anything if you've run out of energy.
        if (moveEnergy < 0 || tileType == "wall") {
            return;
        }

        //Otherwise, hightlight yourself...
        if (myUnit == null) {
            HighlightCanMove(enemySelect);
        }
        else if (!myUnit.Equals(selectedUnit)) {
            HighlightCanMoveCovered(enemySelect);
        }
        highlightedTiles.Add(gameObject);

        //...and all adjacent tiles (if they don't contain enemy units).

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);
            if (hit.collider != null) {
                TileBehavior otherTile = hit.transform.GetComponent<TileBehavior>();
                if (otherTile.myUnit == null || otherTile.myUnit.GetComponent<Character>().player == selectedUnit.GetComponent<Character>().player) {
                    hit.transform.GetComponent<TileBehavior>().HighlightMoveableTiles(moveEnergy - movementCost, enemySelect);
                }
            }
        }
    }

    void HighlightAttackableTiles(GameObject unit, bool enemySelect = false) {
        List<int[,]> attackRanges = unit.GetComponent<Character>().GetAttackRange();
        float tileSize = GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        foreach (int[,] attackRange in attackRanges) {
            for (int i = 0; i < attackRange.GetLength(0); i++) {
                Vector3 xOffSet = new Vector3(tileSize, 0.0f, 0.0f) * attackRange[i, 0];
                Vector3 yOffSet = new Vector3(0.0f, tileSize, 0.0f) * attackRange[i, 1];
                Vector2 tilePosition = transform.position + xOffSet + yOffSet;
                Collider2D hit = Physics2D.OverlapPoint(tilePosition);

                // If there exists a tile in that direction...
                if (hit != null) {
                    // Highlight that tile.
                    highlightedTiles.Add(hit.gameObject);

                    // If that tile has a unit...
                    if (hit.gameObject.GetComponent<TileBehavior>().HasUnit()) {
                        // And the unit belongs to the enemy team...
                        GameObject hitUnit = hit.gameObject.GetComponent<TileBehavior>().GetUnit();
                        if (hitUnit.GetComponent<Character>().GetPlayer() != selectedUnit.GetComponent<Character>().GetPlayer()) {
                            // Stop. Go no further in this direction.
                            hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttack(enemySelect);
                            break;

                        }
                        // And the unit belongs to the player team...
                        else {
                            // Keep going.
                            hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttackEmpty(enemySelect);
                        }
                    }
                    // If that tile is a wall...
                    else if (hit.gameObject.GetComponent<TileBehavior>().tileType == "wall") {
                        // Stop. Do not pass Go. Do not collect 200 dollars.
                        break;
                    }
                    else {
                        // Keep going.
                        hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttackEmpty(enemySelect);
                    }
                }
            }
        }



    }
    #endregion

    #region Selection Functions
    public void OnPointerClick(PointerEventData data) {
        //Condition where pointer click fails
        if (GameManager.actionInProcess) {
            return;
        }

        // If nothing is currently selected...
        if (selectionState == null) {
            // and this tile has a unit on it...
            if (myUnit != null) {
                // and the unit's player is equal to to the current player...
                if (GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToMove();
                }
                // ad the unit's player is equal to the enemy player...
                else {
                    SelectionStateToEnemySelect();
                }
            }

            // and this tile does not have a unit on it...
            else {
                // do nothing.
            }
        }
        // If something is currently selected...
        else {
            // and selection state is move...
            if (selectionState.Equals("move")) {
                // and the selected character can move onto this tile...
                if (highlighted && myUnit == null) {
                    // move that character onto this tile and dehighlight everything.
                    selectedTile.GetComponent<TileBehavior>().ClearUnit();
                    StartCoroutine(MoveUnitToThisTile(selectedUnit, selectedTile));
                }

                // and you are the selectedTile...
                else if (selectedTile.Equals(gameObject)) {
                    Deselect();
                    GameManager.GetSingleton().ClearUI();
                }

                // and the unit's player is equal to to the current player...
                else if (myUnit != null && GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToMove();
                }
                // and the unit's player is equal to the enemy player...
                else if (myUnit != null && !GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToEnemySelect();
                }
                // and the selected character cannot move onto this tile...
                else {
                    // Dehighlight everything.
                    SelectionStateToNull();
                }
            }
            // and selection state is attack...
            else if (selectionState.Equals("attack")) {
                // and the selected character can attack there...
                if (highlighted && myUnit != null && myUnit.GetComponent<Character>().GetPlayer() != selectedUnit.GetComponent<Character>().GetPlayer()) {
                    // (Attack), and deselect everything.

                    //ADD CODE FOR ATTACK

                    SelectionStateToNull();

                }

                // and the unit's player is equal to to the current player...
                else if (myUnit != null && GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToMove();
                }

                // and the unit's player is equal to the enemy player...
                else if (myUnit != null && !GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToEnemySelect();
                }

                // and the selected character cannot attack there...
                else {
                    // Dehighlight everything.
                    SelectionStateToNull();
                }
            }
            // and selection state is enemy select...
            else if (selectionState == "enemySelect" || selectionState == "enemySelectAttack") {
                // and this tile has a unit on it...
                if (myUnit != null) {
                    // and the unit's player is equal to to the current player...
                    if (GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                        // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                        SelectionStateToMove();
                    }

                    // and you are the selectedTile...
                    else if (selectedTile.Equals(gameObject)) {
                        Deselect();
                        //NEEDS EDIT
                        GameManager.GetSingleton().ClearUI();
                    }

                    // and the unit's player is equal to the enemy player...
                    else if (myUnit != null && !GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer())) {
                        // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                        SelectionStateToEnemySelect();
                    }
                }

                // and this tile does not have a unit on it...
                else {
                    // Dehighlight everything.
                    SelectionStateToNull();
                }
            }
        }
    }
    #endregion

    #region Selection State To Functions
    public void SelectionStateToNull() {
        // Deselect everything else
        Deselect();
    }

    public void SelectionStateToMove() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "move";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        // Highlight moveable tiles
        if (selectedUnit.GetComponent<Character>().GetCanMove()) {
            HighlightMoveableTiles(selectedUnit.GetComponent<Character>().GetSpeed());
        }
    }

    public void SelectionStateToAttack() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "attack";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        //Highlight attackable tiles
        selectedTile.transform.GetComponent<TileBehavior>().HighlightAttackableTiles(selectedUnit);
    }

    public void SelectionStateToEnemySelect() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "enemySelect";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        // Highlight moveable tiles
        if (selectedUnit.GetComponent<Character>().GetCanMove()) {
            HighlightMoveableTiles(selectedUnit.GetComponent<Character>().GetSpeed(), true);
        }
    }

    public void SelectionStateToEnemySelectAttack() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "enemySelectAttack";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        //Highlight attackable tiles
        selectedTile.transform.GetComponent<TileBehavior>().HighlightAttackableTiles(selectedUnit, true);
    }
    #endregion

    #region Deselect
    public static void Deselect() {
        // Dehighlight everything
        foreach (GameObject highlightedTile in highlightedTiles) {
            highlightedTile.transform.GetComponent<TileBehavior>().Dehighlight();
        }

        // Clear the list of highlighted tiles
        highlightedTiles.Clear();

        // Reset all selection variables
        selectedUnit = null;
        if (selectedTile != null) {
            selectedTile.GetComponent<TileBehavior>().Dehighlight();
        }
        selectedTile = null;
        selectionState = null;

        //Get rid of all the UI
        GameManager.GetSingleton().ClearUI();
    }
    #endregion

    #region Attack Functions
    public static void AttackSelection() {
        // If selection state is move...
        if (selectionState.Equals("move")) {
            // and there is a selected character...
            if (selectedUnit != null) {
                selectedTile.GetComponent<TileBehavior>().SelectionStateToAttack();
            }
        }
        // If selection state is attack...
        else if (selectionState.Equals("attack")) {
            // and there is a selected character...
            if (selectedUnit != null) {
                selectedTile.GetComponent<TileBehavior>().SelectionStateToMove();
            }
        }
        // If selection state is enemySelect...
        else if (selectionState.Equals("enemySelect")) {
            // and there is a selected character...
            if (selectedUnit != null) {
                selectedTile.GetComponent<TileBehavior>().SelectionStateToEnemySelectAttack();
            }
        }
        // If selection state is enemySelectAttack...
        else if (selectionState.Equals("enemySelectAttack")) {
            // and there is a selected character...
            if (selectedUnit != null) {
                selectedTile.GetComponent<TileBehavior>().SelectionStateToEnemySelect();
            }
        }
    }
    #endregion

    #region Movement Functions
    IEnumerator MoveUnitToThisTile(GameObject unit, GameObject originalTile) {
        // Action in process!
        GameManager.actionInProcess = true;

        // Deselect everything
        Deselect();
        float tileSize = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        tileDim = tileSize;

        // Calculate the steps you need to take
        int unitPlayer = unit.GetComponent<Character>().player;
        List<string> movementSteps = CalculateMovement(new List<string>(), originalTile, gameObject, unit.GetComponent<Character>().GetSpeed(), unitPlayer);

        //Take those steps!
        foreach (string step in movementSteps) {
            if (step.Equals("up")) {
                unit.transform.position += new Vector3(0, tileSize);
            }
            else if (step.Equals("right")) {
                unit.transform.position += new Vector3(tileSize, 0);
            }
            else if (step.Equals("down")) {
                unit.transform.position -= new Vector3(0, tileSize);
            }
            else if (step.Equals("left")) {
                unit.transform.position -= new Vector3(tileSize, 0);
            }
            unit.GetComponent<Character>().RecalculateDepth();
            unit.GetComponent<Character>().StartBounceAnimation();
            yield return new WaitForSeconds(stepDuration);
        }
        PlaceUnit(unit);
        unit.GetComponent<Character>().SetCanMove(false);

        // Action over!
        GameManager.actionInProcess = false;
    }

    // Recursive helper function to calculate the steps to take to get from tile A to tile B
    public static List<string> CalculateMovement(List<string> movement, GameObject currentTile, GameObject goalTile, int moveEnergy, int unitPlayer) {
        // If you're there, return the movement path.
        if (currentTile.Equals(goalTile)) {
            return movement;
        }

        // If you're out of energy, it's an invalid path.
        if (moveEnergy < 0) {
            return null;
        }

        List<List<string>> validPaths = new List<List<string>>();
        // Check for all adjacent tiles:
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(currentTile.transform.position, direction, 1.0f);
            if (hit.collider != null && hit.transform.GetComponent<TileBehavior>().tileType != "wall") {
                GameObject otherTileUnit = hit.transform.GetComponent<TileBehavior>().myUnit;
                if (otherTileUnit == null || otherTileUnit.GetComponent<Character>().player == unitPlayer) {
                    List<string> newMovement = new List<string>(movement.ToArray());
                    if (direction.Equals(Vector2.right)) {
                        newMovement.Add("right");
                    }
                    if (direction.Equals(Vector2.left)) {
                        newMovement.Add("left");
                    }
                    if (direction.Equals(Vector2.up)) {
                        newMovement.Add("up");
                    }
                    if (direction.Equals(Vector2.down)) {
                        newMovement.Add("down");
                    }
                    List<string> path = CalculateMovement(newMovement, hit.collider.gameObject, goalTile, moveEnergy - currentTile.GetComponent<TileBehavior>().movementCost, unitPlayer);
                    if (path != null) {
                        validPaths.Add(path);
                    }
                }
            }
        }

        // Return the shortest valid path
        if (validPaths.Count != 0) {
            List<string> shortestList = validPaths[0];
            foreach (List<string> path in validPaths) {
                if (path.Count < shortestList.Count) {
                    shortestList = path;
                }
            }
            return shortestList;
        }

        // If there are no valid paths from this point, return null
        return null;
    }
    #endregion
}
