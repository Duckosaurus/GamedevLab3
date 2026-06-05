using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider;

    [Header("Coins")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("UI Canvases")]
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private CanvasGroup victoryCanvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    private float currentHealth;
    private int coinCount;
    private bool isDead;

    public bool IsDead => isDead;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        coinCount = 0;
        isDead = false;

        UpdateHealthUI();
        UpdateCoinUI();

        SetCanvasGroup(hudCanvasGroup, true);
        SetCanvasGroup(gameOverCanvasGroup, false);
        if (victoryCanvasGroup != null)
            SetCanvasGroup(victoryCanvasGroup, false);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        StartCoroutine(FadeCanvasGroup(hudCanvasGroup, 1f, 0f));
        StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 0f, 1f));
    }

    public void Respawn()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = respawnPoint.position;
            cc.enabled = true;
        }

        currentHealth = maxHealth;
        coinCount = 0;
        isDead = false;

        UpdateHealthUI();
        UpdateCoinUI();

        StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 1f, 0f));
        StartCoroutine(FadeCanvasGroup(hudCanvasGroup, 0f, 1f));
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateCoinUI();
    }

    public void ShowVictory()
    {
        if (victoryCanvasGroup == null) return;

        StartCoroutine(FadeCanvasGroup(hudCanvasGroup, 1f, 0f));
        StartCoroutine(FadeCanvasGroup(victoryCanvasGroup, 0f, 1f));
        isDead = true;
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = coinCount.ToString();
    }

    private void SetCanvasGroup(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        group.interactable = to > 0.5f;
        group.blocksRaycasts = to > 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = to;
    }
}
