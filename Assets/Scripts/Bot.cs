using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bot : MonoBehaviour
{
    public Text healthDisplay;

    public int initialGold;

    public int maxHealth;

    public GameObject knightPrefab;
    public GameObject archerPrefab;
    public GameObject healerPrefab;

    int health;

    int gold;

    Board board;

    void Start()
    {
        board = FindObjectOfType<Board>();

        gold = initialGold;

        health = maxHealth;

        DisplayHealth();

        board.OnRoundEnded += BuyRandomUnits;
        board.OnRoundEndedInt += SetGold;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            TakeDamage(maxHealth);
        }
    }

    public void BuyUnit(GameObject unit)
    {
        if (gold <= 0)
        {
            return;
        }

        board.AddUnit(unit);

        AddGold(-1);
    }

    public void BuyRandomUnits()
    {
        int randomNumber;

        while (gold > 0)
        {
            randomNumber = Random.Range(0, 3);

            if (randomNumber == 0)
            {
                BuyUnit(knightPrefab);
            }
            else if (randomNumber == 1)
            {
                BuyUnit(archerPrefab);
            }
            else
            {
                BuyUnit(healerPrefab);
            }
        }
    }

    public void AddGold(int value)
    {
        gold += value;
    }

    public void SetGold(int value)
    {
        gold = value;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        DisplayHealth();

        if (health <= 0)
        {
            board.FinishGame(1);
        }
    }

    void DisplayHealth()
    {
        healthDisplay.text = "HP " + health;
    }
}
