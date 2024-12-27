using Unity.Entities;
using System;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

public partial class WinConditionsSystem : SystemBase
{
    public Action OnRoundCompleted;

    public Action<WinConditons, string> SyncWinText;

    protected override void OnUpdate()
    {
        #region Timer
        foreach(RefRW<TimerWinConditions> timerConditions in SystemAPI.Query<RefRW<TimerWinConditions>>())
        {
            float time = timerConditions.ValueRO.time;
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);

            string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);
            SyncWinText?.Invoke(WinConditons.TIMER, $"Time: {niceTime}");
            timerConditions.ValueRW.time -= SystemAPI.Time.DeltaTime;
            if(timerConditions.ValueRO.time <= 0 )
            {
                OnRoundCompleted?.Invoke();
            }
        }
        #endregion
        #region Kills
        foreach(RefRW<KillWinConditions> killWinConditions in SystemAPI.Query<RefRW<KillWinConditions>>())
        {
            SyncWinText?.Invoke(WinConditons.KILLS, $"Kills: {killWinConditions.ValueRO.kills} / {killWinConditions.ValueRO.neededKills}");
            if (killWinConditions.ValueRO.kills >= killWinConditions.ValueRO.neededKills)
            {
                OnRoundCompleted?.Invoke();
            }
        }
        #endregion
        #region Waves
        foreach(WaveWinConditions waveWinConditions in SystemAPI.Query<WaveWinConditions>())
        {
            SyncWinText?.Invoke(WinConditons.WAVES, $"Kills: {waveWinConditions.waves} / {waveWinConditions.neededWaves}");
            if (waveWinConditions.waves >= waveWinConditions.neededWaves)
            {
                OnRoundCompleted?.Invoke();
            }
        }
        #endregion
        #region Trigger
        foreach((TriggerWinConditions triggerWinConditions, Entity entity) in SystemAPI.Query<TriggerWinConditions>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            LocalToWorld triggerPos = EntityManager.GetComponentData<LocalToWorld>(triggerWinConditions.triggerEntity);
            LocalToWorld playerPos = EntityManager.GetComponentData<LocalToWorld>(player);

            SyncWinText?.Invoke(WinConditons.TRIGGER, $"Distance: {Manager.GetMagnitude(playerPos.Position - triggerPos.Position)}");
            if (EntityManager.HasBuffer<StatefulTriggerEvent>(triggerWinConditions.triggerEntity))
            {
                DynamicBuffer<StatefulTriggerEvent> buffer = EntityManager.GetBuffer<StatefulTriggerEvent>(triggerWinConditions.triggerEntity);
                for(int i = 0; i < buffer.Length; i++)
                {
                    var colliderEvent = buffer[i];
                    var otherEntity = colliderEvent.GetOtherEntity(triggerWinConditions.triggerEntity);
                    if(colliderEvent.State == StatefulEventState.Enter)
                    {
                        OnRoundCompleted?.Invoke();
                    }
                }
            }
        }
        #endregion
    }
}
