using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text movesText;
  [SerializeField] private TMP_Text statusText;

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
}