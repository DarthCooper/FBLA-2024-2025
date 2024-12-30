using Unity.Entities;

public interface WinConditionData : IComponentData { }

public struct TimerWinConditions : WinConditionData
{
    public float maxTime;
    public float time;
}

public struct KillWinConditions : WinConditionData
{
    public int neededKills;
    public int kills;
}

public struct WaveWinConditions : WinConditionData
{
    public int neededWaves;
    public int waves;
}

public struct TriggerWinConditions : WinConditionData
{
    public Entity triggerEntity;
}

public enum WinConditons
{
    TIMER,
    KILLS,
    WAVES,
    TRIGGER
}