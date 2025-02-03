using System;
using Unity.Entities;
using UnityEngine;

public partial class EventManagerSystem : SystemBase
{
    public Action<int> OnEndLevel;

    protected override void OnUpdate()
    {
        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManagerEntity);
        if(eventManagerEntity.Equals(Entity.Null) || !SystemAPI.Exists(eventManagerEntity)) { return; }
        if(EntityManager.HasComponent<EndLevelEvent>(eventManagerEntity))
        {
            EndLevelEvent spawnEvent = EntityManager.GetComponentData<EndLevelEvent>(eventManagerEntity);

            OnEndLevel?.Invoke(spawnEvent.levelIndex);
            EntityManager.RemoveComponent<EndLevelEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<SpawnEnemiesEvent>(eventManagerEntity))
        {
            SpawnEnemiesEvent spawnEvent = EntityManager.GetComponentData<SpawnEnemiesEvent>(eventManagerEntity);
            if(spawnEvent.spawnEntity.Equals(Entity.Null) || !SystemAPI.Exists(spawnEvent.spawnEntity)) { return; }
            EntityManager.AddComponent<CanSpawn>(spawnEvent.spawnEntity);
            EntityManager.RemoveComponent<SpawnEnemiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<ActivateEntitiesEvent>(eventManagerEntity))
        {
            ActivateEntitiesEvent spawnEvent = EntityManager.GetComponentData<ActivateEntitiesEvent>(eventManagerEntity);
            if (spawnEvent.ActivateEntityHolder.Equals(Entity.Null) || !SystemAPI.Exists(spawnEvent.ActivateEntityHolder)) { return; }
            EntityManager.AddComponent<CanActivate>(spawnEvent.ActivateEntityHolder);
            EntityManager.RemoveComponent<ActivateEntitiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<DeActivateEntitiesEvent>(eventManagerEntity))
        {
            DeActivateEntitiesEvent deActivateEvent = EntityManager.GetComponentData<DeActivateEntitiesEvent>(eventManagerEntity);
            if(deActivateEvent.DeActivateEntityHolder.Equals(Entity.Null) || !SystemAPI.Exists(deActivateEvent.DeActivateEntityHolder)) { return; }
            EntityManager.AddComponent<CanDeActivate>(deActivateEvent.DeActivateEntityHolder);
            EntityManager.RemoveComponent<DeActivateEntitiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<ShakeCameraEvent>(eventManagerEntity))
        {
            CameraManagers.Instance.Impulse(EntityManager.GetComponentData<ShakeCameraEvent>(eventManagerEntity).index);
            EntityManager.RemoveComponent<ShakeCameraEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<ChoiceEvent>(eventManagerEntity))
        {
            ChoiceEvent choiceEvent = EntityManager.GetComponentData<ChoiceEvent>(eventManagerEntity);
            EntityManager.AddComponent<MakeChoice>(choiceEvent.entity);
            EntityManager.RemoveComponent<ChoiceEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<PopUpEvent>(eventManagerEntity))
        {
            PopUpEvent popUpEvent = EntityManager.GetComponentData<PopUpEvent>(eventManagerEntity);
            EntityManager.AddComponent<DisplayPopUp>(popUpEvent.entity);
            EntityManager.RemoveComponent<PopUpEvent>(eventManagerEntity);
        }
    }
}
