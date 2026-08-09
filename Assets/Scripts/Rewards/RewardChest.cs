using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RewardChest : MonoBehaviour
{
    [Header("Configuracion de Ingredientes")]
    [SerializeField] private IngredientPoolSO ingredientPool;
    [SerializeField] private int optionsCount = 3;

    [Header("Interaccion")]
    [SerializeField] private bool startInteractable = true;
    [SerializeField] private bool destroyAfterOpen = true;

    private Collider2D interactionCollider;
    private bool isInteractable;
    private bool opened;

    private void Awake()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionCollider.isTrigger = true;
        SetInteractable(startInteractable);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable || opened) return;
        if (!IsPlayer(other)) return;

        opened = true;
        OpenChest();
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;

        if (value)
            opened = false;

        if (interactionCollider != null)
            interactionCollider.enabled = value;
    }

    public void SetDestroyAfterOpen(bool value)
    {
        destroyAfterOpen = value;
    }

    private void OpenChest()
    {
        if (ingredientPool == null)
        {
            Debug.LogError("[RewardChest] Falta asignar el IngredientPoolSO en el inspector del cofre.");
            opened = false;
            return;
        }

        List<IngredientData> randomIngredients = ingredientPool.GetRandomIngredients(optionsCount);

        IngredientSelectionUI uiInstance = IngredientSelectionUI.Instance;
        if (uiInstance == null)
            uiInstance = FindFirstObjectByType<IngredientSelectionUI>(FindObjectsInactive.Include);

        if (uiInstance == null)
        {
            Debug.LogError("[RewardChest] No se encontro ningun IngredientSelectionUI en la escena.");
            opened = false;
            return;
        }

        uiInstance.ShowSelection(randomIngredients);
        SetInteractable(false);

        if (destroyAfterOpen)
            Destroy(gameObject, 0.05f);
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player")) return true;
        if (other.GetComponentInParent<PlayerIngredientInventory>() != null) return true;
        if (other.GetComponentInParent<PlayerCombat>() != null) return true;
        return other.GetComponentInParent<DummyPlayer>() != null;
    }
}
