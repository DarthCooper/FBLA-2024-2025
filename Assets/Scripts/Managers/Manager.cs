using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    public int frameRate = 60;

    public float timeInMatch;
    public TMP_Text timer;

    public Animator fadeAnim;

    public SubScene[] assets;

    public void Start()
    {
        Application.targetFrameRate = frameRate;
    }

    public void Awake()
    {
        instance = this;

        EventManagerSystem events = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EventManagerSystem>();
        events.OnEndLevel += EndLevel;

        fadeAnim.SetTrigger("FadeIn");
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

        //timer.text = niceTime;
    }

    public static float GetMagnitude(float3 vector)
    {
        return Mathf.Sqrt(Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.y, 2) + Mathf.Pow(vector.z, 2));
    }

    public void EndLevel(int index)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var ecb = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntityCommandBufferSystem>().CreateCommandBuffer();
        SceneLoaderSystem sceneLoader = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SceneLoaderSystem>();
        sceneLoader.subScene = FindAnyObjectByType<SubScene>();
        var newLevel = ecb.CreateEntity();
        ecb.AddComponent(newLevel, new SceneLoader
        {
            SceneReference = new Unity.Entities.Serialization.EntitySceneReference(assets[index].SceneGUID, 0)
        });
        ecb.AddComponent<UnloadCurScene>(newLevel);

        fadeAnim.SetTrigger("FadeOut");
        //SceneManager.LoadScene(index, LoadSceneMode.Single);
    }
}
