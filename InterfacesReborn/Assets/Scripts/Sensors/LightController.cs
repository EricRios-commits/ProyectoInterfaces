using UnityEngine;

public class LightController : MonoBehaviour
{
    public RenderTexture currentPassthrough; 
    public Light worldLight;

    private RenderTexture mipMapTexture;
    private Texture2D onePixelTexture;
    private float chrono;

    void Start()
    {
        chrono = 0f;
        mipMapTexture = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);
        mipMapTexture.useMipMap = true;
        mipMapTexture.autoGenerateMips = true;

        onePixelTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
    }

    float GetBrightness()
    {
        // Copiamos el passthrough a la RT con mipmaps
        Graphics.Blit(currentPassthrough, mipMapTexture);

        // Último nivel de mipmap (normalmente 1x1)
        int lastMip = mipMapTexture.mipmapCount - 1;

        // Copiar ese mipmap a nuestra textura 1x1
        Graphics.CopyTexture(mipMapTexture, lastMip, 0, onePixelTexture, 0, 0);

        // Leer el color promedio
        Color color = onePixelTexture.GetPixel(0, 0);
        return (color.r + color.g + color.b) / 3f; // 0..1 aprox
    }

    float smoothBrightness = 0f;

    void Update()
    {
        chrono += Time.deltaTime;
        if (chrono >= 0.5f)
        {
            // Leer brillo cada frame (puedes bajarlo a cada 0.2s si quieres)
            float brightness = GetBrightness();

            // Suavizado para evitar parpadeos
            smoothBrightness = Mathf.Lerp(smoothBrightness, brightness, 0.1f);
            chrono = 0f;          
        }
        // Debug.Log($"Brillo: {smoothBrightness}");
        worldLight.intensity = smoothBrightness;
    }
}
