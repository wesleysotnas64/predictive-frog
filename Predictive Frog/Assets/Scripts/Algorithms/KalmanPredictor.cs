using UnityEngine;

/// <summary>
/// Singleton que gerencia o Filtro de Kalman 2D para estimativa de posição,
/// velocidade (x, y, vx, vy) e predição de posição futura.
/// </summary>
public class KalmanPredictor : MonoBehaviour
{
    public static KalmanPredictor Instance { get; private set; }

    [Header("Parâmetros do Filtro")]
    [SerializeField] private float dt = 0.1f; // Amostragem a 10Hz
    [SerializeField] private float processNoise = 0.1f; // Q
    [SerializeField] private float measurementNoise = 0.05f; // R

    // Vetor de Estado (x, y, vx, vy)
    private float x, y, vx, vy;

    // Matriz de Covariância de Erro P (4x4)
    private float[,] P = new float[4, 4];

    private bool isInitialized = false;

    private void Awake()
    {
        // Padrão Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Opcional: Se quiser que o filtro persista entre cenas desmantele o comentário abaixo
        // DontDestroyOnLoad(gameObject);

        ResetFilter();
    }

    /// <summary>
    /// Reinicia o filtro ao começar um novo round.
    /// </summary>
    public void ResetFilter()
    {
        x = y = vx = vy = 0f;
        P = new float[4, 4];

        // Inicializa a matriz P com alta incerteza
        for (int i = 0; i < 4; i++)
        {
            P[i, i] = 10f;
        }

        isInitialized = false;
    }

    /// <summary>
    /// Recebe a posição (x, y) capturada pelo TimeSeriesDataCollector e atualiza o filtro.
    /// </summary>
    public void UpdateState(Vector2 measuredPos)
    {
        if (!isInitialized)
        {
            x = measuredPos.x;
            y = measuredPos.y;
            vx = 0f;
            vy = 0f;
            isInitialized = true;
            return;
        }

        // ==================== 1. PREDIÇÃO ====================
        float x_pred = x + vx * dt;
        float y_pred = y + vy * dt;
        float vx_pred = vx;
        float vy_pred = vy;

        float[,] P_pred = PredictCovariance(P, processNoise, dt);

        // ==================== 2. ATUALIZAÇÃO ====================
        float y_x = measuredPos.x - x_pred;
        float y_y = measuredPos.y - y_pred;

        float S00 = P_pred[0, 0] + measurementNoise;
        float S01 = P_pred[0, 1];
        float S10 = P_pred[1, 0];
        float S11 = P_pred[1, 1] + measurementNoise;

        float detS = S00 * S11 - S01 * S10;
        if (Mathf.Abs(detS) < 1e-6f) return;

        float invS00 = S11 / detS;
        float invS01 = -S01 / detS;
        float invS10 = -S10 / detS;
        float invS11 = S00 / detS;

        float[,] K = new float[4, 2];
        K[0, 0] = P_pred[0, 0] * invS00 + P_pred[0, 1] * invS10;
        K[0, 1] = P_pred[0, 0] * invS01 + P_pred[0, 1] * invS11;

        K[1, 0] = P_pred[1, 0] * invS00 + P_pred[1, 1] * invS10;
        K[1, 1] = P_pred[1, 0] * invS01 + P_pred[1, 1] * invS11;

        K[2, 0] = P_pred[2, 0] * invS00 + P_pred[2, 1] * invS10;
        K[2, 1] = P_pred[2, 0] * invS01 + P_pred[2, 1] * invS11;

        K[3, 0] = P_pred[3, 0] * invS00 + P_pred[3, 1] * invS10;
        K[3, 1] = P_pred[3, 0] * invS01 + P_pred[3, 1] * invS11;

        x = x_pred + K[0, 0] * y_x + K[0, 1] * y_y;
        y = y_pred + K[1, 0] * y_x + K[1, 1] * y_y;
        vx = vx_pred + K[2, 0] * y_x + K[2, 1] * y_y;
        vy = vy_pred + K[3, 0] * y_x + K[3, 1] * y_y;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float term0 = (i == 0 ? 1f : 0f) - K[i, 0];
                float term1 = (i == 1 ? 1f : 0f) - K[i, 1];
                float term2 = (i == 2 ? 1f : 0f);
                float term3 = (i == 3 ? 1f : 0f);

                P[i, j] = term0 * P_pred[0, j] + term1 * P_pred[1, j] + term2 * P_pred[2, j] + term3 * P_pred[3, j];
            }
        }
    }

    /// <summary>
    /// Prevê a posição futura do jogador no tempo lookAheadTime (ex: 0.1s).
    /// </summary>
    public Vector2 PredictFuturePosition(float lookAheadTime = 0.1f)
    {
        if (!isInitialized) return Vector2.zero;

        float futureX = x + vx * lookAheadTime;
        float futureY = y + vy * lookAheadTime;

        return new Vector2(futureX, futureY);
    }

    /// <summary>
    /// Retorna a velocidade estimada atual (útil para debugs e Gizmos)
    /// </summary>
    public Vector2 GetEstimatedVelocity()
    {
        return new Vector2(vx, vy);
    }

    private float[,] PredictCovariance(float[,] p, float q, float dt)
    {
        float[,] pNext = new float[4, 4];

        pNext[0, 0] = p[0, 0] + dt * (p[2, 0] + p[0, 2]) + dt * dt * p[2, 2] + q;
        pNext[0, 1] = p[0, 1] + dt * p[2, 1] + dt * p[0, 3] + dt * dt * p[2, 3];
        pNext[0, 2] = p[0, 2] + dt * p[2, 2];
        pNext[0, 3] = p[0, 3] + dt * p[2, 3];

        pNext[1, 0] = p[1, 0] + dt * p[3, 0] + dt * p[1, 2] + dt * dt * p[3, 2];
        pNext[1, 1] = p[1, 1] + dt * (p[3, 1] + p[1, 3]) + dt * dt * p[3, 3] + q;
        pNext[1, 2] = p[1, 2] + dt * p[3, 2];
        pNext[1, 3] = p[1, 3] + dt * p[3, 3];

        pNext[2, 0] = p[2, 0] + dt * p[2, 2];
        pNext[2, 1] = p[2, 1] + dt * p[2, 3];
        pNext[2, 2] = p[2, 2] + q;
        pNext[2, 3] = p[2, 3];

        pNext[3, 0] = p[3, 0] + dt * p[3, 2];
        pNext[3, 1] = p[3, 1] + dt * p[3, 3];
        pNext[3, 2] = p[3, 2];
        pNext[3, 3] = p[3, 3] + q;

        return pNext;
    }
}