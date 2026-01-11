// Assets/Scripts/GameOverController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text finalScoreText;
    public Button retryButton;
    public Button menuButton;

    [Header("Scenes")]
    public string gameSceneName = "Game";
    public string menuSceneName = "MainMenu";

    void Start()
    {
        // Muestra el score final guardado por el GameManager.
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {SessionData.FinalScore}";

        // Conecta botones.
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryPressed);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuPressed);
    }

    void OnRetryPressed()
    {
        // Reinicio simple: recargar escena Game.
        SceneManager.LoadScene(gameSceneName);
    }

    void OnMenuPressed()
    {
        // Volver al menú.
        SceneManager.LoadScene(menuSceneName);
    }
}
