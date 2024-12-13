using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.InputSystem;

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
        var curSprintInput = playerActions.Player.Sprint.IsPressed();
        var curJumpInput = playerActions.Player.Jump.WasPressedThisFrame();

        SystemAPI.SetSingleton(new PlayerMoveInput
        {
            Value = curMoveInput
        });
        SystemAPI.SetSingleton(new PlayerSprintInput
        {
            Value = curSprintInput
        });
        SystemAPI.SetSingleton(new PlayerJumpInput
        {
            Value = curJumpInput
        });

        var curFireInput = playerActions.Player.Attack.IsPressed();
        var curAimInput = playerActions.Player.Aim.IsPressed();

        SystemAPI.SetSingleton(new PlayerFire
        {
            Value = curFireInput
        });
        SystemAPI.SetSingleton(new PlayerAiming
        {
            value = curAimInput
        });

        Camera camera = Camera.main;
        RefRW<LocalTransform> playerTransform = SystemAPI.GetComponentRW<LocalTransform>(playerEntity);
        float3 dir = float3.zero;
        var mouseInput = Input.mousePosition;
        UnityEngine.Ray ray = camera.ScreenPointToRay(mouseInput);
        UnityEngine.RaycastHit hit;
        Physics.Raycast(ray, out hit);
        if(hit.collider != null)
        {
            Vector3 target = hit.point;
            target.y += 0.5f;
            dir = (float3)target - playerTransform.ValueRO.Position;
            SystemAPI.SetSingleton(new MouseWorldPos
            {
                Value = hit.point
            });
        }
        SystemAPI.SetSingleton(new MousePlayerAngle
        {
            Value = dir
        });
    }

    float AngleBetweenTwoPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
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
