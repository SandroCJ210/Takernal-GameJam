using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RewardChest : MonoBehaviour
{
    [Header("Configuración de Ingredientes")]
    [SerializeField] private IngredientPoolSO ingredientPool;
    [SerializeField] private int optionsCount = 3;

    private bool opened = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened) return;

        // Detecta al jugador ya sea por Tag "Player" o por tener DummyPlayer
        if (other.CompareTag("Player") || other.GetComponent<DummyPlayer>() != null)
        {
            opened = true;
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (ingredientPool == null)
        {
            Debug.LogError("[RewardChest] ¡Falta asignar el IngredientPoolSO en el inspector del cofre!");
            return;
        }

        List<IngredientData> randomIngredients = ingredientPool.GetRandomIngredients(optionsCount);

        IngredientSelectionUI uiInstance = IngredientSelectionUI.Instance;
        if (uiInstance == null)
        {
            uiInstance = FindFirstObjectByType<IngredientSelectionUI>(FindObjectsInactive.Include);
        }

        if (uiInstance != null)
        {
            uiInstance.ShowSelection(randomIngredients);
        }
        else
        {
            Debug.LogError("[RewardChest] No se encontró ningún IngredientSelectionUI en la escena.");
        }

        Destroy(gameObject, 0.05f);
    }

}
