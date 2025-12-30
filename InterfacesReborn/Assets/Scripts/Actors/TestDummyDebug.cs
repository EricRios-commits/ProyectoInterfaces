using UnityEngine;
using Combat;

/// <summary>
/// Script de diagnóstico para el TestDummy.
/// Añade este script al TestDummy para verificar su configuración.
/// </summary>
public class TestDummyDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== DIAGNÓSTICO TESTDUMMY ===");
        Debug.Log($"Nombre: {gameObject.name}");
        Debug.Log($"Capa: {LayerMask.LayerToName(gameObject.layer)} (ID: {gameObject.layer})");
        Debug.Log($"Posición: {transform.position}");
        
        // Verificar componentes
        var testDummy = GetComponent<TestDummy>();
        var healthComponent = GetComponent<HealthComponent>();
        var damageable = GetComponent<IDamageable>();
        var collider = GetComponent<Collider>();
        
        Debug.Log($"TestDummy: {(testDummy != null ? "✓" : "✗")}");
        Debug.Log($"HealthComponent: {(healthComponent != null ? "✓" : "✗")}");
        Debug.Log($"IDamageable: {(damageable != null ? "✓" : "✗")}");
        Debug.Log($"Collider: {(collider != null ? "✓" : "✗")}");
        
        if (collider != null)
        {
            Debug.Log($"Collider tipo: {collider.GetType().Name}");
            Debug.Log($"Collider es trigger: {collider.isTrigger}");
            Debug.Log($"Collider enabled: {collider.enabled}");
        }
        
        if (healthComponent != null)
        {
            Debug.Log($"Salud máxima: {healthComponent.MaxHealth}");
            Debug.Log($"Salud actual: {healthComponent.CurrentHealth}");
        }
        
        Debug.Log("=== FIN DIAGNÓSTICO ===");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"<color=yellow>[TestDummyDebug] COLLISION detectada con {collision.gameObject.name}</color>");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=green>[TestDummyDebug] TRIGGER detectado con {other.name} (Capa: {LayerMask.LayerToName(other.gameObject.layer)})</color>");
    }
}
