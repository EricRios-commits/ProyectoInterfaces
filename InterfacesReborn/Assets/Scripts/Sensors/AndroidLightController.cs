using UnityEngine;
using TMPro;

public class AndroidLightController : MonoBehaviour
{
    public Light worldLight;
    public TextMeshProUGUI debugText;
    
    [Header("Light Mapping Settings")]
    [Tooltip("Minimum light sensor value (in lux)")]
    public float minLux = 0f;
    
    [Tooltip("Maximum light sensor value (in lux)")]
    public float maxLux = 10000f;
    
    [Tooltip("Minimum Unity light intensity")]
    public float minIntensity = 0.1f;
    
    [Tooltip("Maximum Unity light intensity")]
    public float maxIntensity = 2.0f;
    
    [Tooltip("Smoothing factor (0-1, higher = smoother)")]
    public float smoothingFactor = 0.1f;

    private AndroidJavaObject lightSensor;
    private AndroidJavaObject sensorManager;
    private float currentLux = 0f;
    private float smoothBrightness = 1f;
    private bool sensorAvailable = false;

    void Start()
    {
        InitializeLightSensor();
    }

    void InitializeLightSensor()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Get the Unity player activity
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            // Get the sensor service
            sensorManager = activity.Call<AndroidJavaObject>("getSystemService", "sensor");
            
            // Get the light sensor (TYPE_LIGHT = 5)
            lightSensor = sensorManager.Call<AndroidJavaObject>("getDefaultSensor", 5);
            
            if (lightSensor != null)
            {
                // Register sensor listener
                AndroidJavaObject listener = new AndroidJavaObject("com.unity3d.player.UnityPlayer$LightSensorListener");
                bool registered = sensorManager.Call<bool>("registerListener", listener, lightSensor, 3); // SENSOR_DELAY_NORMAL = 3
                
                sensorAvailable = registered;
                Debug.Log("Android Light Sensor initialized: " + registered);
            }
            else
            {
                Debug.LogWarning("Light sensor not available on this device");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize light sensor: " + e.Message);
            sensorAvailable = false;
        }
#else
        Debug.Log("Light sensor only works on Android devices");
        sensorAvailable = false;
#endif
    }

    float GetAmbientLight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (sensorAvailable && lightSensor != null)
        {
            // This is a simplified approach - you'd need a proper Java plugin for real sensor data
            // For now, we'll use a placeholder
            return currentLux;
        }
#endif
        // Fallback: return medium brightness in editor
        return 1000f;
    }

    void Update()
    {
        // Get current ambient light level in lux
        float luxValue = GetAmbientLight();
        
        // Map lux to intensity range
        float normalizedBrightness = Mathf.InverseLerp(minLux, maxLux, luxValue);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedBrightness);
        
        // Smooth the transition
        smoothBrightness = Mathf.Lerp(smoothBrightness, targetIntensity, smoothingFactor);
        
        // Apply to world light
        if (worldLight != null)
        {
            worldLight.intensity = smoothBrightness;
        }
        
        // Update debug text
        if (debugText != null)
        {
            debugText.text = $"Sensor Available: {sensorAvailable}\n" +
                            $"Ambient Light: {luxValue:F1} lux\n" +
                            $"Normalized: {normalizedBrightness:F3}\n" +
                            $"Target Intensity: {targetIntensity:F3}\n" +
                            $"Smooth Intensity: {smoothBrightness:F3}\n" +
                            $"Light Intensity: {(worldLight != null ? worldLight.intensity.ToString("F3") : "N/A")}";
        }
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (sensorManager != null && lightSensor != null)
        {
            // Unregister sensor listener
            // sensorManager.Call("unregisterListener", listener);
        }
#endif
    }
}

