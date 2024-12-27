using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Entities;

class WinConditionsAuthoring : MonoBehaviour
{
    public WinConditons winCondition;

    public float maxTime;

    public float maxKills;

    public float maxWaves;

    public GameObject winTrigger;

    [CustomEditor(typeof(WinConditionsAuthoring))]
    public class WinConditionsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WinConditionsAuthoring script = (WinConditionsAuthoring)target;
            
            script.winCondition = (WinConditons)EditorGUILayout.EnumPopup("Win Options", script.winCondition);

            switch(script.winCondition)
            {
                case WinConditons.TIMER:
                    script.maxTime = EditorGUILayout.FloatField("max Time", script.maxTime);
                    break;
                case WinConditons.KILLS:
                    script.maxKills = EditorGUILayout.FloatField("max Kills", script.maxKills);
                    break;
                case WinConditons.WAVES:
                    script.maxWaves = EditorGUILayout.FloatField("max Waves", script.maxWaves);
                    break;
                case WinConditons.TRIGGER:
                    script.winTrigger = (GameObject)EditorGUILayout.ObjectField("trigger Object", script.winTrigger, typeof(GameObject), true);
                    break;
                default:
                    DrawDefaultInspector(); 
                    break;
            }

            EditorUtility.SetDirty(target);
        }
    }
}

class WinConditionsAuthoringBaker : Baker<WinConditionsAuthoring>
{
    public override void Bake(WinConditionsAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        switch(authoring.winCondition)
        {
            case WinConditons.TIMER:
                AddComponent(entity, new TimerWinConditions
                {
                    maxTime = authoring.maxTime,
                    time = authoring.maxTime
                });
                break;
            case WinConditons.KILLS:
                AddComponent(entity, new KillWinConditions
                {
                    neededKills = (int)authoring.maxKills,
                    kills = 0
                });
                break;
            case WinConditons.WAVES:
                AddComponent(entity, new WaveWinConditions
                {
                    neededWaves = (int)authoring.maxWaves,
                    waves = 0
                });
                break;
            case WinConditons.TRIGGER:
                AddComponent(entity, new TriggerWinConditions
                {
                    triggerEntity = GetEntity(authoring.winTrigger, TransformUsageFlags.Dynamic),
                });
                break;
        }
    }
}
