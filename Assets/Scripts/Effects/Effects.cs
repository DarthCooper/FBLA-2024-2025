using System;
using System.Collections.Generic;
using System.Linq;
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

    Dictionary<Entity, GameObject> magicIdentifiers = new Dictionary<Entity, GameObject>();
    public GameObject healParticles;
    public GameObject leachParticles;

    public LineRenderer pathFinder;

    public GameObject EnemyDeathParticles;

    // Update is called once per frame
    void OnEnable()
    {
        ProjectileEffects playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ProjectileEffects>();
        playerUISystem.OnBulletMove += MoveBulletTrail;
        playerUISystem.OnSpawnBullet += SpawnMuzzleFlash;

        MagicEffects magicEffectSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MagicEffects>();
        magicEffectSystem.OnCast += MoveCastSpell;

        PlayerPathFinderSystem pathFinderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerPathFinderSystem>();
        pathFinderSystem.OnPathFind += MovePathFinder;
        pathFinderSystem.HidePathFinder += DisablePathFinder;

        HealthSystem healthSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HealthSystem>();
        healthSystem.OnEnemyDeath += EnemyDeath;
    }

    private void OnDisable()
    {
        if(World.DefaultGameObjectInjectionWorld == null) { return; }
        ProjectileEffects playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ProjectileEffects>();
        playerUISystem.OnBulletMove -= MoveBulletTrail;
        playerUISystem.OnSpawnBullet -= SpawnMuzzleFlash;

        MagicEffects magicEffectSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MagicEffects>();
        magicEffectSystem.OnCast -= MoveCastSpell;

        PlayerPathFinderSystem pathFinderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerPathFinderSystem>();
        pathFinderSystem.OnPathFind -= MovePathFinder;
        pathFinderSystem.HidePathFinder -= DisablePathFinder;

        HealthSystem healthSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HealthSystem>();
        healthSystem.OnEnemyDeath -= EnemyDeath;
    }

    private void EnemyDeath(float3 pos)
    {
        Instantiate(EnemyDeathParticles, pos, Quaternion.identity);
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

    private void MoveCastSpell(float3 pos, CastingType castType, Entity entity)
    {
        GameObject prefab = null;
        switch(castType)
        {
            case CastingType.HEALING:
                prefab = healParticles;
                break;
            case CastingType.LEACHING:
                prefab = leachParticles;
                break;
        }
        if (magicIdentifiers.ContainsKey(entity))
        {
            if (magicIdentifiers[entity] == null) { magicIdentifiers.Remove(entity); MoveCastSpell(pos, castType, entity); }
            magicIdentifiers[entity].transform.position = pos;
        }
        else
        {
            var newSpell = Instantiate(prefab, bulletTrailParent);
            newSpell.transform.position = pos;
            magicIdentifiers.Add(entity, newSpell);
        }
    }

    private void MovePathFinder(float3[] pos)
    {
        pathFinder.enabled = true;
        Vector3[] convertedPos = new Vector3[pos.Length];
        for (int i = 0; i < pos.Length; i++)
        {
            if(convertedPos.Contains(pos[i])) { continue; }
            convertedPos[i] = pos[i];
        }
        pathFinder.positionCount = convertedPos.Length;
        pathFinder.SetPositions(convertedPos);
    }

    private void DisablePathFinder()
    {
        pathFinder.enabled = false;
    }
}
