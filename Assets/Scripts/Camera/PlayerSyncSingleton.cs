using UnityEngine;

public class PlayerSyncSingleton : MonoBehaviour
{
    public static PlayerSyncSingleton Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
