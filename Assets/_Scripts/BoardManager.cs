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
    [SerializeField] private GameManager gameManager;

    [Header("Animation")]
    [SerializeField] private float swapDuration = 0.15f;
    [SerializeField] private float fallTimePerCell = 0.08f;
    [SerializeField] private float clearDelay = 0.05f;

    [Header("Hint")]
    [SerializeField] private float hintDelay = 3f;

    public Food[,] Grid { get; private set; }

    private bool isBusy = false;
    public bool IsBusy => isBusy;

    private float idleTimer = 0f;
    private bool hintShown = false;
    private Food hintedA;
    private Food hintedB;

    private void Start()
    {
        // GenerateBoard();
    }

    private void Update()
    {
        if (isBusy) return;
        if (Grid == null) return;
        if (gameManager != null && gameManager.IsOutOfMoves) return;

        idleTimer += Time.deltaTime;

        if (!hintShown && idleTimer >= hintDelay)
        {
            ShowHintMove();
            hintShown = true;
        }
    }

    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        int safety = 0;

        do
        {
            GenerateBoardOnce();
            safety++;
        }
        while (!HasAnyValidMove() && safety < 100);

        NotifyPlayerAction();
    }

    private void GenerateBoardOnce()
    {
        ClearHintMove();
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
                return true;
        }

        if (y >= 2)
        {
            Food down1 = Grid[x, y - 1];
            Food down2 = Grid[x, y - 2];

            if (down1 != null && down2 != null &&
                down1.Type == type && down2.Type == type)
                return true;
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
    if (gameManager != null && gameManager.IsOutOfMoves) return;
    if (a == null || b == null) return;
    if (!AreAdjacent(a, b)) return;

    NotifyPlayerAction();
    StartCoroutine(TrySwapRoutine(a, b));
}

    public void NotifyPlayerAction()
    {
        idleTimer = 0f;
        hintShown = false;
        ClearHintMove();
    }

