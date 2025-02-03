using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneECSManager : MonoBehaviour
{
    public Animator Scene1to2Transition;
    public bool Scene1to2TransitionComplete = false;
    public Animator Scene2to3Transition;
    public bool Scene2to3TransitionComplete = false;

    void Awake()
    {
        SceneLoaderSystem sceneLoaderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SceneLoaderSystem>();
        sceneLoaderSystem.OnSceneChange += ClearECSData;
        sceneLoaderSystem.OnSceneChange += SceneTransition;
    }

    private void OnDestroy()
    {
        SceneLoaderSystem sceneLoaderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SceneLoaderSystem>();
        sceneLoaderSystem.OnSceneChange -= ClearECSData;
        sceneLoaderSystem.OnSceneChange -= SceneTransition;
    }

    public void DestroyEntities(Scene scene)
    {
        Destroy(GameObject.FindFirstObjectByType<ScreenSpaceUIController>());

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach(Entity entity in entityManager.GetAllEntities())
        {
            entityManager.DestroyEntity(entity);
        }
    }

    public void ClearECSData()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        world.GetExistingSystemManaged<GridSystem>().grid = null;
        world.GetExistingSystemManaged<QuestSystem>().completedQuests.Clear();
        QuestManager.ResetQuests();
        DialogueManager.ResetDialogue();
    }

    public void SceneTransition()
    {
        if(!Scene1to2TransitionComplete)
        {
            Scene1to2TransitionComplete = true;
            Scene1to2Transition.SetTrigger("Start");
        }else if(Scene1to2TransitionComplete && !Scene2to3TransitionComplete)
        {
            Scene2to3TransitionComplete = true;
            Scene2to3Transition.SetTrigger("Start");
        }
    }
}
