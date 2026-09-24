using UnityEngine;

public class EyePupilController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Transform do jogador (Formiga) para a pupila seguir.")]
    [SerializeField] private Transform playerTarget;

    [Header("Configurações do Olho")]
    [Tooltip("Distância máxima (em unidades da Unity) que a pupila pode se mover a partir do centro do olho.")]
    [SerializeField] private float maxPupilOffset = 0.3f;

    [Tooltip("Velocidade de suavização do movimento da pupila.")]
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 eyeCenterPosition; // Posição inicial (centro do olho)

    private void Start()
    {
        // Define a posição inicial local/mundo do centro do olho
        eyeCenterPosition = transform.position;

        // Tenta encontrar o player pela tag se não for atribuído no Inspector
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTarget == null) return;

        // 1. Calcula o vetor de direção do centro do olho até o jogador
        Vector3 directionToPlayer = playerTarget.position - eyeCenterPosition;
        directionToPlayer.z = 0f; // Mantém no plano 2D

        // 2. Calcula a distância real e o vetor unitário de direção
        float currentDistance = directionToPlayer.magnitude;
        Vector3 normalizedDirection = directionToPlayer.normalized;

        // 3. Aplica o escalar: limita a distância ao valor de 'maxPupilOffset'
        float clampedDistance = Mathf.Min(currentDistance, maxPupilOffset);

        // 4. Determina a posição de destino da pupila
        Vector3 targetPupilPosition = eyeCenterPosition + (normalizedDirection * clampedDistance);

        // 5. Move a pupila suavemente em direção à posição calculada
        transform.position = Vector3.Lerp(transform.position, targetPupilPosition, Time.deltaTime * smoothSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha o limite do olho na janela de Scene para facilitar o ajuste do limite (maxPupilOffset)
        Vector3 center = Application.isPlaying ? eyeCenterPosition : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, maxPupilOffset);
    }
}