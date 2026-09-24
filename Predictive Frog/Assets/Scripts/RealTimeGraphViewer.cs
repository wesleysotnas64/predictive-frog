using System.Collections.Generic;
using UnityEngine;

public class RealTimeGraphViewer : MonoBehaviour
{
    [Header("Referências dos Componentes")]
    [Tooltip("LineRenderer responsável pela curva do Eixo X (ex: Vermelho).")]
    [SerializeField] private LineRenderer lineX;

    [Tooltip("LineRenderer responsável pela curva do Eixo Y (ex: Azul).")]
    [SerializeField] private LineRenderer lineY;

    [Tooltip("Referência ao coletor de dados de série temporal.")]
    [SerializeField] private TimeSeriesDataCollector dataCollector;

    [Header("Configurações do Gráfico na Tela")]
    [Tooltip("Ponto de origem 2D onde o canto inferior esquerdo do gráfico será desenhado.")]
    [SerializeField] private Vector3 graphOrigin = new Vector3(-7f, 2.5f, 0f);

    [Tooltip("Largura do gráfico em unidades da cena.")]
    [SerializeField] private float graphWidth = 4f;

    [Tooltip("Altura do gráfico em unidades da cena.")]
    [SerializeField] private float graphHeight = 2f;

    [Header("Escala do Eixo Vertical (Arena)")]
    [Tooltip("Mínimo X/Y esperado da arena (ex: -8).")]
    [SerializeField] private float minDataValue = -8f;

    [Tooltip("Máximo X/Y esperado da arena (ex: 8).")]
    [SerializeField] private float maxDataValue = 8f;

    private void Update()
    {
        if (dataCollector == null) return;

        List<TimeSeriesDataCollector.PlayerDataPoint> dataPoints = dataCollector.GetTimeSeriesData();

        if (dataPoints == null || dataPoints.Count == 0)
        {
            ResetLines();
            return;
        }

        RenderGraph(dataPoints);
    }

    private void RenderGraph(List<TimeSeriesDataCollector.PlayerDataPoint> dataPoints)
    {
        int totalPoints = dataPoints.Count;

        if (lineX != null) lineX.positionCount = totalPoints;
        if (lineY != null) lineY.positionCount = totalPoints;

        // Trata o caso de haver apenas 1 ponto na lista para evitar divisão por zero
        if (totalPoints == 1)
        {
            float singleX = graphOrigin.x;
            float normX = Mathf.InverseLerp(minDataValue, maxDataValue, dataPoints[0].position.x);
            float normY = Mathf.InverseLerp(minDataValue, maxDataValue, dataPoints[0].position.y);

            if (lineX != null) lineX.SetPosition(0, new Vector3(singleX, graphOrigin.y + (normX * graphHeight), -0.1f));
            if (lineY != null) lineY.SetPosition(0, new Vector3(singleX, graphOrigin.y + (normY * graphHeight), -0.1f));
            return;
        }

        // Normalização baseada na quantidade total de amostras (N - 1 intervalos)
        float maxIndex = totalPoints - 1;

        for (int i = 0; i < totalPoints; i++)
        {
            var point = dataPoints[i];

            // 1. Mapeia o índice do ponto [0, maxIndex] proporcionalmente na largura [0, graphWidth]
            float normalizedProgress = i / maxIndex;
            float posX = graphOrigin.x + (normalizedProgress * graphWidth);

            // 2. Normaliza os eixos de posição do Player na arena
            float normalizedXVal = Mathf.InverseLerp(minDataValue, maxDataValue, point.position.x);
            float normalizedYVal = Mathf.InverseLerp(minDataValue, maxDataValue, point.position.y);

            float posY_XVal = graphOrigin.y + (normalizedXVal * graphHeight);
            float posY_YVal = graphOrigin.y + (normalizedYVal * graphHeight);

            // 3. Atualiza os pontos das duas linhas
            if (lineX != null) lineX.SetPosition(i, new Vector3(posX, posY_XVal, -0.1f));
            if (lineY != null) lineY.SetPosition(i, new Vector3(posX, posY_YVal, -0.1f));
        }
    }

    private void ResetLines()
    {
        if (lineX != null) lineX.positionCount = 0;
        if (lineY != null) lineY.positionCount = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Vector3 topLeft = graphOrigin + new Vector3(0, graphHeight, 0);
        Vector3 topRight = graphOrigin + new Vector3(graphWidth, graphHeight, 0);
        Vector3 bottomRight = graphOrigin + new Vector3(graphWidth, 0, 0);

        Gizmos.DrawLine(graphOrigin, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, graphOrigin);
    }
}