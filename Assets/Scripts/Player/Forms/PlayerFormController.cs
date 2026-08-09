using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerFormState
{
    BaseIngredient,
    Dish
}

public enum PlayerDishEndReason
{
    Expired,
    Delivered,
    Cancelled
}

public class PlayerFormController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerIngredientInventory inventory;
    [SerializeField] private DishRecipeBookSO recipeBook;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private AbilityController abilities;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform formVisualParent;

    [Header("Forma base")]
    [SerializeField] private PlayerFormDataSO baseForm;

    private PlayerFormDataSO currentForm;
    private DishFormDataSO activeDishForm;
    private GameObject currentFormInstance;
    private float dishTimeRemaining;

    public PlayerFormState CurrentState { get; private set; } = PlayerFormState.BaseIngredient;
    public PlayerFormDataSO BaseForm => baseForm;
    public PlayerFormDataSO CurrentForm => currentForm;
    public DishFormDataSO ActiveDishForm => activeDishForm;
    public DishData CurrentDish => activeDishForm != null ? activeDishForm.Dish : null;
    public float DishTimeRemaining => dishTimeRemaining;
    public bool IsDishFormActive => CurrentState == PlayerFormState.Dish && activeDishForm != null;

    public float DishTimeNormalized
    {
        get
        {
            if (activeDishForm == null || activeDishForm.Duration <= 0f) return 0f;
            return Mathf.Clamp01(dishTimeRemaining / activeDishForm.Duration);
        }
    }

    public event Action<PlayerFormDataSO> OnFormChanged;
    public event Action<DishFormDataSO> OnDishFormStarted;
    public event Action<DishFormDataSO, PlayerDishEndReason> OnDishFormEnded;

    private void Awake()
    {
        if (inventory == null) inventory = GetComponent<PlayerIngredientInventory>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (abilities == null) abilities = GetComponent<AbilityController>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (formVisualParent == null) formVisualParent = transform;
    }

    private void Start()
    {
        if (baseForm != null)
            SetBaseForm(baseForm, true);
    }

    private void Update()
    {
        if (!IsDishFormActive) return;
        if (activeDishForm.Duration <= 0f) return;

        dishTimeRemaining -= Time.deltaTime;
        if (dishTimeRemaining <= 0f)
            EndDishForm(PlayerDishEndReason.Expired);
    }

    public bool SetBaseForm(PlayerFormDataSO form, bool applyImmediately = false)
    {
        if (form == null || form.IsDishForm) return false;

        baseForm = form;
        if (applyImmediately || CurrentState == PlayerFormState.BaseIngredient)
            ApplyForm(baseForm);

        return true;
    }

    public bool TryTransformToDish(DishData dish)
    {
        if (recipeBook == null || dish == null) return false;

        DishFormDataSO dishForm = recipeBook.FindDishForm(dish);
        return TryTransformToDish(dishForm);
    }

    public bool CanTransformToDish(DishFormDataSO dishForm)
    {
        if (dishForm == null || dishForm.Dish == null) return false;
        if (CurrentState == PlayerFormState.Dish) return false;

        return inventory != null && inventory.HasIngredients(dishForm.RequiredIngredients);
    }

    public bool CanTransformToDish(DishData dish)
    {
        if (recipeBook == null || dish == null) return false;

        return CanTransformToDish(recipeBook.FindDishForm(dish));
    }

    public bool TryTransformToDish(DishFormDataSO dishForm)
    {
        if (!CanTransformToDish(dishForm)) return false;
        if (!inventory.ConsumeIngredients(dishForm.RequiredIngredients)) return false;

        activeDishForm = dishForm;
        dishTimeRemaining = dishForm.Duration;
        CurrentState = PlayerFormState.Dish;

        ApplyForm(dishForm);
        OnDishFormStarted?.Invoke(dishForm);
        return true;
    }

    public bool ConsumeCurrentDishForDelivery()
    {
        DishData deliveredDish;
        return ConsumeCurrentDishForDelivery(out deliveredDish);
    }

    public bool ConsumeCurrentDishForDelivery(out DishData deliveredDish)
    {
        deliveredDish = CurrentDish;
        if (!IsDishFormActive) return false;

        EndDishForm(PlayerDishEndReason.Delivered);
        return true;
    }

    public void RevertToBaseForm()
    {
        if (CurrentState == PlayerFormState.Dish)
        {
            EndDishForm(PlayerDishEndReason.Cancelled);
            return;
        }

        if (baseForm != null)
            ApplyForm(baseForm);
    }

    private void EndDishForm(PlayerDishEndReason reason)
    {
        if (activeDishForm == null) return;

        DishFormDataSO endedDishForm = activeDishForm;
        activeDishForm = null;
        dishTimeRemaining = 0f;
        CurrentState = PlayerFormState.BaseIngredient;

        if (baseForm != null)
            ApplyForm(baseForm);

        OnDishFormEnded?.Invoke(endedDishForm, reason);
    }

    private void ApplyForm(PlayerFormDataSO form)
    {
        if (form == null) return;

        currentForm = form;

        if (combat != null)
        {
            combat.CancelCurrentAttack();
            combat.SetFirstAttack(form.FirstAttack);
        }

        if (abilities != null)
            abilities.ReplaceFormAbilities(form.FormAbilities);

        if (movement != null && form.OverrideMovement)
            movement.SetMovementTuning(form.Acceleration, form.MaxSpeed, form.Drag);

        ReplaceFormVisual(form);
        OnFormChanged?.Invoke(form);
    }

    private void ReplaceFormVisual(PlayerFormDataSO form)
    {
        if (currentFormInstance != null)
            Destroy(currentFormInstance);

        if (form.FormPrefab == null) return;

        currentFormInstance = Instantiate(form.FormPrefab, formVisualParent);
        currentFormInstance.transform.localPosition = Vector3.zero;
        currentFormInstance.transform.localRotation = Quaternion.identity;

        if (combat == null) return;

        Animator formAnimator = currentFormInstance.GetComponentInChildren<Animator>();
        Hitbox formHitbox = currentFormInstance.GetComponentInChildren<Hitbox>();
        combat.SetCombatReferences(formAnimator, formHitbox);
    }
}
