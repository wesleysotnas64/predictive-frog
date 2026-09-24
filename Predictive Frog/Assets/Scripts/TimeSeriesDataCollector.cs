using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeSeriesDataCollector : MonoBehaviour
{
    [Serializable]
    public struct PlayerDataPoint
    {
        public float timeStamp;  // (currentRound * roundTime) + tempoAtualDoRound
        public Vector2 position; // Coordenadas (X, Y) do Player

        public PlayerDataPoint(float timeStamp, Vector2 position)
        {
            this.timeStamp = timeStamp;
            this.position = position;
        }
    }

    [Header("Referências")]
    [Tooltip("Transform do Player (Formiga) para capturar a posição.")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private UIController uiController;

    [Header("Configurações do Round e Amostragem")]
    [Tooltip("Tempo total de cada round em segundos (Ex: 10s).")]
    [SerializeField] private float roundTime = 10f;

    [Tooltip("Intervalo de tempo entre cada captura de posição em segundos (Ex: 0.1s para 10Hz).")]
    [SerializeField] private float detectTime = 0.1f;

    [Header("Estado Atual (Read-Only)")]
    [SerializeField] private int currentRound = 0;
    [SerializeField] private bool isRoundActive = false;
    [SerializeField] private float currentRoundTimer = 0f;

    [Header("Base de Dados de Séries Temporais")]
    [SerializeField] private List<PlayerDataPoint> timeSeriesData = new List<PlayerDataPoint>();

    private void Start()
    {
        // Tenta encontrar o Player pela Tag caso não seja atribuído no Inspector
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        // Inicia o próximo round apenas ao pressionar 'T' e se não houver um round em andamento
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (!isRoundActive)
            {
                StartCoroutine(StartRoundRoutine());
            }
        }
    }

    private IEnumerator StartRoundRoutine()
    {
        isRoundActive = true;
        currentRoundTimer = 0f;

        float nextSampleTime = 0f;

        while (currentRoundTimer < roundTime)
        {
            currentRoundTimer += Time.deltaTime;

            if (uiController != null)
            {
                uiController.UpdateRoundUI(currentRoundTimer, roundTime, currentRound);
            }

            // Garante que a amostragem ocorra com a precisão de 'detectTime' (ex: a cada 0.1s)
            if (currentRoundTimer >= nextSampleTime)
            {
                CaptureDataPoint();
                nextSampleTime += detectTime;
            }

            // Garante a chamada final ao zerar o tempo para disparar o acionamento do "Learning"
            if (uiController != null)
            {
                uiController.UpdateRoundUI(currentRoundTimer, roundTime, currentRound);
            }

            yield return null; // Aguarda o próximo frame
        }

        // Garante a captura no exato momento final do round
        CaptureDataPoint();

        // Incrementa o contador de rounds e libera para a próxima rodada
        currentRound++;
        isRoundActive = false;

        // Notifica o GameLoopManager que o round acabou para iniciar o modo Learning
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.OnRoundFinished();
        }
    }

    private void CaptureDataPoint()
    {
        if (playerTransform == null) return;

        // Fórmula solicitada: (currentRound * roundTime) + tempoAtualDoRound
        float globalTimeStamp = (currentRound * roundTime) + Mathf.Min(currentRoundTimer, roundTime);
        Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.y);

        // Adiciona a amostra à lista
        PlayerDataPoint dataPoint = new PlayerDataPoint(globalTimeStamp, playerPos);
        timeSeriesData.Add(dataPoint);
    }

    /// <summary>
    /// Permite que o GameLoopManager inicie o round via código.
    /// </summary>
    public void StartRoundManually()
    {
        if (!isRoundActive)
        {
            StartCoroutine(StartRoundRoutine());
        }
    }

    /// <summary>
    /// Retorna a lista completa com todo o histórico acumulado da série temporal.
    /// </summary>
    public List<PlayerDataPoint> GetTimeSeriesData()
    {
        return timeSeriesData;
    }

    /// <summary>
    /// Retorna o número do round atual (0 = Round 1 de calibração).
    /// </summary>
    public int GetCurrentRound()
    {
        return currentRound;
    }

    /// <summary>
    /// Informa se o jogo está atualmente gravando um round.
    /// </summary>
    public bool IsRoundActive()
    {
        return isRoundActive;
    }
}