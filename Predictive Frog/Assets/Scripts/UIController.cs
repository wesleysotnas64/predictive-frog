using System.Collections;
using System.Collections.Generic;
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

    [Tooltip("Texto MeshPro para pontuação (ex: '0 pts').")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("Texto MeshPro para vidas (ex: '3 ants').")]
    [SerializeField] private TextMeshProUGUI livesText;

    [Tooltip("Texto MeshPro dinâmico para indicar início de round e falas do Sapo.")]
    [SerializeField] private TextMeshProUGUI pressEnterText;

    [Header("Configurações Acadêmicas/Animação")]
    [Tooltip("Tempo em segundos reservado para a simulação de aprendizado (re-treino da IA).")]
    [SerializeField] public float learningDuration;

    private bool isLearningMode = false;

    [Header("Diálogos do Sapo (Learning Phase)")]
    private readonly string[] frogDialoguesMaster = new string[]
    {
        "Acha que pode escapar de mim? Estou analisando seus passos!",
        "Hmm... você gosta de dobrar à esquerda, né? Anotado!",
        "Processando seus dados... Sua tática não vai funcionar por muito tempo!",
        "Estou calculando sua trajetória exata. Nenhuma formiga me engana!",
        "Estudando seus movimentos... Você é bem previsível, sabia?",
        "Carregando habilidades de caça... 99% concluído!",
        "Analisando padrão de corrida... Próxima rodada você vira sobremesa!",
        "Hummm, dados saborosos! Estou aprendendo cada truque seu.",
        "Seu histórico de navegação diz que você vai para a direita... Acertei?",
        "Ajustando minha pontaria... Da próxima vez, minha língua não erra!"
    };

    // Lista auxiliar para controlar o sorteio sem repetição
    private List<string> remainingDialogues = new List<string>();

    private void Awake()
    {
        ResetDialogueBag();
    }

    /// <summary>
    /// Repovoa a lista com todas as frases quando o "saco de sorteio" esvazia.
    /// </summary>
    private void ResetDialogueBag()
    {
        remainingDialogues.Clear();
        remainingDialogues.AddRange(frogDialoguesMaster);
    }

    /// <summary>
    /// Sorteia e exibe uma frase sem repetição. 
    /// Reseta e embaralha o conjunto apenas após todas as 10 serem exibidas.
    /// </summary>
    public void ShowFrogLearningDialogue()
    {
        if (pressEnterText == null) return;

        // Se todas as frases já foram exibidas, repovoa o saco de sorteio
        if (remainingDialogues.Count == 0)
        {
            ResetDialogueBag();
            Debug.Log("===> Todas as frases do Sapo foram exibidas! Reiniciando ciclo de frases. <===");
        }

        // Sorteia um índice da lista de frases restantes
        int randomIndex = Random.Range(0, remainingDialogues.Count);
        string selectedDialogue = remainingDialogues[randomIndex];

        // Remove a frase sorteada para não repetir
        remainingDialogues.RemoveAt(randomIndex);

        // Exibe na UI
        pressEnterText.text = $"Sapo: \"{selectedDialogue}\"";
        pressEnterText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Exibe a mensagem solicitando pressionar ENTER para iniciar o próximo round.
    /// </summary>
    public void ShowPressEnterMessage(int nextRound)
    {
        if (pressEnterText != null)
        {
            pressEnterText.text = $"Pressione ENTER para iniciar o Round {nextRound}";
            pressEnterText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Esconde a mensagem central quando o round ativo começa.
    /// </summary>
    public void HidePressEnterMessage()
    {
        if (pressEnterText != null)
        {
            pressEnterText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Atualiza o texto da pontuação no formato "X pts".
    /// </summary>
    public void UpdateScoreUI(int points)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{points} pts";
        }
    }

    /// <summary>
    /// Atualiza o texto de vidas no formato "X ants".
    /// </summary>
    public void UpdateLivesUI(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"{lives} ants";
        }
    }

    /// <summary>
    /// Atualiza a barra, o texto de tempo e o número do round atual.
    /// </summary>
    public void UpdateRoundUI(float currentRoundTimer, float totalRoundTime, int currentRound)
    {
        if (roundText != null)
        {
            roundText.text = $"RND: {currentRound}";
        }

        if (isLearningMode) return;

        float timeRemaining = Mathf.Max(0f, totalRoundTime - currentRoundTimer);

        if (timeAndLearningText != null)
        {
            timeAndLearningText.text = $"{timeRemaining:F2}s";
        }

        float timeRatio = Mathf.Clamp01(timeRemaining / totalRoundTime);

        if (barTime != null)
        {
            Vector3 scale = barTime.localScale;
            scale.x = timeRatio;
            barTime.localScale = scale;
        }

        if (timeRemaining <= 0f)
        {
            StartCoroutine(LearningPhaseRoutine());
        }
    }

    private IEnumerator LearningPhaseRoutine()
    {
        isLearningMode = true;

        if (timeAndLearningText != null)
        {
            timeAndLearningText.text = "Learning...";
        }

        float elapsedTime = 0f;

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

        if (barTime != null)
        {
            Vector3 scale = barTime.localScale;
            scale.x = 1f;
            barTime.localScale = scale;
        }

        isLearningMode = false;

        if (timeAndLearningText != null)
        {
            timeAndLearningText.text = "Ready!";
        }
    }

    public bool IsLearning() => isLearningMode;
}