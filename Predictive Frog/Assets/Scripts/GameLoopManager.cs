using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameLoopManager : MonoBehaviour
{
    public enum GameState
    {
        WaitingToStart, // Aguardando o jogador pressionar Enter para o próximo round
        RoundActive,    // Round em andamento (10s de coleta)
        LearningPhase,  // Fase de aprendizado da IA (3s)
        GameOver        // Fim de jogo
    }

    [Header("Referências de Gerenciadores")]
    [SerializeField] private CandySpawnController candySpawner;
    [SerializeField] private TimeSeriesDataCollector dataCollector;
    [SerializeField] private UIController uiController;

    [Header("Referência do Player")]
    [SerializeField] private Transform playerTransform;

    [Header("Configurações do Respawn")]
    [SerializeField] private Vector3 playerStartPosition = new Vector3(0f, -4f, 0f);

    [Header("Estado do Jogo (Read-Only)")]
    [SerializeField] private GameState currentState = GameState.WaitingToStart;

    public static GameLoopManager Instance { get; private set; }

    private void Awake()
    {
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
        // Encontra o player caso não tenha sido atribuído no Inspector
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        SetupInitialState();
    }

    private void Update()
    {
        // Só permite pressionar Enter se o jogo estiver EXATAMENTE no estado WaitingToStart
        if (currentState == GameState.WaitingToStart)
        {
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                StartNextRound();
            }
        }
    }

    /// <summary>
    /// Prepara a cena para a primeira inicialização (Round 0).
    /// </summary>
    private void SetupInitialState()
    {
        currentState = GameState.WaitingToStart;

        ResetPlayerPosition();

        if (candySpawner != null)
        {
            candySpawner.ClearAllCandies();
            // candySpawner.SpawnDefaultAmount(); // Spawna doces já na tela inicial para o jogador visualizar
        }

        if (uiController != null)
        {
            uiController.UpdateRoundUI(0f, 10f, 0);
            uiController.ShowPressEnterMessage(0); // Exibe "Pressione ENTER para iniciar o Round 0"
        }

        Debug.Log("===> JOGO PRONTO! Pressione ENTER para iniciar o Round 0. <===");
    }

    /// <summary>
    /// Inicia o round ativo (Round 0, Round 1, etc.).
    /// </summary>
    private void StartNextRound()
    {
        currentState = GameState.RoundActive;

        // Oculta a mensagem central ao iniciar a partida
        if (uiController != null)
        {
            uiController.HidePressEnterMessage();
        }

        // 1. Garante que haja doces na tela (caso não tenham sido spawnados)
        if (candySpawner != null)
        {
            candySpawner.SpawnDefaultAmount();
        }

        // 2. Inicia o coletor de dados
        if (dataCollector != null)
        {
            dataCollector.StartRoundManually();
        }

        // 3. SAPO: Inicia ataques apenas do Round 1 em diante
        int currentRound = dataCollector != null ? dataCollector.GetCurrentRound() : 0;
        if (currentRound > 0 && FrogController.Instance != null)
        {
            FrogController.Instance.StartAttackingRoutine();
        }

        Debug.Log($"===> Round {currentRound} Iniciado! Movimentação Liberada! <===");
    }

    /// <summary>
    /// Método chamado quando os 10 segundos do round expiram.
    /// Transita para a fase de aprendizado (3s).
    /// </summary>
    public void OnRoundFinished()
    {
        // Interrompe imediatamente qualquer ataque em andamento do sapo
        if (FrogController.Instance != null)
        {
            FrogController.Instance.StopAttackingRoutine();
        }

        StartCoroutine(LearningRoutine());
    }

    private IEnumerator LearningRoutine()
    {
        currentState = GameState.LearningPhase;
        Debug.Log("===> Entrando na Fase de Aprendizado (3s). Movimentação Bloqueada! <===");

        // Exibe uma das 10 frases aleatórias do Sapo na UI
        if (uiController != null)
        {
            uiController.ShowFrogLearningDialogue();
        }

        // Aguarda os 3 segundos da simulação de aprendizado
        yield return new WaitForSeconds(uiController.learningDuration);

        // Ao finalizar o aprendizado:
        // 1. Reposiciona o jogador em (0, -4)
        ResetPlayerPosition();

        // 2. Prepara os doces do próximo round para o jogador visualizar na tela de espera
        if (candySpawner != null)
        {
            candySpawner.ClearAllCandies();
            // candySpawner.SpawnDefaultAmount();
        }

        // 3. Muda o estado para WaitingToStart
        currentState = GameState.WaitingToStart;

        int nextRound = dataCollector != null ? dataCollector.GetCurrentRound() : 0;

        // Exibe novamente a mensagem de pressionar ENTER para o próximo round
        if (uiController != null)
        {
            uiController.ShowPressEnterMessage(nextRound);
        }

        Debug.Log($"===> Aprendizado Concluído! Pressione ENTER para iniciar o Round {nextRound}. <===");
    }

    public void ResetPlayerPosition()
    {
        if (playerTransform != null)
        {
            playerTransform.position = playerStartPosition;
        }
    }

    /// <summary>
    /// Retorna verdadeiro APENAS durante o round ativo (RoundActive).
    /// Bloqueia a movimentação durante o WaitingToStart e LearningPhase.
    /// </summary>
    public bool CanPlayerMove()
    {
        return currentState == GameState.RoundActive;
    }

    public GameState GetCurrentState() => currentState;
}