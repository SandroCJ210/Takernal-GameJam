using UnityEngine;

/// <summary>
/// Contiene los detalles y el resultado de la evaluación al entregar un platillo a un cliente.
/// </summary>
public struct DeliveryResult
{
    public DishData Dish;
    public CustomerInstance Customer;
    public int Score;
    public bool IsValid => Dish != null && Customer != null;
    public bool IsSuccess => Score > 0;
    public bool IsNeutral => Score == 0;
    public bool IsPenalty => Score < 0;

    public DeliveryResult(DishData dish, CustomerInstance customer, int score)
    {
        Dish = dish;
        Customer = customer;
        Score = score;
    }
}
