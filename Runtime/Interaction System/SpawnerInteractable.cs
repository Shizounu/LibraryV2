using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that spawns an object when interacted with
    /// </summary>
    public class SpawnerInteractable : InteractableBase
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;
        [SerializeField] private bool randomRotation = false;
        [SerializeField] private int maxSpawns = -1; // -1 = unlimited
        [SerializeField] private float spawnCooldown = 1f;

        [Header("Spawn Options")]
        [SerializeField] private bool parentToSpawner = false;
        [SerializeField] private bool disableAfterMaxSpawns = true;

        private int spawnCount = 0;
        private float lastSpawnTime = -Mathf.Infinity;

        public int SpawnCount => spawnCount;
        public bool HasSpawnsRemaining => maxSpawns < 0 || spawnCount < maxSpawns;

        public override bool CanInteract
        {
            get
            {
                if (!base.CanInteract)
                    return false;

                if (!HasSpawnsRemaining)
                    return false;

                if (Time.time - lastSpawnTime < spawnCooldown)
                    return false;

                return true;
            }
        }

        protected override void OnInteract(GameObject interactor)
        {
            SpawnObject();
        }

        private void SpawnObject()
        {
            if (spawnPrefab == null)
            {
                Debug.LogWarning($"No spawn prefab set on {gameObject.name}");
                return;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position + spawnOffset;
            Quaternion rotation = randomRotation ? Random.rotation : (spawnPoint != null ? spawnPoint.rotation : transform.rotation);

            GameObject spawned = Instantiate(spawnPrefab, position, rotation);

            if (parentToSpawner)
            {
                spawned.transform.SetParent(transform);
            }

            spawnCount++;
            lastSpawnTime = Time.time;

            OnObjectSpawned(spawned);

            if (!HasSpawnsRemaining && disableAfterMaxSpawns)
            {
                SetInteractable(false);
            }
        }

        /// <summary>
        /// Called after an object is spawned
        /// </summary>
        protected virtual void OnObjectSpawned(GameObject spawned)
        {
            // Override in derived classes for custom behavior
        }

        /// <summary>
        /// Reset spawn count
        /// </summary>
        public void ResetSpawnCount()
        {
            spawnCount = 0;
            lastSpawnTime = -Mathf.Infinity;
            SetInteractable(true);
        }

        /// <summary>
        /// Manually spawn an object
        /// </summary>
        public GameObject ManualSpawn()
        {
            if (spawnPrefab == null)
                return null;

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position + spawnOffset;
            Quaternion rotation = randomRotation ? Random.rotation : (spawnPoint != null ? spawnPoint.rotation : transform.rotation);

            return Instantiate(spawnPrefab, position, rotation);
        }
    }
}
