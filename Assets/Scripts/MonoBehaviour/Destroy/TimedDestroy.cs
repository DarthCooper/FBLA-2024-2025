using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float timer;

    void Update()
    {
        if(timer < 0 )
        {
            Destroy(gameObject);
        }
        timer -= Time.deltaTime;
    }
}
