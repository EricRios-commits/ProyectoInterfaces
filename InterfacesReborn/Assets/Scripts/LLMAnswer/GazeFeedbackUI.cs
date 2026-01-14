using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides visual feedback for the gaze interaction by displaying a radial fill indicator
/// that shows the progress of the gaze timer. This creates a "loading ring" effect
/// that gives the player visual confirmation that their gaze is being registered.
/// </summary>
[RequireComponent(typeof(Image))]
public class GazeFeedbackUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The GazeController to read progress from")]
    private GazeController gazeController;

    [Header("Visual Settings")]
    [SerializeField]
    [Tooltip("Color of the fill when actively gazing")]
    private Color activeColor = new Color(1f, 1f, 1f, 0.8f);

    [SerializeField]
    [Tooltip("Color of the fill when not gazing (usually transparent)")]
    private Color inactiveColor = new Color(1f, 1f, 1f, 0f);

    [SerializeField]
    [Tooltip("How quickly the color transitions between active and inactive states")]
    private float colorTransitionSpeed = 10f;

    [SerializeField]
    [Tooltip("Minimum alpha value when the indicator is visible")]
    [Range(0f, 1f)]
    private float minAlpha = 0.3f;

    [SerializeField]
    [Tooltip("Maximum alpha value when fully active")]
    [Range(0f, 1f)]
    private float maxAlpha = 1f;

    [SerializeField]
    [Tooltip("Optional: Scale animation when gazing starts")]
    private bool useScaleAnimation = true;

    [SerializeField]
    [Tooltip("Scale multiplier when fully active")]
    private float activeScale = 1.1f;

    [SerializeField]
    [Tooltip("How quickly the scale changes")]
    private float scaleTransitionSpeed = 8f;

    private Image fillImage;
    private Vector3 initialScale;
    private Color currentColor;
    private float currentScaleMultiplier = 1f;

    void Awake()
    {
        fillImage = GetComponent<Image>();
        initialScale = transform.localScale;
        currentColor = inactiveColor;

        // Ensure the Image component is configured correctly for radial fill
        if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[GazeFeedbackUI] Image on {gameObject.name} is not set to Filled type. Setting it now.");
            fillImage.type = Image.Type.Filled;
        }

        if (fillImage.fillMethod != Image.FillMethod.Radial360)
        {
            Debug.LogWarning($"[GazeFeedbackUI] Image on {gameObject.name} is not using Radial360 fill. Setting it now.");
            fillImage.fillMethod = Image.FillMethod.Radial360;
        }

        // Start with no fill
        fillImage.fillAmount = 0f;
        fillImage.color = inactiveColor;
    }

    void Start()
    {
        // Validate the reference
        if (gazeController == null)
        {
            Debug.LogError($"[GazeFeedbackUI] GazeController not assigned on {gameObject.name}. Please assign it in the inspector.");
        }
    }

    void Update()
    {
        if (gazeController == null)
            return;

        // Update fill amount based on gaze progress
        fillImage.fillAmount = gazeController.GazeProgress;

        // Determine target color based on gazing state
        Color targetColor = gazeController.IsGazing ? activeColor : inactiveColor;
        
        // Smoothly transition color
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
        
        // Apply alpha modulation based on fill amount if actively gazing
        if (gazeController.IsGazing)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, gazeController.GazeProgress);
            currentColor.a = alpha;
        }

        fillImage.color = currentColor;

        // Optional scale animation
        if (useScaleAnimation)
        {
            float targetScale = gazeController.IsGazing ? activeScale : 1f;
            currentScaleMultiplier = Mathf.Lerp(currentScaleMultiplier, targetScale, Time.deltaTime * scaleTransitionSpeed);
            transform.localScale = initialScale * currentScaleMultiplier;
        }
    }

    /// <summary>
    /// Manually set the GazeController reference. Useful for runtime setup.
    /// </summary>
    /// <param name="controller">The GazeController to track</param>
    public void SetGazeController(GazeController controller)
    {
        gazeController = controller;
    }

    /// <summary>
    /// Update the active color at runtime
    /// </summary>
    public void SetActiveColor(Color color)
    {
        activeColor = color;
    }

    /// <summary>
    /// Update the inactive color at runtime
    /// </summary>
    public void SetInactiveColor(Color color)
    {
        inactiveColor = color;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Configure the Image component correctly in the editor
    /// </summary>
    [ContextMenu("Setup Image Component")]
    private void SetupImageComponent()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillAmount = 0f;
            img.color = inactiveColor;
            Debug.Log($"[GazeFeedbackUI] Image component configured on {gameObject.name}");
        }
    }
#endif
}