private IEnumerator TrySwapRoutine(Food a, Food b)
{
    isBusy = true;
    ClearHintMove();

    gameManager?.PlaySwapSfx();
    yield return StartCoroutine(SwapAnimated(a, b));

    HashSet<Food> uniqueMatches = new HashSet<Food>();

    foreach (Food food in matchDetector.FindMatchesAt(Grid, width, height, a.X, a.Y))
        uniqueMatches.Add(food);

    foreach (Food food in matchDetector.FindMatchesAt(Grid, width, height, b.X, b.Y))
        uniqueMatches.Add(food);

    List<Food> matches = new List<Food>(uniqueMatches);

    if (matches.Count == 0)
    {
        yield return StartCoroutine(SwapAnimated(a, b));
        gameManager?.PlayInvalidSwapSfx();
    }
    else
    {
        gameManager?.ConsumeMove();
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

        a.StartCoroutine(a.MoveTo(GetWorldPosition(bx, by), swapDuration));
        b.StartCoroutine(b.MoveTo(GetWorldPosition(ax, ay), swapDuration));

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
                if (!HasAnyValidMove())
                {
                    gameManager?.SetStatus("No moves! Regenerate...");
                    gameManager?.PlayBoardRegenSfx();
                    yield return new WaitForSeconds(0.25f);
                    GenerateBoard();
                    gameManager?.SetStatus(string.Empty);
                }

                yield break;
            }

            comboStep++;
            gameManager?.AddScore(matches.Count, comboStep);
            ClearMatches(matches);
            gameManager?.PlayClearSfx();

            if (clearDelay > 0f)
                yield return new WaitForSeconds(clearDelay);

            yield return StartCoroutine(CollapseColumnsRoutine());
            yield return StartCoroutine(RefillBoardRoutine());
        }
    }

    private IEnumerator CollapseColumnsRoutine()
    {
        float maxDuration = 0f;

        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;

            for (int y = 0; y < height; y++)
            {
                if (Grid[x, y] == null)
                {
                    if (emptyY == -1) emptyY = y;
                }
                else if (emptyY != -1)
                {
                    Food food = Grid[x, y];
                    Grid[x, emptyY] = food;
                    Grid[x, y] = null;

                    int distance = y - emptyY;
                    float duration = distance * fallTimePerCell;

                    food.SetGridPosition(x, emptyY);
                    food.StartCoroutine(food.MoveTo(GetWorldPosition(x, emptyY), duration));

                    maxDuration = Mathf.Max(maxDuration, duration);
                    emptyY++;
                }
            }
        }

        if (maxDuration > 0f)
            yield return new WaitForSeconds(maxDuration);
    }

    private IEnumerator RefillBoardRoutine()
    {
        float maxDuration = 0f;

        for (int x = 0; x < width; x++)
        {
            int spawnOffset = 0;

            for (int y = 0; y < height; y++)
            {
                if (Grid[x, y] == null)
                {
                    FoodType type = GetRandomTypeWithoutMatch(x, y);

                    int spawnY = height + spawnOffset;
                    Food food = foodFactory.Create(type, GetWorldPosition(x, spawnY), boardRoot);
                    if (food == null) continue;

                    food.Init(x, y, type);
                    Grid[x, y] = food;

                    int distance = spawnY - y;
                    float duration = distance * fallTimePerCell;

                    food.StartCoroutine(food.MoveTo(GetWorldPosition(x, y), duration));
                    maxDuration = Mathf.Max(maxDuration, duration);

                    spawnOffset++;
                }
            }
        }

        if (maxDuration > 0f)
            yield return new WaitForSeconds(maxDuration);
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

    private bool AreAdjacent(Food a, Food b)
    {
        int dx = Mathf.Abs(a.X - b.X);
        int dy = Mathf.Abs(a.Y - b.Y);
        return dx + dy == 1;
    }

    public bool HasAnyValidMove()
    {
        return TryGetHintMove(out _, out _);
    }

    public bool TryGetHintMove(out Food first, out Food second)
    {
        first = null;
        second = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Food current = Grid[x, y];
                if (current == null) continue;

                if (x < width - 1)
                {
                    Food right = Grid[x + 1, y];
                    if (right != null && TestSwapCreatesMatch(current, right))
                    {
                        first = current;
                        second = right;
                        return true;
                    }
                }

                if (y < height - 1)
                {
                    Food up = Grid[x, y + 1];
                    if (up != null && TestSwapCreatesMatch(current, up))
                    {
                        first = current;
                        second = up;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void ShowHintMove()
    {
        ClearHintMove();

        if (TryGetHintMove(out Food a, out Food b))
        {
            hintedA = a;
            hintedB = b;
            hintedA.SetHintHighlight(true);
            hintedB.SetHintHighlight(true);
        }
    }

    public void ClearHintMove()
    {
        if (hintedA != null) hintedA.SetHintHighlight(false);
        if (hintedB != null) hintedB.SetHintHighlight(false);

        hintedA = null;
        hintedB = null;
    }

    private bool TestSwapCreatesMatch(Food a, Food b)
    {
        SwapInGrid(a, b);

        HashSet<Food> uniqueMatches = new HashSet<Food>();

        foreach (Food food in matchDetector.FindMatchesAt(Grid, width, height, a.X, a.Y))
            uniqueMatches.Add(food);

        foreach (Food food in matchDetector.FindMatchesAt(Grid, width, height, b.X, b.Y))
            uniqueMatches.Add(food);

        SwapInGrid(a, b);

        return uniqueMatches.Count > 0;
    }

    private void SwapInGrid(Food a, Food b)
    {
        int ax = a.X;
        int ay = a.Y;
        int bx = b.X;
        int by = b.Y;

        Grid[ax, ay] = b;
        Grid[bx, by] = a;

        a.SetGridPosition(bx, by);
        b.SetGridPosition(ax, ay);
    }

    public void ClearBoard()
    {
        if (boardRoot == null) return;

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = boardRoot.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    public void StartGameBoard()
{
    StopAllCoroutines();
    isBusy = false;

    ClearHintMove();
    idleTimer = 0f;
    hintShown = false;

    ClearBoard();
    GenerateBoard();
}
}