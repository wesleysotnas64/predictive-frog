using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CandySpawnController : MonoBehaviour
{
    [Header("Prefab e Configurações")]
    [SerializeField] private GameObject candyPrefab;

    [Tooltip("Quantidade padrão de doces a serem gerados no spawn simples.")]
    [SerializeField] private int defaultCandyAmount = 5;

    [Header("Limites de Spawn")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 0f;

    [Header("Distâncias de Segurança")]
    [Tooltip("Distância mínima em relação à posição inicial do jogador (0, -4).")]
    [SerializeField] private float minDistanceFromPlayer = 1.5f;

    [Tooltip("Distância mínima entre um doce e outro.")]
    [SerializeField] private float minDistanceBetweenCandies = 0.8f;

    [Header("Controle de Instâncias")]
    [SerializeField] private List<GameObject> activeCandies = new List<GameObject>();

    private readonly Vector3 playerStartPosition = new Vector3(0f, -4f, 0f);

    private void Update()
    {
        activeCandies.RemoveAll(candy => candy == null);

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SpawnCandies(defaultCandyAmount);
        }
    }

    public void SpawnDefaultAmount()
    {
        SpawnCandies(defaultCandyAmount);
    }

    public void SpawnRandomCandiesWithRandomAmount(int minAmount = 1, int maxAmount = 10)
    {
        int randomAmount = Random.Range(minAmount, maxAmount + 1);
        SpawnCandies(randomAmount);
    }

    /// <summary>
    /// Instancia os doces garantindo distância mínima entre doces e em relação ao jogador (0, -4).
    /// </summary>
    public void SpawnCandies(int amount)
    {
        if (candyPrefab == null) return;

        ClearAllCandies();

        int maxAttempts = 30; // Evita travamentos por loop infinito

        for (int i = 0; i < amount; i++)
        {
            Vector3 validPosition = Vector3.zero;
            bool foundValidPos = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float randomX = Random.Range(minX, maxX);
                float randomY = Random.Range(minY, maxY);
                Vector3 candidatePos = new Vector3(randomX, randomY, 0f);

                // 1. Verifica a distância em módulo para o Player (0, -4)
                if (Vector3.Distance(candidatePos, playerStartPosition) < minDistanceFromPlayer)
                {
                    continue;
                }

                // 2. Verifica a distância para outros doces já gerados
                bool tooCloseToOtherCandy = false;
                foreach (GameObject existingCandy in activeCandies)
                {
                    if (existingCandy != null && Vector3.Distance(candidatePos, existingCandy.transform.position) < minDistanceBetweenCandies)
                    {
                        tooCloseToOtherCandy = true;
                        break;
                    }
                }

                if (!tooCloseToOtherCandy)
                {
                    validPosition = candidatePos;
                    foundValidPos = true;
                    break;
                }
            }

            // Fallback: se não achar um ponto perfeito após 30 tentativas, usa o último gerado
            if (!foundValidPos)
            {
                validPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            }

            GameObject newCandy = Instantiate(candyPrefab, validPosition, Quaternion.identity);
            activeCandies.Add(newCandy);
        }
    }

    public List<GameObject> GetActiveCandies()
    {
        activeCandies.RemoveAll(candy => candy == null);
        return activeCandies;
    }

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