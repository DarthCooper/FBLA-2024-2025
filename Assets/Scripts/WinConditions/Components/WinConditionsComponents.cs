using Unity.Entities;

public struct TimerWinConditions : IComponentData
{
    public float maxTime;
    public float time;
}

public struct KillWinConditions : IComponentData
{
    public int neededKills;
    public int kills;
}

public struct WaveWinConditions : IComponentData
{
    public int neededWaves;
    public int waves;
}

public struct TriggerWinConditions : IComponentData
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