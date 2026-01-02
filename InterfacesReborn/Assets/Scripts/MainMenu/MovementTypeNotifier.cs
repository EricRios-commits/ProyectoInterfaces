using System;
using UnityEngine;

public class MovementTypeNotifier : MonoBehaviour
{
    public delegate void message(string moventMode);
    public event message MovementModeAlert;
    
    public void OnValueChanged(Int32 newValue)
    {
        if (newValue == 0)
        {
            MovementModeAlert("Teleport");
        }
        if (newValue == 1)
        {
            MovementModeAlert("Joystick");
        }
    }
}
