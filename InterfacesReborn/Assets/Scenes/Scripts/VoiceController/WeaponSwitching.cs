using UnityEngine;
using Whisper.Samples;

public class WeaponSwitching : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al MicrophoneController para escuchar comandos de voz")]
    public MicrophoneController microphoneController;
    
    [Tooltip("GameObject que contiene todas las armas como hijos")]
    public Transform weaponsContainer;
    
    [Tooltip("Controlador derecho para posicionar el arma")]
    public OVRInput.Controller rightController = OVRInput.Controller.RTouch;
    
    [Header("Configuración")]
    [Tooltip("Offset de posición respecto al controlador")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("Offset de rotación respecto al controlador")]
    public Vector3 rotationOffset = Vector3.zero;
    
    // Arma actualmente equipada
    private GameObject currentWeapon;
    private string equippedWeaponName = "";
    
    void Start()
    {
        // Suscribirse al evento de comandos de armas
        if (microphoneController != null)
        {
            microphoneController.onWeaponCommand += OnWeaponCommand;
            Debug.Log("[WeaponSwitching] Suscrito a eventos de comandos de armas.");
        }
        else
        {
            Debug.LogError("[WeaponSwitching] ¡MicrophoneController no asignado!");
        }
        
        // Verificar que el contenedor de armas esté asignado
        if (weaponsContainer == null)
        {
            Debug.LogError("[WeaponSwitching] ¡weaponsContainer no asignado!");
        }
    }
    
    void Update()
    {
        // Si hay un arma equipada, mantenerla en la posición del controlador derecho
        if (currentWeapon != null)
        {
            UpdateWeaponPosition();
        }
    }
    
    private void OnWeaponCommand(string weaponName)
    {
        Debug.Log($"[WeaponSwitching] Comando recibido: {weaponName}");
        
        if (weaponName == "hand")
        {
            // Desequipar arma actual
            UnequipWeapon();
        }
        else
        {
            // Equipar el arma solicitada
            EquipWeapon(weaponName);
        }
    }
    
    private void EquipWeapon(string weaponName)
    {
        if (weaponsContainer == null)
        {
            Debug.LogError("[WeaponSwitching] weaponsContainer no está asignado.");
            return;
        }
        
        // Capitalizar el nombre del arma para buscar el GameObject
        string capitalizedName = char.ToUpper(weaponName[0]) + weaponName.Substring(1);
        
        // Buscar el arma en el contenedor
        Transform weaponTransform = weaponsContainer.Find(capitalizedName);
        
        if (weaponTransform == null)
        {
            Debug.LogWarning($"[WeaponSwitching] Arma '{capitalizedName}' no encontrada en el contenedor.");
            return;
        }
        
        // Si ya hay un arma equipada, desequiparla primero
        if (currentWeapon != null)
        {
            UnequipWeapon();
        }
        
        // Equipar la nueva arma
        currentWeapon = weaponTransform.gameObject;
        equippedWeaponName = capitalizedName;
        currentWeapon.SetActive(true);
        
        Debug.Log($"[WeaponSwitching] ⚔️ {capitalizedName} equipada.");
    }
    
    private void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            // Devolver el arma al contenedor
            currentWeapon.transform.SetParent(weaponsContainer);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            currentWeapon.SetActive(false);
            
            Debug.Log($"[WeaponSwitching] Arma desequipada.");
            currentWeapon = null;
            equippedWeaponName = "";
        }
    }
    
    private void UpdateWeaponPosition()
    {
        // Obtener la posición y rotación del controlador derecho
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(rightController);
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(rightController);

        switch (equippedWeaponName)
        {
            case "Axe":
                controllerRotation *= Quaternion.Euler(0f, 90f, 0f);
                break;
            case "Spear":
                controllerPosition += controllerRotation * new Vector3(0f, 0.3f, 0f);
                controllerRotation *= Quaternion.Euler(0f, 90f, 0f);
                break;
        }
        
        // Aplicar offsets
        currentWeapon.transform.position = controllerPosition + controllerRotation * positionOffset;
        currentWeapon.transform.rotation = controllerRotation * Quaternion.Euler(rotationOffset);
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (microphoneController != null)
        {
            microphoneController.onWeaponCommand -= OnWeaponCommand;
        }
    }
}
