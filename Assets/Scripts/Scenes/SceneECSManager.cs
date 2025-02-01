using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneECSManager : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded += ClearECSData;
        SceneManager.sceneUnloaded += DestroyEntities;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= ClearECSData;
        SceneManager.sceneUnloaded -= DestroyEntities;
    }

    public void DestroyEntities(Scene scene)
    {
        Destroy(GameObject.FindFirstObjectByType<ScreenSpaceUIController>());
    }

    public void ClearECSData(Scene scene, LoadSceneMode mode)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        world.GetExistingSystemManaged<GridSystem>().grid = null;
        world.GetExistingSystemManaged<QuestSystem>().completedQuests.Clear();
        QuestManager.ResetQuests();
        DialogueManager.ResetDialogue();
    }
}
