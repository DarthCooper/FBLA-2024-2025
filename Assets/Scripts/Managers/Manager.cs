using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    public int frameRate = 60;

    public float timeInMatch;
    public TMP_Text timer;

    public void Start()
    {
        Application.targetFrameRate = frameRate;
    }

    public void Awake()
    {
        instance = this;

        EventManagerSystem events = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EventManagerSystem>();
        events.OnEndLevel += EndLevel;
    }

    public void OnDisable()
    {
        if (World.DefaultGameObjectInjectionWorld == null) { return; }
        EventManagerSystem events = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EventManagerSystem>();
        events.OnEndLevel -= EndLevel;
    }

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

    public void EndLevel(int index)
    {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
        Debug.Log(index);
    }
}
