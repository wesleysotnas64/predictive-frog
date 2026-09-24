using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CandySpawnController : MonoBehaviour
{
    [Header("Prefab e Configurações")]
    [SerializeField] private GameObject candyPrefab;

    [Tooltip("Quantidade padrão de doces a serem gerados no spawn simples.")]
    [SerializeField] private int defaultCandyAmount = 5;

    [Header("Limites de Spawn (Mesmos do Candy.cs)")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 0f;

    [Header("Controle de Instâncias")]
    [SerializeField] private List<GameObject> activeCandies = new List<GameObject>();

    private void Update()
    {
        // Limpa a lista removendo referências nulas (doces que foram destruídos ao serem coletados)
        activeCandies.RemoveAll(candy => candy == null);

        // Teste: Pressionar a tecla R para gerar doces com quantidade aleatória
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SpawnRandomCandiesWithRandomAmount(1, 8); // Exemplo: gera entre 1 e 8 doces
        }
    }

    /// <summary>
    /// Spawna a quantidade fixa configurada em 'defaultCandyAmount'.
    /// </summary>
    public void SpawnDefaultAmount()
    {
        SpawnCandies(defaultCandyAmount);
    }

    /// <summary>
    /// Spawna uma quantidade totalmente aleatória de doces dentro de um intervalo [min, max].
    /// </summary>
    /// <param name="minAmount">Mínimo de doces a serem gerados.</param>
    /// <param name="maxAmount">Máximo de doces a serem gerados.</param>
    public void SpawnRandomCandiesWithRandomAmount(int minAmount = 1, int maxAmount = 10)
    {
        int randomAmount = Random.Range(minAmount, maxAmount + 1);
        SpawnCandies(randomAmount);
    }

    /// <summary>
    /// Executa a instanciação dos doces em posições aleatórias e adiciona à lista de controle.
    /// </summary>
    /// <param name="amount">Quantidade exata de doces a instanciar.</param>
    public void SpawnCandies(int amount)
    {
        if (candyPrefab == null)
        {
            Debug.LogError("CandyPrefab não foi atribuído no CandySpawnController!");
            return;
        }

        float candyRadius = 0.4f; // Raio de checagem para evitar sobreposição

        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = Vector3.zero;
            bool validPositionFound = false;
            int maxAttempts = 15; // Evita loop infinito

            // Tenta achar um ponto sem nenhum doce por perto
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float randomX = Random.Range(minX, maxX);
                float randomY = Random.Range(minY, maxY);
                Vector3 testPos = new Vector3(randomX, randomY, 0f);

                // Verifica se já existe algum collider de Doce nesse ponto
                Collider2D hit = Physics2D.OverlapCircle(testPos, candyRadius, LayerMask.GetMask("Default"));
                
                if (hit == null || !hit.CompareTag("Candy"))
                {
                    spawnPosition = testPos;
                    validPositionFound = true;
                    break;
                }
            }

            // Se não achou ponto ideal após tentativas, usa a última posição sorteada mesmo assim
            if (!validPositionFound)
            {
                spawnPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            }

            GameObject newCandy = Instantiate(candyPrefab, spawnPosition, Quaternion.identity);
            activeCandies.Add(newCandy);
        }
    }

    /// <summary>
    /// Retorna a lista atual de doces ativos na tela (útil para os dados da IA no futuro).
    /// </summary>
    public List<GameObject> GetActiveCandies()
    {
        activeCandies.RemoveAll(candy => candy == null);
        return activeCandies;
    }

    /// <summary>
    /// Destrói todos os doces atualmente ativos na tela e limpa a lista.
    /// </summary>
    public void ClearAllCandies()
    {
        foreach (GameObject candy in activeCandies)
        {
            if (candy != null)
            {
                Destroy(candy);
            }
        }
        activeCandies.Clear();
    }
}