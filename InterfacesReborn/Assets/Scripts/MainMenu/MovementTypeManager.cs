using System;
using UnityEngine;
using UnityEngine.UI;

public class MovementTypeManager : MonoBehaviour
{
    [SerializeField]
    private Dropdown dropdown;
    private const string movementTypeKey = "MovementType";
    private int movementType;
    
    void Start()
    {
        movementType = PlayerPrefs.GetInt(movementTypeKey, 0);
        dropdown.value = movementType;
    }

    public void OnValueChanged(Int32 newValue)
    {
        movementType = newValue;
        Debug.Log("El tipo de movimiento ha cambiado a " + movementType);
        PlayerPrefs.SetInt(movementTypeKey, movementType);
        PlayerPrefs.Save();
    }
}
