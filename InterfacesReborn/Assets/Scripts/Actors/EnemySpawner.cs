using UnityEngine;
using System.Collections.Generic;

namespace Actors
{
    /// <summary>
    /// Manages enemy spawning in the game.
    /// Follows Single Responsibility Principle - only handles enemy spawning.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform playerTransform;
        
        [Header("Spawn Configuration")]
        [SerializeField] private int maxEnemies = 10;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private bool spawnOnStart = false;
        [SerializeField] private bool continuousSpawning = false;
        
        [Header("Wave System (Optional)")]
        [SerializeField] private bool useWaves = false;
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float timeBetweenWaves = 10f;

        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private float lastSpawnTime;
        private int currentWave = 0;
        private int enemiesSpawnedThisWave = 0;

        private void Start()
        {
            if (spawnOnStart)
            {
                if (useWaves)
                {
                    StartWave();
                }
                else
                {
                    SpawnInitialEnemies();
                }
            }
        }

        private void Update()
        {
            CleanupDeadEnemies();

            if (continuousSpawning && !useWaves)
            {
                if (Time.time - lastSpawnTime >= spawnInterval && spawnedEnemies.Count < maxEnemies)
                {
                    SpawnRandomEnemy();
                }
            }
            else if (useWaves)
            {
                UpdateWaveSystem();
            }
        }

        #region Spawning

        /// <summary>
        /// Spawn an enemy at a random spawn point.
        /// </summary>
        public GameObject SpawnRandomEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("No enemy prefabs assigned to spawner!");
                return null;
            }
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            return SpawnEnemy(prefab, GetRandomSpawnPoint());
        }

        /// <summary>
        /// Spawn a specific enemy prefab at a position.
        /// </summary>
        public GameObject SpawnEnemy(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return null;
            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            IEnemy enemyComponent = enemy.GetComponent<IEnemy>();
            if (enemyComponent != null && playerTransform != null)
            {
                enemyComponent.Target = playerTransform;
            }
            spawnedEnemies.Add(enemy);
            lastSpawnTime = Time.time;
            return enemy;
        }

        /// <summary>
        /// Spawn a specific enemy type at a specific spawn point.
        /// </summary>
        public GameObject SpawnEnemyAtPoint(int enemyIndex, int spawnPointIndex)
        {
            if (enemyPrefabs == null || enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length)
                return null;
            if (spawnPoints == null || spawnPointIndex < 0 || spawnPointIndex >= spawnPoints.Length)
                return null;
            return SpawnEnemy(enemyPrefabs[enemyIndex], spawnPoints[spawnPointIndex].position);
        }

        private void SpawnInitialEnemies()
        {
            int enemiesToSpawn = Mathf.Min(maxEnemies, spawnPoints?.Length ?? 1);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnRandomEnemy();
            }
        }

        #endregion

        #region Wave System

        private void StartWave()
        {
            currentWave++;
            enemiesSpawnedThisWave = 0;
            Debug.Log($"Starting Wave {currentWave}");
        }

        private void UpdateWaveSystem()
        {
            if (enemiesSpawnedThisWave >= enemiesPerWave)
            {
                if (spawnedEnemies.Count == 0)
                {
                    if (Time.time - lastSpawnTime >= timeBetweenWaves)
                    {
                        StartWave();
                    }
                }
                return;
            }
            if (Time.time - lastSpawnTime >= spawnInterval && spawnedEnemies.Count < maxEnemies)
            {
                SpawnRandomEnemy();
                enemiesSpawnedThisWave++;
            }
        }

        #endregion

        #region Management

        private void CleanupDeadEnemies()
        {
            spawnedEnemies.RemoveAll(enemy => enemy == null);
        }

        private Vector3 GetRandomSpawnPoint()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return spawnPoint.position;
            }
            Vector2 randomCircle = Random.insideUnitCircle * 5f;
            return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        /// <summary>
        /// Despawn all enemies.
        /// </summary>
        public void DespawnAllEnemies()
        {
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();
        }

        /// <summary>
        /// Get the number of currently alive enemies.
        /// </summary>
        public int GetAliveEnemyCount()
        {
            CleanupDeadEnemies();
            return spawnedEnemies.Count;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (spawnPoints == null)
                return;

            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
                }
            }
        }

        #endregion
    }
}

