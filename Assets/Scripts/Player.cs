using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Text goldDisplay;

    public Text healthDisplay;

    public int initialGold;

    public int maxHealth;

    int health;
    
    int gold;

    Board board;

    void Start()
    {
        board = FindObjectOfType<Board>();

        gold = initialGold;

        health = maxHealth;

        DisplayGold();

        DisplayHealth();

        board.OnRoundEndedInt += SetGold;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
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

    public void AddGold(int value)
    {
        gold += value;

        DisplayGold();
    }

    public void SetGold(int value)
    {
        gold = value;

        DisplayGold();
    }

    void DisplayGold()
    {
        goldDisplay.text = gold.ToString();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        DisplayHealth();

        if (health <= 0)
        {
            board.FinishGame(2);
        }
    }

    void DisplayHealth()
    {
        healthDisplay.text = "HP " + health;
    }
}
