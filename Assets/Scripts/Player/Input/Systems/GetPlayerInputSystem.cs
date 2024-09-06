using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
partial class GetPlayerInputSystem : SystemBase
{
    private InputSystem_Actions playerActions;
    private Entity playerEntity;

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();
        RequireForUpdate<PlayerMoveInput>();

        playerActions = new InputSystem_Actions();
    }

    protected override void OnStartRunning()
    {
        playerActions.Enable();

        playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
    }

    protected override void OnUpdate()
    {
        var curMoveInput = playerActions.Player.Move.ReadValue<Vector2>();

        SystemAPI.SetSingleton(new PlayerMoveInput
        {
            Value = curMoveInput,
        });
    }

    protected override void OnStopRunning()
    {
        playerActions.Disable();

        playerEntity = Entity.Null;
    }

    protected override void OnDestroy()
    {

    }
}
