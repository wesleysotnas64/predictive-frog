using System.Collections;
using UnityEngine;

public class FrogsTongue : MonoBehaviour
{
    [Header("Configurações do Ataque")]
    [Tooltip("Tempo em segundos que a língua leva para ir, parar e voltar.")]
    [SerializeField] private float timeSpeed = 0.1f;

    [Header("Referências Visuais e Componentes")]
    [SerializeField] private LineRenderer tongueLine;
    [SerializeField] private Transform mouthTransform;
    [SerializeField] private CircleCollider2D tongueCollider;

    private Vector3 initialPosition;
    private Vector3 destination;
    private bool hasHitPlayerThisAttack = false;

    private void Awake()
    {
        // Salva a posição inicial estática na boca do sapo
        initialPosition = transform.position;

        // Se o colisor não foi atribuído via Inspector, tenta obter automaticamente
        if (tongueCollider == null)
        {
            tongueCollider = GetComponent<CircleCollider2D>();
        }

        // Garante que o colisor inicie desativado
        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }
    }

    /// <summary>
    /// Ativa a língua e inicia a sequência de ataque.
    /// </summary>
    public void LaunchAttack(Vector3 targetPosition)
    {
        destination = targetPosition;
        hasHitPlayerThisAttack = false; // Reseta a flag para o novo ataque
        
        // Desativa o colisor preventivamente no início do lançamento
        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }

        // Garante que o GameObject esteja ativo antes de iniciar a corrotina
        gameObject.SetActive(true);
        StartCoroutine(TongueAttackRoutine());
    }

    private IEnumerator TongueAttackRoutine()
    {
        // Configura o LineRenderer no início do ataque
        if (tongueLine != null)
        {
            tongueLine.positionCount = 2;
            tongueLine.SetPosition(0, initialPosition);
            tongueLine.SetPosition(1, initialPosition);
            tongueLine.enabled = true;
            if (mouthTransform != null) mouthTransform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        }

        // 1. FASE DE IDA (SEM DANO - Colisor desativado)
        float elapsedTime = 0f;
        while (elapsedTime < timeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeSpeed;
            transform.position = Vector3.Lerp(initialPosition, destination, t);
            
            UpdateLineRenderer();
            yield return null;
        }

        transform.position = destination;
        UpdateLineRenderer();
        OnTongueHitTarget();

        // 2. FASE DE PAUSA / IMPACTO (COM DANO - Colisor ativo)
        if (tongueCollider != null)
        {
            tongueCollider.enabled = true;
        }

        yield return new WaitForSeconds(timeSpeed);

        // Desativa o colisor logo ao término da fase de impacto
        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }

        // 3. FASE DE VOLTA (SEM DANO - Colisor desativado)
        elapsedTime = 0f;
        while (elapsedTime < timeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeSpeed;
            transform.position = Vector3.Lerp(destination, initialPosition, t);
            
            UpdateLineRenderer();
            yield return null;
        }

        // Reseta posição e esconde a linha
        transform.position = initialPosition;
        UpdateLineRenderer();
        
        if (tongueLine != null)
        {
            tongueLine.enabled = false;
        }

        // Desativa o GameObject ao finalizar todo o ciclo
        if (mouthTransform != null) mouthTransform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        gameObject.SetActive(false);
    }

    private void UpdateLineRenderer()
    {
        if (tongueLine != null)
        {
            tongueLine.SetPosition(0, initialPosition);
            tongueLine.SetPosition(1, transform.position);
        }
    }

    private void OnTongueHitTarget()
    {
        // Método mantido para eventos adicionais no ponto de impacto, se necessário
    }

    #region Colisões

    // Para projetos 2D (Collider2D e Rigidbody2D)
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject);
    }

    // Para projetos 3D (Collider e Rigidbody)
    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerCollision(other.gameObject);
    }

    /// <summary>
    /// Processa o impacto com o jogador, subtrai uma vida e reseta sua posição.
    /// </summary>
    private void HandlePlayerCollision(GameObject hitObject)
    {
        // Garante que atinja apenas o Player e apenas 1 vez por ataque
        if (!hasHitPlayerThisAttack && hitObject.CompareTag("Player"))
        {
            hasHitPlayerThisAttack = true;

            // 1. Reduz 1 vida no GameController
            if (GameController.Instance != null)
            {
                GameController.Instance.LoseLife();
            }

            // 2. Reseta a posição do player via GameLoopManager
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.ResetPlayerPosition();
            }
            else
            {
                // Fallback caso acesse diretamente a transform do player
                hitObject.transform.position = new Vector3(0f, -4f, 0f);
            }

            Debug.Log("Língua atingiu a formiga no momento de impacto! -1 Vida e posição resetada.");
        }
    }

    #endregion
}