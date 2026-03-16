using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private int startingMoves = 30;
    [SerializeField] private int scorePerTile = 10;

    [Header("References")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private SfxManager sfxManager;

    public int CurrentScore { get; private set; }
    public int CurrentMoves { get; private set; }
    public bool IsOutOfMoves { get; private set; }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        CurrentScore = 0;
        CurrentMoves = startingMoves;
        IsOutOfMoves = false;

        RefreshUI();
        SetStatus(string.Empty);
    }

    public void ConsumeMove()
    {
        if (IsOutOfMoves) return;

        CurrentMoves = Mathf.Max(0, CurrentMoves - 1);
        RefreshUI();

        if (CurrentMoves <= 0)
        {
            IsOutOfMoves = true;
            SetStatus("Out of moves!");
            sfxManager?.PlayOutOfMoves();
        }
    }

    public void AddScore(int matchedCount, int comboStep)
    {
        int gainedScore = matchedCount * scorePerTile * comboStep;
        CurrentScore += gainedScore;
        RefreshUI();

        Debug.Log($"Score +{gainedScore} | Total: {CurrentScore}");
    }

    public void SetStatus(string message)
    {
        gameUI?.SetStatus(message);
    }

    private void RefreshUI()
    {
        gameUI?.SetScore(CurrentScore);
        gameUI?.SetMoves(CurrentMoves);
    }

    public void PlaySwapSfx() => sfxManager?.PlaySwap();
    public void PlayInvalidSwapSfx() => sfxManager?.PlayInvalidSwap();
    public void PlayClearSfx() => sfxManager?.PlayClear();
    public void PlayBoardRegenSfx() => sfxManager?.PlayBoardRegen();
}