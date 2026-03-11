using UnityEngine;

public class PlayButtonScript : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
