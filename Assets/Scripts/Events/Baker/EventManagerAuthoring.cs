using Unity.Entities;
using UnityEditor;
using UnityEngine;

class EventManagerAuthoring : MonoBehaviour
{
    [HideInInspector] public EventType startingEvent = EventType.NONE;

    [HideInInspector] public GameObject spawnerEntity;

    [CustomEditor(typeof(EventManagerAuthoring))]
    public class EventManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EventManagerAuthoring script = (EventManagerAuthoring)target;

            script.startingEvent = (EventType)EditorGUILayout.EnumPopup("Starting Event", script.startingEvent);

            switch (script.startingEvent)
            {
                case EventType.SPAWNENEMIES:
                    script.spawnerEntity = (GameObject)EditorGUILayout.ObjectField("Spawner Object", script.spawnerEntity, typeof(GameObject), true);
                    break;
                case EventType.NONE:
                    DrawDefaultInspector(); 
                    break;
                default:
                    DrawDefaultInspector();
                    break;
            }

            EditorUtility.SetDirty(target);
        }
    }
}

class EventManagerAuthoringBaker : Baker<EventManagerAuthoring>
{
    public override void Bake(EventManagerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<EventManger>(entity);

        switch(authoring.startingEvent)
        {
            case EventType.SPAWNENEMIES:
                AddComponent(entity, new SpawnEnemiesEvent
                {
                    spawnEntity = GetEntity(authoring.spawnerEntity, TransformUsageFlags.Dynamic)
                });
                break;
            case EventType.ENDLEVEL:
                break;
        }
    }
}
