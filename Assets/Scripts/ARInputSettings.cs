using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ARInputSettings", menuName = "Celestial Sphere/AR Input Settings")]
public class ARInputSettings : ScriptableObject
{
    [Header("Orientation Settings")]
    public float smoothFactor = 0.1f;
    public float updateFrequency = 10f;
    public float minimumMovementThreshold = 0.01f;

    [Header("Calibration Settings")]
    public bool autoCalibrateOnStart = true;
    public float calibrationTime = 3f;

    [Header("Filter Settings")]
    public bool useLowPassFilter = true;
    public float filterFactor = 0.5f;
}