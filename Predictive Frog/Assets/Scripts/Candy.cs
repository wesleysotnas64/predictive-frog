using UnityEngine;

public class Candy : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";

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
        if (other.CompareTag(playerTag))
        {
            CollectCandy();
        }
    }

    private void CollectCandy()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.AddPoints(scoreValue);
        }
        Destroy(gameObject);
    }

    public int GetCandyValue()
    {
        return scoreValue;
    }
}