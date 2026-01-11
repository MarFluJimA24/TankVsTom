// Assets/Scripts/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

// Controla score, vidas, nivel/dificultad y transición a GameOver.
// Parche DOTween: mata tweens al destruir UI/cambiar escena para evitar warnings de RectTransform destruido.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text levelText;
    public TMP_Text tiltText;

    [Header("Rules")]
    public int startLives = 3;
    public float secondsPerLevel = 25f;

    [Header("Scenes")]
    public string gameOverSceneName = "GameOver";

    private int score;
    private int lives;
    private int level;
    private float aliveTime;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        score = 0;
        lives = startLives;
        level = 1;
        aliveTime = 0f;

        UpdateHUD();
    }

    void Update()
    {
        aliveTime += Time.deltaTime;
        int newLevel = 1 + Mathf.FloorToInt(aliveTime / secondsPerLevel);

        if (newLevel != level)
        {
            level = newLevel;
            UpdateHUD();

            // Feedback localizado en UI (safe): mata tween previo y link al GameObject (KillOnDestroy).
            PunchUI(levelText);
        }
    }

    public int GetLevel()
    {
        return level;
    }

    public void SetTiltValue(float tiltX)
    {
        if (tiltText != null)
            tiltText.text = $"Tilt: {tiltX:0.00}";
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateHUD();

        PunchUI(scoreText);
    }

    public void LoseLife(AudioClip sfxLoseLife)
    {
        lives--;

        AudioManager.I?.PlaySfx(sfxLoseLife, 1f);

        UpdateHUD();

        PunchUI(livesText);

        if (lives <= 0)
        {
            // Guardar score final
            SessionData.FinalScore = score;

            // Importante: matar tweens del HUD antes de cambiar de escena,
            // porque el Canvas se destruye al cargar GameOver.
            KillHudTweens();

            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    private void UpdateHUD()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (livesText != null) livesText.text = $"Lives: {lives}";
        if (levelText != null) levelText.text = $"Level: {level}";
    }

    // Punch seguro para textos: evita acumulación de tweens y evita warnings al destruir.
    private void PunchUI(TMP_Text tmp)
    {
        if (tmp == null) return;

        Transform t = tmp.transform;

        // Mata cualquier tween anterior sobre este Transform (evita stacking).
        t.DOKill();

        // Crea tween y lo enlaza al GameObject para que muera al destruirse.
        t.DOPunchScale(Vector3.one * 0.15f, 0.18f, 8, 1f)
            .SetLink(tmp.gameObject, LinkBehaviour.KillOnDestroy);
    }

    // Mata tweens del HUD de forma explícita (para cambios de escena).
    private void KillHudTweens()
    {
        if (scoreText != null) scoreText.transform.DOKill();
        if (livesText != null) livesText.transform.DOKill();
        if (levelText != null) levelText.transform.DOKill();
        if (tiltText != null) tiltText.transform.DOKill();
    }

    void OnDisable()
    {
        // Seguridad extra por si este objeto se desactiva antes de destruirse.
        KillHudTweens();
    }

    void OnDestroy()
    {
        // Seguridad extra por si se destruye en un cambio de escena.
        KillHudTweens();
    }
}
