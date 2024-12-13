using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUIController : MonoBehaviour
{
    public GameObject enemyIdentifierPrefab;
    Dictionary<Entity, GameObject> identifiers = new Dictionary<Entity, GameObject>();
    public Transform identifierParent;

    private Entity lastPlayerTarget = Entity.Null;

    private Transform mainCameraTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        EnemyUIManager entityUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EnemyUIManager>();
        entityUISystem.OnMove += MoveHealthIcon;
        entityUISystem.OnTakeDamage += UpdateHealthIcon;
        entityUISystem.OnDeath += DestroyHealthIcon;
        entityUISystem.OnUseWeapon += ToggleAttackIcon;
        entityUISystem.OnAttackDealy += FillAttackIcon;
        entityUISystem.OnStun += ToggleStunVisual;

        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.toggledEnemy += ToggleTargetIdentifier;
        playerUISystem.deToggleAll += DeToggleAllIdentifier;
    }

    private void OnDisable()
    {
        if(World.DefaultGameObjectInjectionWorld == null) { return; }
        EnemyUIManager entityUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EnemyUIManager>();
        entityUISystem.OnMove -= MoveHealthIcon;
        entityUISystem.OnTakeDamage -= UpdateHealthIcon;
        entityUISystem.OnDeath -= DestroyHealthIcon;
        entityUISystem.OnUseWeapon -= ToggleAttackIcon;
        entityUISystem.OnAttackDealy -= FillAttackIcon;
        entityUISystem.OnStun -= ToggleStunVisual;

        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.toggledEnemy -= ToggleTargetIdentifier;
        playerUISystem.deToggleAll -= DeToggleAllIdentifier;
    }

    private void MoveHealthIcon(float3 entityPos, Entity entity)
    {
        float3 translatedPos = new float3
        {
            x = entityPos.x,
            y = -0.7f,
            z = entityPos.z
        };
        if (identifiers.ContainsKey(entity))
        {
            if (identifiers[entity] == null) { return; }
            identifiers[entity].transform.position = translatedPos;
        }else
        {
            var newIcon = Instantiate(enemyIdentifierPrefab, identifierParent);
            newIcon.transform.position = translatedPos;
            identifiers.Add(entity, newIcon);
        }
    }

    private void UpdateHealthIcon(float health, float maxHealth, Entity entity)
    {
        if(!identifiers.ContainsKey(entity)) { return; }
        if(identifiers[entity] == null) { return; }
        identifiers[entity].GetComponent<EntityHealthUI>().HealthSlider.GetComponent<Image>().fillAmount = health / maxHealth;
    }

    private void DestroyHealthIcon(Entity entity)
    {
        if (!identifiers.ContainsKey(entity)) { return; }
        if (identifiers[entity] == null) { return; }
        Destroy(identifiers[entity]);
    }

    private void ToggleAttackIcon(bool toggle, Entity entity)
    {
        if (!identifiers.ContainsKey(entity)) { return; }
        if (identifiers[entity] == null) { return; }
        identifiers[entity].GetComponent<EntityAttackUI>().canvas.SetActive(toggle);
    }

    public void FillAttackIcon(float delay, float maxDelay, Entity entity)
    {
        if (!identifiers.ContainsKey(entity)) { return; }
        if (identifiers[entity] == null) { return; }
        identifiers[entity].GetComponent<EntityAttackUI>().visual.fillAmount = delay / maxDelay;
    }

    private void ToggleTargetIdentifier(bool toggle, Entity entity)
    {
        if (!identifiers.ContainsKey(entity)) { return; }
        if (identifiers[entity] == null) { return; }
        if(!lastPlayerTarget.Equals(entity))
        {
            DeToggleAllIdentifier();
        }
        identifiers[entity].GetComponent<EntityHealthUI>().identifiers.gameObject.SetActive(toggle);
        lastPlayerTarget = entity;
    }

    private void DeToggleAllIdentifier()
    {
        foreach (var identifier in identifiers.Values)
        {
            if (identifier == null) { continue; }
            identifier.GetComponent<EntityHealthUI>().identifiers.gameObject.SetActive(false);
        }
    }

    private void ToggleStunVisual(bool toggle, Entity entity)
    {
        if (!identifiers.ContainsKey(entity)) { return; }
        if (identifiers[entity] == null) { return; }
        identifiers[entity].GetComponent<EntityAttackUI>().stunCanvas.SetActive(toggle);
    }
}
