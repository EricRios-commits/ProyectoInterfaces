using System;
using UnityEngine;

public class PlayerMovementType : MonoBehaviour
{
    [SerializeField]
    private GameObject movementProvider;

    [SerializeField]
    private GameObject teleportationArea;
    private const string movementTypeKey = "MovementType";
    private int movementType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Llamada a START");
        MovementTypeSetter();
    }

    public void OnValueChanged(Int32 newValue)
    {
        MovementTypeSetter();
    }

    private void MovementTypeSetter()
    {
        movementType = PlayerPrefs.GetInt(movementTypeKey, 0);
        Debug.Log("El tipo de movimiento guardado es " + movementType);
        if (movementType == 0)
        {
            teleportationArea.SetActive(true);
            movementProvider.SetActive(false);
        } else
        {
            teleportationArea.SetActive(false);
            movementProvider.SetActive(true);
        }
    }
}
