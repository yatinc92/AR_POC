using UnityEngine;

public class MoonPhaseController : MonoBehaviour
{
    [Header("Moon Phase Settings")]
    public float phase = 0.5f; // 0 = new moon, 0.5 = full moon

    private Renderer moonRenderer;
    private Material moonMaterial;

    void Start()
    {
        moonRenderer = GetComponent<Renderer>();
        if (moonRenderer != null)
        {
            // Create material instance
            moonMaterial = moonRenderer.material;
            UpdateMoonAppearance();
        }
    }

    public void SetPhase(float newPhase)
    {
        phase = Mathf.Clamp01(newPhase);
        UpdateMoonAppearance();
    }

    private void UpdateMoonAppearance()
    {
        if (moonMaterial != null)
        {
            // Simple phase visualization - darken part of the moon
            float illumination = Mathf.Abs(Mathf.Sin(phase * Mathf.PI));
            moonMaterial.color = new Color(illumination, illumination, illumination);

            // You can enhance this with custom shader for better phase visualization
        }
    }
}