using System.Collections;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Elementos da Interface")]
    [Tooltip("Imagem da barra colorida (BarTime) que diminui e aumenta.")]
    [SerializeField] private RectTransform barTime;

    [Tooltip("Texto MeshPro (TimeAndLearningText) para exibir tempo e status.")]
    [SerializeField] private TextMeshProUGUI timeAndLearningText;

    [Tooltip("Texto MeshPro (Round Text) para exibir o número do round.")]
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Configurações Acadêmicas/Animação")]
    [Tooltip("Tempo em segundos reservado para a simulação de aprendizado (re-treino da IA).")]
    [SerializeField] private float learningDuration = 3.0f;

    private bool isLearningMode = false;

    /// <summary>
    /// Atualiza a barra e o texto com base no tempo decorrido enviado pelo TimeSeriesDataCollector.
    /// </summary>
    /// <param name="currentRoundTimer">Tempo atual decorrido no round (0 até totalRoundTime).</param>
    /// <param name="totalRoundTime">Tempo total do round em segundos (ex: 10s).</param>
    /// <param name="currentRound">Número do round atual.</param>
    public void UpdateRoundUI(float currentRoundTimer, float totalRoundTime, int currentRound)
    {
        if (roundText != null)
        {
            roundText.text = $"RND: {currentRound}";
        }

        // Se estiver executando o ciclo de aprendizado, ignora atualizações do timer do round
        if (isLearningMode) return;

        // Calcula o tempo restante (10.0s -> 0.0s)
        float timeRemaining = Mathf.Max(0f, totalRoundTime - currentRoundTimer);

        // 1. Atualiza o texto com 2 casas decimais (Ex: "8.45s")
        if (timeAndLearningText != null)
        {
            timeAndLearningText.text = $"{timeRemaining:F2}s";
        }

        // 2. Calcula a proporção normalizada [1.0 -> 0.0]
        float timeRatio = Mathf.Clamp01(timeRemaining / totalRoundTime);

        // 3. Altera a escala X da BarTime
        if (barTime != null)
        {
            Vector3 scale = barTime.localScale;
            scale.x = timeRatio;
            barTime.localScale = scale;
        }

        // Quando o tempo do round esgota (chega a 0.00s), dispara a fase de aprendizado
        if (timeRemaining <= 0f)
        {
            StartCoroutine(LearningPhaseRoutine());
        }
    }

    /// <summary>
    /// Corrotina que simula a fase de treino/aprendizado do modelo de IA.
    /// </summary>
    private IEnumerator LearningPhaseRoutine()
    {
        isLearningMode = true;

        // Atualiza o texto para informar a fase de processamento do modelo
        if (timeAndLearningText != null)
        {
            timeAndLearningText.text = "Learning...";
        }

        float elapsedTime = 0f;

        // Anima a barra preenchendo de 0 a 1 em escala X durante 'learningDuration'
        while (elapsedTime < learningDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / learningDuration);

            if (barTime != null)
            {
                Vector3 scale = barTime.localScale;
                scale.x = progress;
                barTime.localScale = scale;
            }

            yield return null;
        }

        // Garante a barra completamente cheia no final do treino
        if (barTime != null)
        {
            Vector3 scale = barTime.localScale;
            scale.x = 1f;
            barTime.localScale = scale;
        }

        isLearningMode = false;
        Debug.Log("Fase de aprendizado/re-treino concluída com sucesso!");
    }

    /// <summary>
    /// Retorna se o sistema está atualmente ocupado processando o aprendizado da IA.
    /// </summary>
    public bool IsLearning()
    {
        return isLearningMode;
    }
}