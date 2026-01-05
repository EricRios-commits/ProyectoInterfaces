using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace Behavior.Enemy
{
    public class EnemySpawner : MonoBehaviour, IHealthObserver
    {
        [SerializeField] private Transform[] spawnPoints;
        
        private List<GameObject> activeEnemies = new List<GameObject>();
        
        public GameObject SpawnEnemy(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return null;
            var enemy = Instantiate(prefab, position, Quaternion.identity);
            var health = enemy.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.AddObserver(this);
            }
            activeEnemies.Add(enemy);
            return enemy;
        }
        
        public Vector3 GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return transform.position;
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        
        public void ApplyDifficultyMultiplier(GameObject enemy, float multiplier)
        {
            var health = enemy.GetComponent<HealthComponent>();
            if (health != null)
            {
                float newMaxHealth = health.MaxHealth * multiplier;
                health.SetMaxHealth(newMaxHealth);
            }
        }
        
        public int GetAliveCount()
        {
            CleanupDeadEnemies();
            return activeEnemies.Count;
        }
        
        public void CleanupDeadEnemies()
        {
            activeEnemies.RemoveAll(e => e == null);
        }
        
        private void OnEnemyDied(GameObject enemy)
        {
            activeEnemies.Remove(enemy);
        }
        
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

        public void OnHealthChanged(float currentHealth, float maxHealth, float delta)
        {
        }

        public void OnDamageTaken(DamageInfo damageInfo, float currentHealth, float maxHealth)
        {
        }

        public void OnDeath(GameObject dead, DamageInfo finalDamage)
        {
            OnEnemyDied(dead);
        }
    }
}

