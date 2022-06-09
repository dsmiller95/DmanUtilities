using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Dman.EntityUtilities
{
    public class EntityPrefabRegistry: MonoBehaviour
    {

        private static EntityPrefabRegistry _instance;
        public static EntityPrefabRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<EntityPrefabRegistry>();
                }
                return _instance;
            }
        }

        public GameObject[] spawnableEntityPrefabs;

        private Dictionary<int, Entity> entitiesByGoInstanceId;
        private BlobAssetStore prefabAssetStore;

        public Entity GetEntityPrefab(GameObject spawnablePrefab)
        {
            if (entitiesByGoInstanceId.TryGetValue(spawnablePrefab.gameObject.GetInstanceID(), out var entity))
            {
                return entity;
            }
            throw new System.Exception($"spawnable prefab {spawnablePrefab} not registered with the entity prefab registry");
        }

        private void Awake()
        {
            entitiesByGoInstanceId = new Dictionary<int, Entity>();
            var extraPrefabs = GetComponents<IEntityPrefabRegistrar>()
                .SelectMany(x => x.GetPrefabsToRegister(this));
            prefabAssetStore?.Dispose();
            prefabAssetStore = new BlobAssetStore();
            foreach (var spawnableEntity in spawnableEntityPrefabs.Concat(extraPrefabs))
            {
                var instanceId = spawnableEntity.gameObject.GetInstanceID();
                if (entitiesByGoInstanceId.ContainsKey(instanceId))
                {
                    continue;
                }
                var prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawnableEntity.gameObject,
                    GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, prefabAssetStore));

                entitiesByGoInstanceId[instanceId] = prefabEntity;
            }
        }

        private void OnDestroy()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var entity in entitiesByGoInstanceId.Values)
            {
                entityManager.DestroyEntity(entity);
            }
            entitiesByGoInstanceId = null;
            prefabAssetStore.Dispose();
            prefabAssetStore = null;
        }
    }

    public interface IEntityPrefabRegistrar
    {
        public IEnumerable<GameObject> GetPrefabsToRegister(EntityPrefabRegistry registry);
    }
}
