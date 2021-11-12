using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public Vector2 position;

    public enum enumType
    {
        knight, archer, healer
    }
    public enumType type;

    public enum enumTeam
    {
        allied, enemy
    }
    public enumTeam team;

    public float maxHealth;

    public float damage;

    public int range;

    public float moveSpeed;

    public float attackSpeed;

    float health;

    bool dead;

    public enum enumStatus
    {
        NONE, WAITING, READY, MOVING, ATTACKING
    }
    enumStatus status = enumStatus.NONE;
    enumStatus previousStatus = enumStatus.NONE;

    Animator animator;

    Slider healthBar;

    Board board;
    Player player;
    Bot bot;

    bool frameProtection = true;

    bool highlighted;
    bool dragging;

    Unit attackingUnit;

    float attackSpeedCounter;

    List<Vector2> tilesToCheck;

    void Start()
    {        
        board = FindObjectOfType<Board>();
        player = FindObjectOfType<Player>();
        bot = FindObjectOfType<Bot>();

        healthBar = GetComponentInChildren<Slider>();

        healthBar.maxValue = maxHealth;

        health = maxHealth;

        DisplayHealth();

        attackSpeedCounter = attackSpeed;

        animator = GetComponentInChildren<Animator>();

        animator.applyRootMotion = false;
    }

    void Update()
    {        
        if (status == enumStatus.MOVING)
        {
            animator.SetBool("Moving", true);
        }
        else if (!frameProtection)
        {
            animator.SetBool("Moving", false);
        }

        if (status != previousStatus)
        {
            frameProtection = false;
        }
        else
        {
            frameProtection = true;
        }

        previousStatus = status;

        if (!board.GetRoundStarted())
        {
            if (highlighted && Input.GetMouseButtonDown(0) && team == enumTeam.allied)
            {
                dragging = true;

                board.IsDragging(true, GetComponent<Unit>());
            }

            if (Input.GetMouseButtonUp(0) && dragging)
            {
                transform.localPosition = board.GetHighlightedTile().transform.localPosition;

                MoveUnit(new Vector2(transform.localPosition.x, transform.localPosition.z));
                
                dragging = false;

                board.IsDragging(false, null);
            }

            if (dragging)
            {
                Plane plane = new Plane(Vector3.up, Vector3.up * transform.position.y);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    transform.position = ray.GetPoint(distance);
                }

                transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, 0, 7), transform.localPosition.y, Mathf.Clamp(transform.localPosition.z, 0, 3));
            }

            return;
        }
        
        if (status == enumStatus.MOVING)
        {
            transform.Translate(new Vector3(0, 0, moveSpeed * Time.deltaTime));

            if (team == enumTeam.allied)
            {
                if (transform.localPosition.z > position.y)
                {
                    FinishMovement();
                }
            }
            else
            {
                if (transform.localPosition.z < position.y)
                {
                    FinishMovement();
                }
            }
        }
        else if (status == enumStatus.ATTACKING)
        {
            if (attackSpeedCounter >= attackSpeed)
            {
                animator.SetTrigger("Attacking");
                
                if (board.CheckEnemyUnit(attackingUnit.position, GetComponent<Unit>()))
                {
                    attackingUnit.TakeDamage(damage);
                }
                else
                {
                    attackingUnit.TakeDamage(-damage);
                }
                
                attackSpeedCounter = 0;
            }
            else
            {
                attackSpeedCounter += Time.deltaTime;
            }

            if (attackingUnit.IsDead())
            {
                attackingUnit = null;

                status = enumStatus.NONE;
            }
        }
        else
        {
            SetStatus();

            return;
        }

        if (dead)
        {
            Die();
        }
    }

    void FinishMovement()
    {        
        transform.localPosition = new Vector3(position.x, transform.localPosition.y, position.y);

        status = enumStatus.READY;
    }

    void SetStatus()
    {
        tilesToCheck = new List<Vector2>();

        if (team == enumTeam.allied)
        {
            if (position.y == 7f)
            {
                AttackOpponent();

                return;
            }
            
            for (int i = 1; i <= range && position.y + i <= 7; i++)
            {
                tilesToCheck.Add(position + new Vector2(0f, i));
            }
        }
        else
        {
            if (position.y == 0f)
            {
                AttackOpponent();

                return;
            }
            
            for (int i = 1; i <= range && position.y - i >= 0; i++)
            {
                tilesToCheck.Add(position - new Vector2(0f, i));
            }
        }

        // Check for enemies in range
        for (int i = 0; i < tilesToCheck.Count; i++)
        {                        
            if (!board.GetTileAvailability(tilesToCheck[i]))
            {
                if ((type == enumType.healer && !board.CheckEnemyUnit(tilesToCheck[i], GetComponent<Unit>())) || board.CheckEnemyUnit(tilesToCheck[i], GetComponent<Unit>()))
                {
                    attackingUnit = board.GetUnit(tilesToCheck[i]);

                    status = enumStatus.READY;

                    if (attackingUnit.CheckIfReady() || i < range - 1)
                    {
                        status = enumStatus.ATTACKING;
                    }

                    return;
                }
            }
        }

        if (board.GetTileAvailability(tilesToCheck[0]))
        {
            status = enumStatus.MOVING;

            if (team == enumTeam.allied)
            {
                MoveUnit(position + Vector2.up);
            }
            else
            {
                MoveUnit(position - Vector2.up);
            }
        }
        else
        {
            status = enumStatus.WAITING;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        DisplayHealth();

        if (health <= 0)
        {
            dead = true;
        }
    }

    void DisplayHealth()
    {
        healthBar.value = health;
    }

    // Check if unit is in position to be attacked
    public bool CheckIfReady()
    {
        return status == enumStatus.READY || status == enumStatus.ATTACKING;
    }

    public void SetUnitPosition(Vector2 newPosition)
    {
        if (board == null)
        {
            board = FindObjectOfType<Board>();
        }

        position = newPosition;

        if (newPosition != null)
        {
            board.SetTileUnit(position, GetComponent<Unit>());
        }
    }

    public void MoveUnit(Vector2 newPosition)
    {
        board.SetTileUnit(position, null);

        if (!board.GetTileAvailability(newPosition))
        {
            if (!board.GetUnit(newPosition).IsDead())
            {
                board.GetUnit(newPosition).transform.localPosition = new Vector3(position.x, board.GetUnit(newPosition).transform.localPosition.y, position.y);

                board.GetUnit(newPosition).SetUnitPosition(position);
            }
        }

        SetUnitPosition(newPosition);
    }

    void AttackOpponent()
    {
        if (team == enumTeam.allied)
        {
            bot.TakeDamage(1);
        }
        else
        {
            player.TakeDamage(1);
        }
        
        Die();
    }

    public bool IsDead()
    {
        return dead;
    }

    public void Die()
    {
        board.SetTileUnit(position, null);

        dead = true;

        board.CheckUnitsAlive();

        Destroy(gameObject);
    }

    private void OnMouseEnter()
    {
        highlighted = true;
    }

    private void OnMouseExit()
    {
        highlighted = false;
    }
}
