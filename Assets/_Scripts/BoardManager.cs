using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
  [Header("Board")]
  [SerializeField] private int width = 8;
  [SerializeField] private int height = 8;
  [SerializeField] private float cellSize = 1f;

  [Header("Layout")]
  [SerializeField] private Vector2 startPosition = new Vector2(-3.5f, -3.5f);
  [SerializeField] private Transform boardRoot;

  [Header("References")]
  [SerializeField] private FoodFactory foodFactory;
  [SerializeField] private MatchDetector matchDetector;

  [Header("Animation")]
  [SerializeField] private float swapDuration = 0.15f;
  [SerializeField] private float fallDuration = 0.12f;
  [SerializeField] private float clearDelay = 0.05f;

  public Food[,] Grid { get; private set; }

  private bool isBusy = false;
  public bool IsBusy => isBusy;

  private void Start()
  {
    GenerateBoard();
  }

  [ContextMenu("Generate Board")]
  public void GenerateBoard()
  {
    ClearBoard();

    Grid = new Food[width, height];

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        SpawnFood(x, y);
      }
    }
  }

  private void SpawnFood(int x, int y)
  {
    FoodType type = GetRandomTypeWithoutMatch(x, y);
    Vector3 pos = GetWorldPosition(x, y);

    Food food = foodFactory.Create(type, pos, boardRoot);
    if (food == null) return;

    food.Init(x, y, type);
    Grid[x, y] = food;
  }

  private FoodType GetRandomTypeWithoutMatch(int x, int y)
  {
    FoodType type;
    int safety = 0;

    do
    {
      type = foodFactory.GetRandomType();
      safety++;
    }
    while (CreatesImmediateMatch(x, y, type) && safety < 100);

    return type;
  }

  private bool CreatesImmediateMatch(int x, int y, FoodType type)
  {
    if (x >= 2)
    {
      Food left1 = Grid[x - 1, y];
      Food left2 = Grid[x - 2, y];

      if (left1 != null && left2 != null &&
          left1.Type == type && left2.Type == type)
      {
        return true;
      }
    }

    if (y >= 2)
    {
      Food down1 = Grid[x, y - 1];
      Food down2 = Grid[x, y - 2];

      if (down1 != null && down2 != null &&
          down1.Type == type && down2.Type == type)
      {
        return true;
      }
    }

    return false;
  }

  public Vector3 GetWorldPosition(int x, int y)
  {
    return new Vector3(
        startPosition.x + x * cellSize,
        startPosition.y + y * cellSize,
        0f
    );
  }

  public bool IsInsideBoard(int x, int y)
  {
    return x >= 0 && x < width && y >= 0 && y < height;
  }

  public Food GetFood(int x, int y)
  {
    if (!IsInsideBoard(x, y)) return null;
    return Grid[x, y];
  }

  public Food GetNeighbor(int x, int y, Vector2Int direction)
  {
    int targetX = x + direction.x;
    int targetY = y + direction.y;

    if (!IsInsideBoard(targetX, targetY)) return null;

    return Grid[targetX, targetY];
  }

  public void TrySwap(Food a, Food b)
  {
    if (isBusy) return;
    if (a == null || b == null) return;
    if (!AreAdjacent(a, b)) return;

    StartCoroutine(TrySwapRoutine(a, b));
  }

  private IEnumerator TrySwapRoutine(Food a, Food b)
  {
    isBusy = true;

    yield return StartCoroutine(SwapAnimated(a, b));

    List<Food> matches = matchDetector.FindAllMatches(Grid, width, height);

    if (matches.Count == 0)
    {
      yield return StartCoroutine(SwapAnimated(a, b));
      Debug.Log("No match, swap back");
    }
    else
    {
      yield return StartCoroutine(ResolveBoardRoutine());
    }

    isBusy = false;
  }

  private IEnumerator SwapAnimated(Food a, Food b)
  {
    int ax = a.X;
    int ay = a.Y;
    int bx = b.X;
    int by = b.Y;

    Grid[ax, ay] = b;
    Grid[bx, by] = a;

    a.SetGridPosition(bx, by);
    b.SetGridPosition(ax, ay);

    Vector3 aTarget = GetWorldPosition(bx, by);
    Vector3 bTarget = GetWorldPosition(ax, ay);

    a.StartCoroutine(a.MoveTo(aTarget, swapDuration));
    b.StartCoroutine(b.MoveTo(bTarget, swapDuration));

    yield return new WaitForSeconds(swapDuration);
  }

  private IEnumerator ResolveBoardRoutine()
  {
    int comboStep = 0;

    while (true)
    {
      List<Food> matches = matchDetector.FindAllMatches(Grid, width, height);

      if (matches.Count == 0)
      {
        Debug.Log("Board stable");
        yield break;
      }

      comboStep++;
      Debug.Log("Cascade step: " + comboStep + " | Matches: " + matches.Count);

      ClearMatches(matches);

      if (clearDelay > 0f)
      {
        yield return new WaitForSeconds(clearDelay);
      }

      yield return StartCoroutine(CollapseColumnsRoutine());
      yield return StartCoroutine(RefillBoardRoutine());
    }
  }

  private IEnumerator CollapseColumnsRoutine()
  {
    bool moved = false;

    for (int x = 0; x < width; x++)
    {
      int emptyY = -1;

      for (int y = 0; y < height; y++)
      {
        if (Grid[x, y] == null)
        {
          if (emptyY == -1)
          {
            emptyY = y;
          }
        }
        else if (emptyY != -1)
        {
          Food food = Grid[x, y];
          Grid[x, emptyY] = food;
          Grid[x, y] = null;

          food.SetGridPosition(x, emptyY);
          food.StartCoroutine(food.MoveTo(GetWorldPosition(x, emptyY), fallDuration));

          emptyY++;
          moved = true;
        }
      }
    }

    if (moved)
    {
      yield return new WaitForSeconds(fallDuration);
    }
  }
  private IEnumerator RefillBoardRoutine()
  {
    bool spawned = false;

    for (int x = 0; x < width; x++)
    {
      int spawnOffset = 0;

      for (int y = 0; y < height; y++)
      {
        if (Grid[x, y] == null)
        {
          FoodType type = GetRandomTypeWithoutMatch(x, y);

          Vector3 spawnPos = GetWorldPosition(x, height + spawnOffset);
          Food food = foodFactory.Create(type, spawnPos, boardRoot);

          if (food == null) continue;

          food.Init(x, y, type);
          Grid[x, y] = food;

          food.StartCoroutine(food.MoveTo(GetWorldPosition(x, y), fallDuration));

          spawnOffset++;
          spawned = true;
        }
      }
    }

    if (spawned)
    {
      yield return new WaitForSeconds(fallDuration);
    }
  }
  private bool AreAdjacent(Food a, Food b)
  {
    int dx = Mathf.Abs(a.X - b.X);
    int dy = Mathf.Abs(a.Y - b.Y);

    return dx + dy == 1;
  }

  private void ClearMatches(List<Food> matches)
  {
    foreach (Food food in matches)
    {
      if (food == null) continue;

      Grid[food.X, food.Y] = null;
      Destroy(food.gameObject);
    }
  }

  public void ClearBoard()
  {
    if (boardRoot == null) return;

    for (int i = boardRoot.childCount - 1; i >= 0; i--)
    {
      DestroyImmediate(boardRoot.GetChild(i).gameObject);
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.white;

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        Vector3 pos = new Vector3(
            startPosition.x + x * cellSize,
            startPosition.y + y * cellSize,
            0f
        );

        Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
      }
    }
  }
}