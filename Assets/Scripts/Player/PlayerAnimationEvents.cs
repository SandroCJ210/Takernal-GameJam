using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerCombat combat;

    private void Awake()
    {
        if (combat == null)
            combat = GetComponentInParent<PlayerCombat>();
    }

    public void AE_ActivateHitbox()
    {
        if (combat != null)
            combat.AE_ActivateHitbox();
    }

    public void AE_LungeForward()
    {
        if (combat != null)
            combat.AE_LungeForward();
    }

    public void AE_OpenComboWindow()
    {
        if (combat != null)
            combat.AE_OpenComboWindow();
    }

    public void AE_CloseComboWindow()
    {
        if (combat != null)
            combat.AE_CloseComboWindow();
    }

    public void AE_AttackEnd()
    {
        if (combat != null)
            combat.AE_AttackEnd();
    }
}
