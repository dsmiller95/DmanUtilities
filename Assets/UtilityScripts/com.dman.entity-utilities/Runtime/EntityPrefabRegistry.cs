using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Dman.EntityUtilities
{
    public class EntityPrefabRegistry: MonoBehaviour
    {
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
            prefabAssetStore?.Dispose();
            prefabAssetStore = new BlobAssetStore();
            foreach (var spawnableEntity in spawnableEntityPrefabs)
            {
                var prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawnableEntity.gameObject,
                    GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, prefabAssetStore));

                entitiesByGoInstanceId[spawnableEntity.gameObject.GetInstanceID()] = prefabEntity;
            }
        }

        private void OnDestroy()
        {
            prefabAssetStore.Dispose();
            prefabAssetStore = null;
        }
    }
}
