using System.Collections;
using UnityEngine;

public class FrogsTongue : MonoBehaviour
{
    [Header("Configurações do Ataque")]
    [Tooltip("Tempo em segundos que a língua leva para ir, parar e voltar.")]
    [SerializeField] private float timeSpeed = 0.4f;

    [Header("Referências Visuais")]
    [SerializeField] private LineRenderer tongueLine;
    [SerializeField] private Transform mouthTransform;

    private Vector3 initialPosition;
    private Vector3 destination;


    private void Awake()
    {
        // Salva a posição inicial estática na boca do sapo
        initialPosition = transform.position;
    }

    /// <summary>
    /// Ativa a língua e inicia a sequência de ataque.
    /// </summary>
    public void LaunchAttack(Vector3 targetPosition)
    {
        destination = targetPosition;
        
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
            mouthTransform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        }

        // 1. FASE DE IDA
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

        // 2. FASE DE PAUSA (IMPACTO)
        yield return new WaitForSeconds(timeSpeed);

        // 3. FASE DE VOLTA
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
        gameObject.SetActive(false);
        mouthTransform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
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
        
    }
}