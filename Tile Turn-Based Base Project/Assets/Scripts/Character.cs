﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour {

    public int[] curStatArr;
    protected string cName;
    public int totalHealth;
    public int currentHealth;

    public int player;

    private bool canMove = true;
    private bool canAttack = true;

    public GameObject occupiedTile;

    // Sprite Rendering
    private SpriteRenderer myRenderer;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    [SerializeField]
    private Text DamageTextPrefab;
    public Text damageText;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private AudioClip[] stepSounds;
    private AudioSource audioSource;

    public abstract void TakeDamage(int damage, int stat);
    public abstract void Ability();
    public abstract void DisplayStats();
    public abstract List<int[,]> GetAttackRange();

    // Movement Bounce Animation
    float totalStretch = 0.3f;
    float totalSquish = 0.3f;

    #region Initialization
    void Start() {
        myRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
        audioSource = GetComponent<AudioSource>();
        SetHPFull();
    }
    #endregion

    #region Getter and Setter
    public string GetName() {
        return cName;
    }

    public int GetHP() {
        return currentHealth;
    }

    public void SetHPFull() {
        currentHealth = totalHealth;
    }

    public int[] GetCopyStats() {
        int[] copy = new int[curStatArr.Length];
        for (int i = 0; i < curStatArr.Length; i++) {
            copy[i] = curStatArr[i];
        }
        return copy;
    }

    public void ModifyStats(int[] stats) {
        if (stats.Length == curStatArr.Length) {
            for (int i = 0; i < stats.Length; i++) {
                curStatArr[i] = stats[i];
            }
        }
    }

    public virtual int GetMight() {
        return curStatArr[0];
    }

    public int GetSpeed() {
        return curStatArr[1];
    }

    public bool GetCanMove() {
        return canMove;
    }

    public void SetCanMove(bool canMoveBool) {
        canMove = canMoveBool;
    }

    public bool GetCanAttack() {
        return canAttack;
    }

    public void SetCanAttack(bool canAttackBool) {
        canAttack = canAttackBool;

        if (canAttackBool == true) {
            GetComponent<Renderer>().material.color = Color.white;
        }
        else if (canAttackBool == false) {
            GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    public void SetPlayer(int playerNumber) {
        player = playerNumber;
        //anim.SetInteger("player", playerNumber);
    }

    public int GetPlayer() {
        return player;
    }

    public void RecalculateDepth() {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
    }

    public GameObject GetOccupiedTile() {
        return occupiedTile;
    }

    public void SetOccupiedTile(GameObject tile) {
        occupiedTile = tile;
    }
    #endregion

    #region Sprite
    void WhiteSprite() {
        myRenderer.material.shader = shaderGUItext;
        myRenderer.color = Color.white;
    }

    void NormalSprite() {
        myRenderer.material.shader = shaderSpritesDefault;
        myRenderer.color = Color.white;
    }
    #endregion

    #region Animation
    IEnumerator HurtAnimation(int damage) {
        // Go white
        WhiteSprite();

        //Create Damage Text
        print("damage text created");
        damageText = Instantiate(DamageTextPrefab);
        Vector3 textPositionOffset = new Vector3(0, 1.25f, 0);
        damageText.transform.position = Camera.main.WorldToScreenPoint(transform.position + textPositionOffset);
        //damageText.GetComponent<DamageTextBehavior>().SetDamage(damage);

        // Shaking
        Vector3 defaultPosition = transform.position;
        System.Random r = new System.Random();
        for (int i = 0; i < 5; i++) {
            double horizontalOffset = r.NextDouble() * 0.2 - 0.1f;
            Vector3 vectorOffset = new Vector3((float)horizontalOffset, 0, 0);
            transform.position += vectorOffset;
            yield return new WaitForSeconds(0.025f);
            transform.position = defaultPosition;
        }

        // Go normal
        NormalSprite();
    }

    IEnumerator DeathAnimation() {
        // loop over 0.5 second backwards
        print("death time");
        for (float i = 0.25f; i >= 0; i -= Time.deltaTime) {
            // set color with i as alpha
            myRenderer.color = new Color(1, 1, 1, i);
            transform.localScale = new Vector3(1.5f - i, 1.5f - i, 1);
            yield return null;
        }

        myRenderer.color = new Color(1, 1, 1, 1);
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }

    public void StartBounceAnimation() {
        StartCoroutine("BounceAnimation");
    }

    IEnumerator BounceAnimation() {
        int frames = 3;
        //Vector3 originalPosition = transform.position;
        float stretch = totalStretch;
        float squish = totalSquish;
        for (int i = frames; i > 0; i--) {
            transform.localScale = new Vector3(1 + stretch, 1 - squish, 1);
            yield return new WaitForSeconds(0.01f);
            stretch /= 2.5f;
            squish /= 2.5f;
        }
        transform.localScale = new Vector3(1, 1, 1);

        // Play random step sound
        System.Random r = new System.Random();
        int stepNum = r.Next(0, stepSounds.Length);
        //audioSource.clip = stepSounds[stepNum];
        //audioSource.Play();
    }
    #endregion

    public void SetAnimVar() {
        if (player == 1) {
            //anim.SetInteger("player", 1);
        }
        else if (player == 2) {
            //anim.SetInteger("player", 2);
        }
    }

    #region Stats
    public void ResetStats() {
        currentHealth = totalHealth;
    }

    public void HPDamage(int damage) {
        currentHealth -= damage;
        if (currentHealth > 0) {
            StartCoroutine("HurtAnimation", damage);
        }
        else {
            StartCoroutine("DeathAnimation");
        }
    }
    #endregion
}
