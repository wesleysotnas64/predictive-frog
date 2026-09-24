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

    [Header("Componentes")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public enum CandyColorType
    {
        Yellow,
        Blue,
        Red,
        Brown,
        Black
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
    /// Seleciona uma cor aleatória mantendo S e V no máximo (com adaptações para marrom e preto).
    /// </summary>
    public void ApplyRandomCandyColor()
    {
        if (spriteRenderer == null) return;

        CandyColorType randomType = (CandyColorType)Random.Range(0, 5);

        switch (randomType)
        {
            case CandyColorType.Yellow:
                spriteRenderer.color = Color.HSVToRGB(0.16f, 1.0f, 1.0f);
                break;

            case CandyColorType.Blue:
                spriteRenderer.color = Color.HSVToRGB(0.66f, 1.0f, 1.0f);
                break;

            case CandyColorType.Red:
                spriteRenderer.color = Color.HSVToRGB(0.0f, 1.0f, 1.0f);
                break;

            case CandyColorType.Brown:
                spriteRenderer.color = Color.HSVToRGB(0.08f, 1.0f, 0.4f);
                break;

            case CandyColorType.Black:
                spriteRenderer.color = Color.HSVToRGB(0.0f, 0.0f, 0.0f);
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

        // Altera a cor para uma nova tonalidade aleatória
        ApplyRandomCandyColor();
    }

    private void CollectCandy()
    {
        Debug.Log("Doce coletado pela formiga!");
        Destroy(gameObject);
    }
}