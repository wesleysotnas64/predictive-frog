using System.Collections;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    public static FrogController Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private FrogsTongue tongueScript;

    [Header("Configuração dos Ataques")]
    [Tooltip("Quantidade exata ou média de ataques desejada por round.")]
    [SerializeField] private int targetAttackCount = 5;

    [Tooltip("Variação aleatória na quantidade de ataques (ex: se 1, pode fazer entre targetAttackCount - 1 e targetAttackCount + 1).")]
    [SerializeField] private int attackCountVariance = 1;

    [Header("Limites do Mapa para Teste (Sorteio de Alvo)")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4.5f;
    [SerializeField] private float maxY = 4.5f;

    private Coroutine attackRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicia o ciclo de ataques pseudo-aleatórios do sapo para o round atual.
    /// </summary>
    public void StartAttackingRoutine()
    {
        StopAttackingRoutine(); // Garante que não haja rotinas duplicadas
        attackRoutine = StartCoroutine(FrogAttackCycle());
    }

    /// <summary>
    /// Parar imediatamente os ataques do sapo (no fim do round ou fase de aprendizado).
    /// </summary>
    public void StopAttackingRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator FrogAttackCycle()
    {
        // 1. Determina a quantidade final de ataques para este round
        int totalAttacksThisRound = Mathf.Max(1, targetAttackCount + Random.Range(-attackCountVariance, attackCountVariance + 1));

        // 2. Calcula o tempo médio entre disparos baseado na duração do round (10 segundos)
        // Reservamos uma margem inicial/final para o sapo não atacar no segundo 0.0s nem no 10.0s
        float totalRoundDuration = 10.0f;
        float baseInterval = totalRoundDuration / (totalAttacksThisRound + 1);

        Debug.Log($"[FrogController] Iniciando ciclo com {totalAttacksThisRound} ataques planejados.");

        for (int i = 0; i < totalAttacksThisRound; i++)
        {
            // Sorteia uma pequena variação de intervalo (cadência pseudo-aleatória)
            float randomDelay = baseInterval + Random.Range(-0.3f, 0.3f);
            randomDelay = Mathf.Max(0.5f, randomDelay); // Mantém um delay mínimo seguro

            yield return new WaitForSeconds(randomDelay);

            // Executa o disparo de teste
            ExecuteRandomAttack();
        }
    }

    /// <summary>
    /// Sorteia um ponto aleatório e aciona a língua (Apenas para Teste).
    /// </summary>
    private void ExecuteRandomAttack()
    {
        if (tongueScript == null) return;

        // Sorteia uma posição (X, Y) dentro dos limites definidos
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 randomTarget = new Vector3(randomX, randomY, 0f);

        Debug.Log($"[FrogController] Disparando língua para alvo de teste: {randomTarget}");

        tongueScript.LaunchAttack(randomTarget);
    }
}