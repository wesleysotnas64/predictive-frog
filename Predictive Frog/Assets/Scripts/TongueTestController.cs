using UnityEngine;
using UnityEngine.InputSystem;

public class TongueTestController : MonoBehaviour
{
    [SerializeField] private FrogsTongue tongueScript;

    private void Update()
    {
        // Detecta o clique do mouse e ativa o ataque da língua
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Se a língua já estiver executando o ataque (ativa), ignora novos cliques
            if (tongueScript != null && !tongueScript.gameObject.activeInHierarchy)
            {
                Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                mouseWorldPosition.z = 0f;

                tongueScript.LaunchAttack(mouseWorldPosition);
            }
        }
    }
}