using Unity.Physics;
using UnityEngine;

public enum CollisionLayer
{
    Solid = 1 << 0,
    Obstacle = 1 << 1,
    Character = 1 << 2,
    Enemy = 1 << 3,
    Interactable = 1 << 4,
    InteractableTrigger = 1 << 5,
    EnemyTrigger = 1 << 6,
    PlayerTrigger = 1 << 7,
    MeleeWeapon = 1 << 8,
    Projectile = 1 << 9,
}

public class CollisionFilters
{
    public readonly static CollisionFilter
        filterSolid = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Solid),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Interactable | CollisionLayer.Solid | CollisionLayer.Enemy | CollisionLayer.Obstacle | CollisionLayer.PlayerTrigger | CollisionLayer.Projectile),
        },
        filterObstacle = new CollisionFilter()
        {
            BelongsTo = (uint)CollisionLayer.Obstacle,
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Interactable | CollisionLayer.Solid | CollisionLayer.Obstacle | CollisionLayer.Projectile),
        },
        filterCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Character),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.Interactable | CollisionLayer.Enemy | CollisionLayer.Obstacle | CollisionLayer.Projectile | CollisionLayer.MeleeWeapon)
        },
        filterEnemy = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Enemy),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Interactable | CollisionLayer.Solid | CollisionLayer.Projectile | CollisionLayer.MeleeWeapon)
        },
        filterInteractable = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Interactable),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.InteractableTrigger)
        },
        filterInteractableTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.InteractableTrigger),
            CollidesWith = (uint)(CollisionLayer.Interactable)
        },
        filterEnemyTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.EnemyTrigger),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Solid | CollisionLayer.Obstacle)
        },
        filterPlayerTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.PlayerTrigger),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.Interactable | CollisionLayer.Enemy | CollisionLayer.Obstacle)
        },
        filterMeleeWeaponTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.MeleeWeapon),
            CollidesWith = (uint)(CollisionLayer.Enemy | CollisionLayer.Character)
        },
        filterProjectileTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Projectile),
            CollidesWith = (uint)(CollisionLayer.Enemy | CollisionLayer.Character | CollisionLayer.Obstacle | CollisionLayer.Solid)
        };

    public static CollisionFilter getCollisionFilter(CollisionLayer layer)
    {
        switch (layer)
        {
            case CollisionLayer.Solid:
                return filterSolid;
            case CollisionLayer.Obstacle:
                return filterObstacle;
            case CollisionLayer.Character:
                return filterCharacter;
            case CollisionLayer.Enemy:
                return filterEnemy;
            case CollisionLayer.Interactable:
                return filterInteractable;
            case CollisionLayer.InteractableTrigger:
                return filterInteractableTrigger;
            case CollisionLayer.EnemyTrigger:
                return filterEnemyTrigger;
            case CollisionLayer.PlayerTrigger:
                return filterPlayerTrigger;
            case CollisionLayer.MeleeWeapon:
                return filterMeleeWeaponTrigger;
            case CollisionLayer.Projectile:
                return filterProjectileTrigger;
            default:
                return filterSolid;
        }
    }
}
