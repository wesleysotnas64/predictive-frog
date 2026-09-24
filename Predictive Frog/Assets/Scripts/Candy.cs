using UnityEngine;

public class Candy : MonoBehaviour
{
    [Header("Limites Fixos de Spawn")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 0f;

    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string candyTag = "Candy";

    [Header("Componentes e Propriedades")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Valor de pontuação do doce (10, 20, 30, 40 ou 50).")]
    [SerializeField] private int scoreValue = 10;

    public enum CandyColorType
    {
        Yellow, // 10 pontos
        Blue,   // 20 pontos
        Red,    // 30 pontos
        Brown,  // 40 pontos
        Black   // 50 pontos
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        ApplyRandomCandyColor();
    }

    /// <summary>
    /// Seleciona uma cor aleatória mantendo S e V no máximo (com adaptações para marrom e preto)
    /// e atribui a pontuação correspondente ao tipo do doce.
    /// </summary>
    public void ApplyRandomCandyColor()
    {
        if (spriteRenderer == null) return;

        CandyColorType randomType = (CandyColorType)Random.Range(0, 5);

        switch (randomType)
        {
            case CandyColorType.Yellow:
                spriteRenderer.color = Color.HSVToRGB(0.16f, 1.0f, 1.0f);
                scoreValue = 10;
                break;

            case CandyColorType.Blue:
                spriteRenderer.color = Color.HSVToRGB(0.66f, 1.0f, 1.0f);
                scoreValue = 20;
                break;

            case CandyColorType.Red:
                spriteRenderer.color = Color.HSVToRGB(0.0f, 1.0f, 1.0f);
                scoreValue = 30;
                break;

            case CandyColorType.Brown:
                spriteRenderer.color = Color.HSVToRGB(0.08f, 1.0f, 0.4f);
                scoreValue = 40;
                break;

            case CandyColorType.Black:
                spriteRenderer.color = Color.HSVToRGB(0.0f, 0.0f, 0.0f);
                scoreValue = 50;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Se colidir com o Player: o doce é destruído
        if (other.CompareTag(playerTag))
        {
            CollectCandy();
            return;
        }

        // 2. Se colidir com outro Doce ao nascer: reposiciona no limite definido
        if (other.CompareTag(candyTag))
        {
            RepositionCandy();
        }
    }

    /// <summary>
    /// Reposiciona o doce dentro dos limites X [-8, 8] e Y [-4, 0]
    /// </summary>
    public void RepositionCandy()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        transform.position = new Vector3(randomX, randomY, 0f);

        // Altera a cor e reatribui o valor para uma nova combinação aleatória
        ApplyRandomCandyColor();
    }

    private void CollectCandy()
    {
        // Soma os pontos no GameController
        if (GameController.Instance != null)
        {
            GameController.Instance.AddPoints(scoreValue);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Retorna o valor de pontuação atribuído a este doce.
    /// </summary>
    public int GetCandyValue()
    {
        return scoreValue;
    }
}