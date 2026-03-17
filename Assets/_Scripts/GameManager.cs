using UnityEngine;

public class GameManager : MonoBehaviour
{
  [Header("Gameplay")]
  [SerializeField] private int startingMoves = 30;
  [SerializeField] private int scorePerTile = 10;

  [Header("Panels")]
  [SerializeField] private GameObject startMenuPanel;
  [SerializeField] private GameObject gameplayPanel;
  [SerializeField] private GameObject endMenuPanel;

  [Header("Board")]
  [SerializeField] private GameObject boardRoot;
  [SerializeField] private BoardManager boardManager;

  [Header("References")]
  [SerializeField] private GameUI gameUI;
  [SerializeField] private SfxManager sfxManager;

  public int CurrentScore { get; private set; }
  public int CurrentMoves { get; private set; }
  public bool IsOutOfMoves { get; private set; }

  private void Start()
  {
    ShowStartMenu();
  }

  public void ShowStartMenu()
  {
    IsOutOfMoves = false;

    startMenuPanel?.SetActive(true);
    gameplayPanel?.SetActive(false);
    endMenuPanel?.SetActive(false);
    boardRoot?.SetActive(false);

    SetStatus(string.Empty);
  }

  public void OnPlayButton()
  {
    StartNewGame();
  }

  public void StartNewGame()
  {
    CurrentScore = 0;
    CurrentMoves = startingMoves;
    IsOutOfMoves = false;
    gameUI?.SetFinalScore(0);

    startMenuPanel?.SetActive(false);
    gameplayPanel?.SetActive(true);
    endMenuPanel?.SetActive(false);
    boardRoot?.SetActive(true);

    RefreshUI();
    SetStatus(string.Empty);

    boardManager?.StartGameBoard();
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
      ShowEndMenu();
    }
  }

  public void AddScore(int matchedCount, int comboStep)
  {
    int gainedScore = matchedCount * scorePerTile * comboStep;
    CurrentScore += gainedScore;
    RefreshUI();

    Debug.Log($"Score +{gainedScore} | Total: {CurrentScore}");
  }

  public void ShowEndMenu()
  {
    gameplayPanel?.SetActive(false);
    boardRoot?.SetActive(false);
    endMenuPanel?.SetActive(true);
    gameUI?.SetFinalScore(CurrentScore);
  }

  public void OnRestartButton()
  {
    StartNewGame();
  }

  public void OnOutButton()
  {
    ShowStartMenu();
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