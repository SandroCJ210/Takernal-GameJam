using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class InputHandler : StaticInstance<InputHandler>
{
    public UnityAction<Vector2> OnMoveRecieved;
    public UnityAction OnAttackRecieved;
    public UnityAction OnAbility1Recieved;
    public UnityAction OnUltimateRecieved;

    public void OnMove(InputValue value)
    {
        OnMoveRecieved?.Invoke(value.Get<Vector2>());
    }
    
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
            OnAttackRecieved?.Invoke();
    }
 
    public void OnAbility1(InputValue value)
    {
        if (value.isPressed)
            OnAbility1Recieved?.Invoke();
    }

    public void OnUltimate(InputValue value)
    {
        if (value.isPressed)
            OnUltimateRecieved?.Invoke();
    }
}
