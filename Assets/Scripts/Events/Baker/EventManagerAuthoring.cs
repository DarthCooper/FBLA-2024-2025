using Unity.Entities;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

class EventManagerAuthoring : MonoBehaviour
{
    [HideInInspector] public EventType startingEvent = EventType.NONE;

    [HideInInspector] public GameObject spawnerEntity;

    #if UNITY_EDITOR
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
                case EventType.ActivateEntities:
                    script.spawnerEntity = (GameObject)EditorGUILayout.ObjectField("Activation Object", script.spawnerEntity, typeof(GameObject), true);
                    break;
                case EventType.POPUP:
                    script.spawnerEntity = (GameObject)EditorGUILayout.ObjectField("PopUp Object", script.spawnerEntity, typeof (GameObject), true); 
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
    #endif
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
            case EventType.ActivateEntities:
                AddComponent(entity, new ActivateEntitiesEvent
                {
                    ActivateEntityHolder = GetEntity(authoring.spawnerEntity, TransformUsageFlags.Dynamic)
                });
                break;
            case EventType.ENDLEVEL:
                break;
            case EventType.POPUP:
                AddComponent(entity, new PopUpEvent
                {
                    entity = GetEntity(authoring.spawnerEntity, TransformUsageFlags.Dynamic),
                });
                break;
        }
    }
}
