using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public float timeInMatch;
    public TMP_Text timer;

    string countdownFormat = "{00}:{00}";

    public void Update()
    {
        timeInMatch -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeInMatch / 60F);
        int seconds = Mathf.FloorToInt(timeInMatch - minutes * 60);

        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        timer.text = niceTime;
    }

    public static float GetMagnitude(float3 vector)
    {
        return Mathf.Sqrt(Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.y, 2) + Mathf.Pow(vector.z, 2));
    }
}
