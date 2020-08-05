using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClass : Character
{
    //strength, speed, knowledge, will
    private int[] Stats = { 5, 5, 2, 4 };


    // Start is called before the first frame update
    void Awake() {
        totalHealth = 35;
        currentHealth = totalHealth;
        curStatArr = Stats;
        cName = "Test";
        themeName = "Test";
    }

    // Update is called once per frame
    void Update() {

    }

    public override void TakeDamage(int damage, int stat) {
        print(curStatArr.Length);
        curStatArr[stat] -= damage;
    }

    public override void Ability() {
        throw new System.NotImplementedException();
    }

    public override void DisplayStats() {
        menuName.text = "Test";

        //open menu for character, display stats, etc.
    }

    public override List<int[,]> GetAttackRange() {
        int flipIfPlayer2 = 1;
        if (player == 2) {
            flipIfPlayer2 = -1;
        }

        List<int[,]> attackRanges = new List<int[,]>();

        int[,] forwardRange = {
            {1 * flipIfPlayer2, 0 },
        };

        int[,] aboveRange = {
            {0, 1},
        };

        int[,] belowRange = {
            {0, -1},
        };

        attackRanges.Add(forwardRange);
        attackRanges.Add(aboveRange);
        attackRanges.Add(belowRange);

        return attackRanges;
    }
}
