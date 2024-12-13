using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public TrailRenderer bulletTrailPrefab;
    public Transform bulletTrailParent;

    Dictionary<Entity, TrailRenderer> identifiers = new Dictionary<Entity, TrailRenderer>();

    public ParticleSystem muzzleFlash;
    public Transform muzzleFlashParent;

    // Update is called once per frame
    void OnEnable()
    {
        ProjectileEffects playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ProjectileEffects>();
        playerUISystem.OnBulletMove += MoveBulletTrail;
        playerUISystem.OnSpawnBullet += SpawnMuzzleFlash;
    }

    private void OnDisable()
    {
        if(World.DefaultGameObjectInjectionWorld == null) { return; }
        ProjectileEffects playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ProjectileEffects>();
        playerUISystem.OnBulletMove -= MoveBulletTrail;
        playerUISystem.OnSpawnBullet -= SpawnMuzzleFlash;
    }

    private void MoveBulletTrail(float3 pos, Entity entity)
    {
        if (identifiers.ContainsKey(entity))
        {
            if (identifiers[entity] == null) { return; }
            identifiers[entity].transform.position = pos;
        }
        else
        {
            var newTrail = Instantiate(bulletTrailPrefab, bulletTrailParent);
            newTrail.transform.position = pos;
            identifiers.Add(entity, newTrail);
        }
    }

    public void SpawnMuzzleFlash(float3 pos, Quaternion rot)
    {
        var newTrail = Instantiate(muzzleFlash, pos, rot, muzzleFlashParent);
        newTrail.transform.position = pos;
    }
}
