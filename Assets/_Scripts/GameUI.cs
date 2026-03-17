using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text movesText;
  [SerializeField] private TMP_Text statusText;

  [SerializeField] private TMP_Text finalScoreText;

  public void SetScore(int score)
  {
    if (scoreText != null)
      scoreText.text = $"Score: {score}";
  }

  public void SetMoves(int moves)
  {
    if (movesText != null)
      movesText.text = $"Moves: {moves}";
  }

  public void SetStatus(string message)
  {
    if (statusText != null)
      statusText.text = message;
  }

  public void SetFinalScore(int value)
  {
    if (finalScoreText != null)
      finalScoreText.text = $"Final Score\n<size=150%>{value}</size>";
  }
}