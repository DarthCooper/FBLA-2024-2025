using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class NPCSpawnerAuthoring : MonoBehaviour
{
    [SerializeField]
    public NPCSetterSpawnerData spawn;

    public GameObject Ranger;
    public GameObject Cleric;
    public GameObject Barbarian;
    public GameObject Rogue;
    public GameObject Warlock;
}

[Serializable]
public class NPCSetterSpawnerData
{
    public float3 pos;
    public float delay;
    public float spawnRadius;
    public NPCs[] NPCTypes;
}

class NPCSpawnerAuthoringBaker : Baker<NPCSpawnerAuthoring>
{
    public override void Bake(NPCSpawnerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var builder = new BlobBuilder(Allocator.Temp);
        ref NPCSpawnerData spawnerPool = ref builder.ConstructRoot<NPCSpawnerData>();

        var spawnDataArray = builder.Allocate(ref spawnerPool.npcs, authoring.spawn.NPCTypes.Length);

        spawnerPool.pos = authoring.spawn.pos;
        spawnerPool.radius = authoring.spawn.spawnRadius;

        for (int i = 0; i < spawnDataArray.Length; i++)
        {
            ref NPCs data = ref spawnDataArray[i];
            data = authoring.spawn.NPCTypes[i];
        }

        var blobReference = builder.CreateBlobAssetReference<NPCSpawnerData>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<NPCSpawnerData>(ref blobReference, out var hash);

        AddComponent(entity, new NPCSpawners
        {
            BlobAssetReference = blobReference
        });

        AddComponent(entity, new NPCTypes
        {
            Barbarian = GetEntity(authoring.Barbarian, TransformUsageFlags.Dynamic),
            Cleric = GetEntity(authoring.Cleric, TransformUsageFlags.Dynamic),
            Ranger = GetEntity(authoring.Ranger, TransformUsageFlags.Dynamic),
            Rogue = GetEntity(authoring.Rogue, TransformUsageFlags.Dynamic),
            Warlock = GetEntity(authoring.Warlock, TransformUsageFlags.Dynamic),
        });
    }
}
