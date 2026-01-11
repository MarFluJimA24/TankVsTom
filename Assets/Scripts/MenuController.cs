// Assets/Scripts/MenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public Button exitButton;

    [Header("Scenes")]
    public string gameSceneName = "Game";

    void Awake()
    {
        // Conecta los botones a sus funciones.
        if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitPressed);
    }

    void OnStartPressed()
    {
        // Carga la escena del juego.
        SceneManager.LoadScene(gameSceneName);
    }

    void OnExitPressed()
    {
        // Cierra la app (en el editor no se cierra, en móvil sí).
        Application.Quit();
    }
}
