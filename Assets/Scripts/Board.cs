using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Board : MonoBehaviour
{
    public float roundTimer;

    public int goldPerRound;

    public GameObject unitsParent;

    public Text timerDisplay;
    public GameObject startButton;

    public GameObject[] buyables;

    public GameObject winScreen;
    public Text winnerText;

    int round;

    Unit[] units;

    float roundTimerCount;

    Tile[,] tiles;

    bool playing;

    Tile highlightedTile;

    bool isDragging;
    Unit draggingUnit;

    public UnityAction OnRoundEnded;
    public UnityAction<int> OnRoundEndedInt;

    void Start()
    {
        Time.timeScale = 1;
        
        tiles = new Tile[8, 8];

        Tile[] findTiles = GetComponentsInChildren<Tile>();

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                tiles[i, j] = findTiles[8 * i + j];
            }
        }

        roundTimerCount = roundTimer;

        OnRoundEnded.Invoke();

        round = 1;
    }

    void Update()
    {        
        if (!GetRoundStarted())
        {
            if (roundTimerCount <= 0)
            {
                StartRound();
            }
            else
            {
                roundTimerCount -= Time.deltaTime;
            }
        }

        timerDisplay.text = ((int)roundTimerCount + 1).ToString();

        if (isDragging)
        {
            for (int i = 0; i < 8; i++)
            {
                if (draggingUnit.transform.localPosition.x > i - 0.5f && draggingUnit.transform.localPosition.x < i + 0.5f)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (draggingUnit.transform.localPosition.z > j - 0.5f && draggingUnit.transform.localPosition.z < j + 0.5f)
                        {
                            SetHighlightedTile(tiles[i, j]);

                            return;
                        }
                    }
                }
            }
        }
        else
        {
            SetHighlightedTile(null);
        }
    }

    public void AddUnit(GameObject unitPrefab)
    {
        Vector2 spawnTile = new Vector2();
        bool spawnSet = false;

        if (unitPrefab.GetComponent<Unit>().team.ToString() == "allied")
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GetTileAvailability(new Vector2(j, i)))
                    {
                        spawnTile = new Vector2(j, i);
                        spawnSet = true;
                        break;
                    }
                }
                if (spawnSet) break;
            }
        }
        else
        {
            for (int i = 7; i > 3; i--)
            {
                for (int j = 7; j > -1; j--)
                {
                    if (GetTileAvailability(new Vector2(j, i)))
                    {
                        spawnTile = new Vector2(j, i);
                        spawnSet = true;
                        break;
                    }
                }
                if (spawnSet) break;
            }
        }

        if (!spawnSet) return;

        Unit newUnit;
        newUnit = Instantiate(unitPrefab, unitsParent.transform).GetComponent<Unit>();
        newUnit.SetUnitPosition(spawnTile);
        newUnit.transform.localPosition = new Vector3(newUnit.position.x, newUnit.transform.localPosition.y, newUnit.position.y);

        if (newUnit.team.ToString() == "enemy")
        {
            newUnit.transform.Rotate(0, 180, 0);
        }
    }

    public bool GetTileAvailability(Vector2 tile)
    {
        return tiles[(int)tile.x, (int)tile.y].GetUnit() == null;
    }
    
    public bool CheckEnemyUnit(Vector2 tile, Unit unit)
    {
        return tiles[(int)tile.x, (int)tile.y].GetUnit().team != unit.team;
    }

    public void SetTileUnit(Vector2 tile, Unit unit)
    {
        tiles[(int)tile.x, (int)tile.y].SetUnit(unit);
    }

    public Unit GetUnit(Vector2 tile)
    {
        return tiles[(int)tile.x, (int)tile.y].GetUnit();
    }

    public void StartRound()
    {        
        playing = true;

        roundTimerCount = roundTimer;

        timerDisplay.gameObject.SetActive(false);
        startButton.SetActive(false);

        for (int i = 0; i < buyables.Length; i++)
        {
            buyables[i].SetActive(false);
        }
    }

    public void EndRound()
    {
        playing = false;

        round++;

        timerDisplay.gameObject.SetActive(true);
        startButton.SetActive(true);

        for (int i = 0; i < buyables.Length; i++)
        {
            buyables[i].SetActive(true);
        }

        OnRoundEndedInt.Invoke(goldPerRound * round);
        OnRoundEnded.Invoke();
    }

    public void CheckUnitsAlive()
    {
        units = GetComponentsInChildren<Unit>();

        for (int i = 0; i < units.Length; i++)
        {
            if (!units[i].IsDead())
            {
                return;
            }
        }

        EndRound();
    }

    public bool GetRoundStarted()
    {
        return playing;
    }

    public void SetHighlightedTile(Tile tile)
    {
        if (highlightedTile != null)
        {
            highlightedTile.Highlight(false);
        }
        
        highlightedTile = tile;
        
        if (isDragging)
        {
            tile.Highlight(true);
        }
    }

    public Tile GetHighlightedTile()
    {
        return highlightedTile;
    }

    public void IsDragging(bool dragging, Unit unit)
    {
        isDragging = dragging;

        draggingUnit = unit;
    }

    public void FinishGame(int winner)
    {
        winScreen.SetActive(true);

        if (winner == 1)
        {
            winnerText.text = "Vitória";
        }
        else
        {
            winnerText.text = "Derrota";
        }

        Time.timeScale = 0;
    }
}
