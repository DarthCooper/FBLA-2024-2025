using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceUIController : MonoBehaviour
{
    public Image playerHealthBar;

    public Image meleeDelayVisual;
    public Image rangedDelayVisual;

    private void OnEnable()
    {
        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.showHealth += ShowPlayerHealth;
        playerUISystem.showMeleeDelay += ShowMeleeDelay;
        playerUISystem.showRangedDelay += ShowRangeDelay;
    }

    private void OnDisable()
    {
        if(World.DefaultGameObjectInjectionWorld == null) { return; }
        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.showHealth -= ShowPlayerHealth;
        playerUISystem.showMeleeDelay -= ShowMeleeDelay;
        playerUISystem.showRangedDelay -= ShowRangeDelay;
    }

    public void ShowPlayerHealth(float health, float maxHealth)
    {
        playerHealthBar.fillAmount = health / maxHealth;
    }

    public void ShowMeleeDelay(float delay, float maxDelay)
    {
        meleeDelayVisual.fillAmount = delay / maxDelay;
    }

    public void ShowRangeDelay(float delay, float maxDelay)
    {
        rangedDelayVisual.fillAmount = delay / maxDelay;
    }
}
