using Unity.Physics;
using UnityEngine;

public enum CollisionLayer
{
    Solid = 1 << 0,
    Character = 1 << 1,
    Enemy = 1 << 2,
    Interactable = 1 << 4,
    InteractableTrigger = 1 << 5,
}

public class CollisionFilters
{
    public readonly static CollisionFilter
        filterSolid = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Solid),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Interactable | CollisionLayer.Solid | CollisionLayer.Enemy),
        },
        filterCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Character),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.Interactable)
        },
        filterEnemy = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Enemy),
            CollidesWith = (uint)(CollisionLayer.Solid)
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
        };

    public static CollisionFilter getCollisionFilter(CollisionLayer layer)
    {
        switch (layer)
        {
            case CollisionLayer.Solid:
                return filterSolid;
            case CollisionLayer.Character:
                return filterCharacter;
            case CollisionLayer.Enemy:
                return filterEnemy;
            case CollisionLayer.Interactable:
                return filterInteractable;
            case CollisionLayer.InteractableTrigger:
                return filterInteractableTrigger;
            default:
                return filterSolid;
        }
    }
}
