using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private UIController uiController;

    [Header("Atributos do Jogo")]
    [SerializeField] private int currentPoints = 0;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        // Singleton simples para facilitar o acesso global
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Tenta buscar o UIController automaticamente se não estiver atribuído
        if (uiController == null)
        {
            // uiController = FindObjectOfType<UIController>();
            uiController = GameObject.Find("UIController")?.GetComponent<UIController>();
        }

        // Inicializa o estado do jogo
        currentLives = maxLives;
        
        // Atualiza a UI com os valores iniciais
        UpdateAllUI();
    }

    /// <summary>
    /// Adiciona pontos ao total e notifica o UIController.
    /// </summary>
    public void AddPoints(int points)
    {
        currentPoints += points;
        if (uiController != null)
        {
            uiController.UpdateScoreUI(currentPoints);
        }
    }

    /// <summary>
    /// Deduz uma vida do jogador e notifica a UI.
    /// </summary>
    public void LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        if (uiController != null)
        {
            uiController.UpdateLivesUI(currentLives);
        }

        if (currentLives <= 0)
        {
            Debug.Log("Game Over! A formiga ficou sem vidas.");
        }
    }

    /// <summary>
    /// Sincroniza todos os textos da interface.
    /// </summary>
    public void UpdateAllUI()
    {
        if (uiController != null)
        {
            uiController.UpdateScoreUI(currentPoints);
            uiController.UpdateLivesUI(currentLives);
        }
    }

    public int GetCurrentPoints() => currentPoints;
    public int GetCurrentLives() => currentLives;
}