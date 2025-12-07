using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CelestialSphereController : MonoBehaviour
{
    [Header("Celestial Objects")]
    public Transform celestialSphere;
    public Transform sun;
    public Light sunLight;
    public Transform moon;
    public Renderer moonRenderer;
   // public Transform milkyWay;
   // public Material milkyWayMaterial;

    [Header("Grid Settings")]
    public LineRenderer gridLinePrefab;
    public Material gridMaterial;
    public float sphereRadius = 10f;

    [Header("Line Width Settings")]
    [Range(0.01f, 0.5f)]
    public float latitudeLineWidth = 0.03f;
    [Range(0.01f, 0.5f)]
    public float longitudeLineWidth = 0.02f;
    [Range(0.01f, 0.5f)]
    public float equatorLineWidth = 0.04f;
    [Range(0.01f, 0.1f)]
    public float horizonLineWidth = 0.03f;
    [Range(0.01f, 0.8f)]
    public float poleMarkerSize = 0.1f;

    [Header("Visual Settings")]
    public Gradient skyGradient;
    public Color dayColor = Color.cyan;
    public Color nightColor = Color.black;

    [Header("AR Direction Settings")]
    public bool useARDirection = true;
    public float smoothFactor = 0.1f;
    public float minimumMovementThreshold = 0.01f;

    // Astronomical constants
    private const double DEG_TO_RAD = Math.PI / 180.0;
    private const double RAD_TO_DEG = 180.0 / Math.PI;
    private const double J2000 = 2451545.0;

    // Current settings
    private DateTime currentDateTime;
    private float latitude = 18.6056704f; // New York default
    private float longitude = 73.7804288f;

    // UI Components
    private Canvas canvas;
    private GameObject dashboardPanel;
    private Button toggleDashboardButton;
    private TextMeshProUGUI dateTimeText;
    private TextMeshProUGUI sunPositionText;
    private TextMeshProUGUI moonPositionText;
    private TextMeshProUGUI milkyWayPositionText;
    private TextMeshProUGUI deviceOrientationText;
    private TMP_InputField latInput, lonInput, dateInput, timeInput;
    private Button updateButton;

    // Grid lines storage
    private List<LineRenderer> gridLines = new List<LineRenderer>();
    private List<GameObject> directionLabels = new List<GameObject>();

    // AR Direction Tracking
    private Quaternion currentDeviceRotation = Quaternion.identity;
    private Quaternion smoothedDeviceRotation = Quaternion.identity;
    private Vector3 currentCompassDirection = Vector3.forward;
    private bool isOrientationInitialized = false;
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.1f; // Update 10 times per second

    // Sensor status
    private bool gyroEnabled = false;
    private bool compassEnabled = false;
    private float lastCompassHeading = 0f;
    private float initialCompassHeading = 0f;
    private bool initialAlignmentDone = false;
    // Screen size tracking
    private Vector2 lastScreenSize;
    private ScreenOrientation lastScreenOrientation;
    private float uiScaleFactor = 1f;

    [Header("Star Settings")]
    public bool showStars = true;
    public StarDatabase starDatabase; // Reference to your StarDatabase component

    private void InitializeStars()
    {
        if (showStars && starDatabase != null)
        {
            starDatabase.CreateStarObjects(celestialSphere, sphereRadius);
            Debug.Log("Stars initialized");
        }
        else if (showStars)
        {
            Debug.LogWarning("StarDatabase reference is missing!");
        }
    }

    private void UpdateStarPositions()
    {
        if (showStars && starDatabase != null)
        {
            starDatabase.UpdateLocationFromCelestialSphere();
            starDatabase.UpdateStarPositions(sphereRadius);
        }
    }
    void Start()
    {
        CalculateUIScaleFactor();
        InitializeSensors();
        InitializeCelestialSphere();
        CreateCanvas();
        InitializeDashboard();
        CreateGridSystem();
        CreateCardinalDirections();
        CreateHorizonLine();
        CreatePoles();
        CreateCelestialObjectLabels();

        currentDateTime = DateTime.Now;
        // Ensure dashboard input fields reflect the current system date/time
        if (dateInput != null)
        {
            dateInput.text = currentDateTime.ToString("yyyy-MM-dd");
        }
        if (timeInput != null)
        {
            timeInput.text = currentDateTime.ToString("HH:mm");
        }
        UpdateAllCelestialPositions();
       // AdjustForMobile();
        SetupARCamera();

        // Initialize stars
        InitializeStars();

        StartCoroutine(DelayedPositionUpdate());
    }
    private void CalculateUIScaleFactor()
    {
        // Calculate scale factor based on screen size
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Use the smaller dimension for consistent scaling
        float minDimension = Mathf.Min(screenWidth, screenHeight);
        float referenceMinDimension = 1080f; // Reference for 1920x1080

        uiScaleFactor = minDimension / referenceMinDimension;

        // Clamp scale factor to reasonable limits
        uiScaleFactor = Mathf.Clamp(uiScaleFactor, 0.5f, 2.0f);

        // Store current screen properties
        lastScreenSize = new Vector2(screenWidth, screenHeight);
        lastScreenOrientation = Screen.orientation;

        Debug.Log($"Screen: {screenWidth}x{screenHeight}, Scale Factor: {uiScaleFactor}");
    }

    private void CheckScreenSizeChange()
    {
        // Check if screen size or orientation changed
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y ||
            Screen.orientation != lastScreenOrientation)
        {
            Debug.Log("Screen size/orientation changed, recalculating UI scale");
            CalculateUIScaleFactor();
            RefreshUIForScreenChange();
        }
    }

    private void RefreshUIForScreenChange()
    {
        // Refresh all UI elements with new scale
        if (dashboardPanel != null)
        {
            UpdateDashboardUI();
        }
    }

    private void InitializeSensors()
    {
        Debug.Log("Initializing mobile sensors...");

        // Initialize gyroscope
        gyroEnabled = InitializeGyroscope();

        // Initialize compass
        compassEnabled = InitializeCompass();

        Debug.Log($"Sensors - Gyro: {gyroEnabled}, Compass: {compassEnabled}");
    }

    private bool InitializeGyroscope()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = 0.0167f; // 60 Hz
            Debug.Log("Gyroscope initialized successfully");
            return true;
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device");
            return false;
        }
    }

    private bool InitializeCompass()
    {
        // Compass is generally supported on mobile devices
        Input.compass.enabled = true;

        if (SystemInfo.supportsAccelerometer)
        {
            Debug.Log("Compass and accelerometer initialized");
            return true;
        }
        else
        {
            Debug.LogWarning("Accelerometer not supported, compass may be limited");
            return false;
        }
    }

    private IEnumerator DelayedPositionUpdate()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateAllCelestialPositions();
        Debug.Log("Delayed position update completed");
    }

    private void SetupARCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found for AR!");
            return;
        }

        // Configure camera for AR compatibility
        mainCamera.nearClipPlane = 0.01f;
        mainCamera.farClipPlane = 1000f;

        // Position camera appropriately for AR
        mainCamera.transform.position = new Vector3(0, 0, 0);

        // For AR, we need to ensure the celestial sphere is at a visible distance
        if (celestialSphere != null)
        {
            celestialSphere.position = new Vector3(0, 0, 5f); // Move sphere in front of camera
        }

        Debug.Log("AR Camera setup completed");
    }

    void Update()
    {
        UpdateDateTimeDisplay();
        UpdateSkyColor();

        // Check for screen size changes
        if (Time.frameCount % 30 == 0) // Check every 30 frames
        {
            CheckScreenSizeChange();
        }

        if (useARDirection)
        {
            UpdateDeviceOrientation();
            UpdateCelestialSphereOrientation();
        }
        else
        {
            // Update sidereal time rotation (traditional method)
            double lst = CalculateLocalSiderealTime();
            celestialSphere.localRotation = Quaternion.Euler(0, (float)lst, 0);
        }

        // Update labels every frame to ensure they stay with their objects
        UpdateCelestialObjectLabels();
    }

    #region AR Direction Tracking - REAL WORLD ALIGNED

    private void UpdateDeviceOrientation()
    {
        float currentTime = Time.time;
        if (currentTime - lastUpdateTime < UPDATE_INTERVAL)
            return;

        lastUpdateTime = currentTime;

        try
        {
            // Get current compass heading for real-world direction
            float currentHeading = GetCompassHeading();

            // Apply smoothing to compass heading to reduce jitter
            if (!isOrientationInitialized)
            {
                lastCompassHeading = currentHeading;
                isOrientationInitialized = true;
                initialAlignmentDone = true;
                Debug.Log($"Initial alignment - Heading: {currentHeading:F1}�");
            }
            else
            {
                // Smooth compass heading changes
                if (Mathf.Abs(currentHeading - lastCompassHeading) > 0.5f)
                {
                    lastCompassHeading = Mathf.LerpAngle(lastCompassHeading, currentHeading, smoothFactor);
                }
            }

            // Update orientation display
            UpdateOrientationDisplay();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error updating device orientation: {e.Message}");
        }
    }

    private Quaternion GetDeviceRotation()
    {
        // Use gyroscope if available for smooth rotation
        if (gyroEnabled && Input.gyro.enabled)
        {
            return GetGyroRotation();
        }

        // Fallback to accelerometer-based orientation
        return GetAccelerometerOrientation();
    }

    private Quaternion GetGyroRotation()
    {
        try
        {
            // Get gyro attitude and convert to Unity coordinate system
            Quaternion gyroAttitude = Input.gyro.attitude;

            // Convert from right-handed to left-handed system
            // This conversion makes the gyro data work properly in Unity
            return new Quaternion(gyroAttitude.x, gyroAttitude.y, -gyroAttitude.z, -gyroAttitude.w);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Gyro error: {e.Message}");
            return GetAccelerometerOrientation();
        }
    }

    private Quaternion GetAccelerometerOrientation()
    {
        try
        {
            Vector3 acceleration = Input.acceleration;

            if (acceleration.magnitude > minimumMovementThreshold)
            {
                // Get tilt from accelerometer
                Vector3 gravity = acceleration.normalized;
                Quaternion tilt = Quaternion.FromToRotation(Vector3.down, gravity);

                return tilt;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Accelerometer error: {e.Message}");
        }

        return Quaternion.identity;
    }

    private Vector3 GetCompassDirection()
    {
        float heading = GetCompassHeading();
        float headingRad = heading * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(headingRad), 0, Mathf.Cos(headingRad));
    }

    private float GetCompassHeading()
    {
        if (compassEnabled && Input.compass.enabled)
        {
            try
            {
                // Priority: true heading > magnetic heading
                float heading = Input.compass.trueHeading;
                if (heading == 0f || float.IsNaN(heading) || heading > 360f)
                {
                    heading = Input.compass.magneticHeading;
                }

                // Ensure heading is in 0-360 range
                heading %= 360f;
                if (heading < 0) heading += 360f;

                return heading;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Compass error: {e.Message}");
            }
        }

        // Fallback: use device rotation (less accurate)
        return smoothedDeviceRotation.eulerAngles.y;
    }

    private void UpdateCelestialSphereOrientation()
    {
        if (!isOrientationInitialized || !initialAlignmentDone)
            return;

        try
        {
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime < UPDATE_INTERVAL)
                return;

            lastUpdateTime = currentTime;

            // Get current compass heading - this is our real-world reference
            float currentHeading = GetCompassHeading();

            // KEY FIX: The celestial sphere should rotate OPPOSITE to the device rotation
            // to maintain real-world alignment

            // Get device rotation (how the device is oriented in 3D space)
            Quaternion deviceRotation = GetDeviceRotation();

            // Calculate local sidereal time for celestial motion
            double lst = CalculateLocalSiderealTime();

            // Create the target rotation:
            // 1. Start with sidereal time (celestial sphere rotation due to Earth's rotation)
            // 2. Apply compass correction to align with real-world North
            // 3. Compensate for device tilt to keep horizon level

            Quaternion siderealRotation = Quaternion.Euler(0, (float)lst, 0);

            // Compass correction: rotate the sphere so that when device points North,
            // the celestial sphere's North aligns with real North
            Quaternion compassCorrection = Quaternion.Euler(0, -currentHeading, 0);

            // Combine: first apply sidereal rotation, then compass correction
            Quaternion celestialRotation = compassCorrection * siderealRotation;

            // Apply device tilt compensation to keep the horizon level
            // Extract just the tilt (pitch and roll) from device rotation, ignore yaw
            Vector3 deviceEuler = deviceRotation.eulerAngles;
            Quaternion tiltCompensation = Quaternion.Euler(-deviceEuler.x, 0, -deviceEuler.z);

            // Final rotation: apply tilt compensation to keep horizon level
            Quaternion targetRotation = tiltCompensation * celestialRotation;

            // Apply smooth rotation to celestial sphere
            celestialSphere.rotation = Quaternion.Slerp(
                celestialSphere.rotation,
                targetRotation,
                smoothFactor * 0.3f // Conservative smoothing for stability
            );

            // Debug information
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Real-World Alignment - Compass: {currentHeading:F1}�, " +
                         $"Sidereal: {lst:F1}�, " +
                         $"Device Tilt: ({deviceEuler.x:F1}, {deviceEuler.z:F1})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating celestial sphere orientation: {e.Message}");
        }
    }

    private void UpdateOrientationDisplay()
    {
        if (deviceOrientationText != null)
        {
            float heading = GetCompassHeading();
            Vector3 deviceEuler = GetDeviceRotation().eulerAngles;

            // Convert heading to cardinal direction
            string cardinal = GetCardinalDirection(heading);

            string orientationInfo = $"Compass: {heading:F1}� {cardinal}\n";
            orientationInfo += $"Device: Pitch:{deviceEuler.x:F1}�, Roll:{deviceEuler.z:F1}�\n";
            orientationInfo += $"Sidereal Time: {CalculateLocalSiderealTime():F1}�\n";
            orientationInfo += $"AR Mode: {(useARDirection ? "ACTIVE" : "OFF")}";
            orientationInfo += $"\nLocation: {latitude:F4}�, {longitude:F4}�";

            deviceOrientationText.text = orientationInfo;
        }
    }

    private string GetCardinalDirection(float heading)
    {
        string[] directions = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
                           "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        int index = (int)((heading + 11.25f) / 22.5f) % 16;
        return directions[index];
    }

    private bool IsSignificantMovement(Quaternion newRotation, Quaternion oldRotation)
    {
        float angleDifference = Quaternion.Angle(newRotation, oldRotation);
        return angleDifference > minimumMovementThreshold;
    }

    #endregion
    #region Milky Way Visualization

    /*private void CreateEnhancedMilkyWay()
    {
        if (milkyWay != null)
        {
            Destroy(milkyWay.gameObject);
        }

        // Create a more accurate Milky Way representation
        GameObject milkyWayObj = new GameObject("MilkyWay");
        milkyWayObj.transform.SetParent(celestialSphere);
        milkyWayObj.transform.localPosition = Vector3.zero;

        // Create multiple bands for better Milky Way representation
        CreateMilkyWayBand(milkyWayObj.transform, 60f, 0.15f, new Color(0.2f, 0.2f, 0.4f, 0.8f)); // Outer band
        CreateMilkyWayBand(milkyWayObj.transform, 55f, 0.12f, new Color(0.3f, 0.3f, 0.5f, 0.7f)); // Middle band
        CreateMilkyWayBand(milkyWayObj.transform, 50f, 0.10f, new Color(0.4f, 0.4f, 0.6f, 0.6f)); // Inner band
        CreateMilkyWayBand(milkyWayObj.transform, 62f, 0.08f, new Color(0.1f, 0.1f, 0.3f, 0.4f)); // Halo effect

        // Add galactic center highlight
        CreateGalacticCenter(milkyWayObj.transform);

        milkyWay = milkyWayObj.transform;

        Debug.Log("Enhanced Milky Way created");
    }

    private void CreateMilkyWayBand(Transform parent, float inclination, float width, Color color)
    {
        GameObject band = new GameObject("MilkyWayBand");
        band.transform.SetParent(parent);
        band.transform.localPosition = Vector3.zero;

        // Create a torus or cylinder for the band
        MeshFilter meshFilter = band.AddComponent<MeshFilter>();
        MeshRenderer renderer = band.AddComponent<MeshRenderer>();

        // Create a custom mesh for the Milky Way band
        Mesh mesh = CreateMilkyWayMesh(width, 360f, 72);
        meshFilter.mesh = mesh;

        // Set material
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;
        renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        renderer.material.SetInt("_ZWrite", 0);
        renderer.material.DisableKeyword("_ALPHATEST_ON");
        renderer.material.EnableKeyword("_ALPHABLEND_ON");
        renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        renderer.material.renderQueue = 3000;

        // Position along galactic plane
        band.transform.localRotation = Quaternion.Euler(inclination, 0, 0);
    }

    private Mesh CreateMilkyWayMesh(float width, float arc, int segments)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        float radius = sphereRadius * 0.95f; // Slightly inside celestial sphere
        float angleStep = arc * Mathf.Deg2Rad / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Create two vertices for width
            vertices.Add(new Vector3(x, -width / 2, z));
            vertices.Add(new Vector3(x, width / 2, z));

            // UV mapping
            float u = (float)i / segments;
            uv.Add(new Vector2(u, 0));
            uv.Add(new Vector2(u, 1));

            if (i < segments)
            {
                int baseIndex = i * 2;
                // First triangle
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                // Second triangle
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 2);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }*/

    private void CreateGalacticCenter(Transform parent)
    {
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.name = "GalacticCenter";
        center.transform.SetParent(parent);
        center.transform.localScale = Vector3.one * 0.5f;

        // Position at galactic center coordinates
        Vector3 galacticCenterPos = CalculateGalacticCenterPosition();
        center.transform.localPosition = galacticCenterPos.normalized * sphereRadius * 0.9f;

        Renderer renderer = center.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(0.8f, 0.8f, 1f, 0.6f);
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.8f, 1f));
    }

    private Vector3 CalculateGalacticCenterPosition()
    {
        DateTime utc = currentDateTime.ToUniversalTime();
        double jd = CalculateJulianDate(utc);

        // Galactic center coordinates (Sagittarius A*)
        double galacticCenterRA = 266.41667;  // 17h 45m 40s
        double galacticCenterDec = -28.0;     // -29� 00' 28"

        return EquatorialToHorizontal(galacticCenterRA, galacticCenterDec, jd);
    }

    /*private void UpdateMilkyWayPosition()
    {
        if (milkyWay != null)
        {
            // Update Milky Way position based on current time
            DateTime utc = currentDateTime.ToUniversalTime();
            double jd = CalculateJulianDate(utc);

            Vector3 milkyWayPos = CalculateGalacticCenterPosition();
            milkyWay.localRotation = Quaternion.Euler(60, (float)CalculateLocalSiderealTime() * 0.5f, 0);
        }
    }*/

    #endregion
    #region Celestial Object Labels - CORRECTED FOR SPHERE VIEW

    private void CreateCelestialObjectLabels()
    {
        // Create Sun label
        CreateCelestialLabel("SunLabel", "Sun", Color.yellow, sun);

        // Create Moon label
        CreateCelestialLabel("MoonLabel", "Moon", Color.white, moon);

        // Create Milky Way label
      //  CreateCelestialLabel("MilkyWayLabel", "Milky Way", Color.cyan, milkyWay);
    }

    private void CreateCelestialLabel(string labelName, string displayText, Color color, Transform targetObject)
    {
        if (targetObject == null) return;

        GameObject labelObj = new GameObject(labelName);
        labelObj.transform.SetParent(celestialSphere);

        // Add canvas for better text rendering
        Canvas canvas = labelObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.01f; // Scale down the canvas

        // Add text component
        TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = displayText;
        text.fontSize = 60;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;

        // Set up rect transform
        RectTransform rectTransform = labelObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(450, 150); // Adequate size for text

        // Position the label slightly above the object (further out from sphere center)
        Vector3 objectDirection = targetObject.localPosition.normalized;
        labelObj.transform.localPosition = objectDirection * (sphereRadius + 0.5f); // Further out for better visibility

        // Add billboard component to make text always face camera
        BillboardText billboard = labelObj.AddComponent<BillboardText>();

        // Add to direction labels for management
        directionLabels.Add(labelObj);

        Debug.Log($"Created {labelName} at position: {labelObj.transform.localPosition}");
    }

    private void UpdateCelestialObjectLabels()
    {
        foreach (GameObject label in directionLabels)
        {
            if (label != null && label.transform.parent == celestialSphere)
            {
                // Update position for celestial objects
                if (label.name == "SunLabel" && sun != null)
                {
                    UpdateLabelPosition(label, sun);
                }
                else if (label.name == "MoonLabel" && moon != null)
                {
                    UpdateLabelPosition(label, moon);
                }
               // else if (label.name == "MilkyWayLabel" && milkyWay != null)
                //{
                  //  UpdateLabelPosition(label, milkyWay);
                //}
            }
        }
    }

    private void UpdateLabelPosition(GameObject label, Transform targetObject)
    {
        if (targetObject == null) return;

        // Position label slightly beyond the celestial object
        Vector3 objectDirection = targetObject.localPosition.normalized;
        label.transform.localPosition = objectDirection * (sphereRadius + 0.5f);

        // The billboard component will handle rotation to face camera
    }

    // Billboard component to make text always face the camera
    public class BillboardText : MonoBehaviour
    {
        private Camera mainCamera;
        private Transform cameraTransform;

        void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        void Update()
        {
            if (cameraTransform != null)
            {
                // Make the text face the camera while maintaining up direction
                transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                               cameraTransform.rotation * Vector3.up);
            }
        }
    }

    #endregion

    private void InitializeCelestialSphere()
    {
        if (celestialSphere == null)
        {
            GameObject sphere = new GameObject("CelestialSphere");
            celestialSphere = sphere.transform;
            celestialSphere.position = new Vector3(0, 0, 8f);
            celestialSphere.localScale = Vector3.one * 2f;
        }
        CreateCelestialObjectsIfMissing();
    }

    private void CreateCelestialObjectsIfMissing()
    {
        if (sun == null)
        {
            GameObject sunObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sunObj.name = "Sun";
            sunObj.transform.SetParent(celestialSphere);
            sunObj.transform.localPosition = Vector3.zero;
            sunObj.transform.localScale = Vector3.one * 1.0f;

            Renderer sunRenderer = sunObj.GetComponent<Renderer>();
            sunRenderer.material = new Material(Shader.Find("Standard"));
            sunRenderer.material.color = Color.yellow;
            sunRenderer.material.EnableKeyword("_EMISSION");
            sunRenderer.material.SetColor("_EmissionColor", Color.yellow * 2f);
            sunRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            sun = sunObj.transform;

            GameObject lightObj = new GameObject("SunLight");
            lightObj.transform.SetParent(celestialSphere);
            lightObj.transform.localPosition = Vector3.zero;
            sunLight = lightObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.5f;
            sunLight.color = new Color(1f, 0.95f, 0.8f);
            sunLight.shadows = LightShadows.Soft;
        }

        // Create Moon if missing
        if (moon == null)
        {
            GameObject moonObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moonObj.name = "Moon";
            moonObj.transform.SetParent(celestialSphere);
            moonObj.transform.localPosition = Vector3.zero;
            moonObj.transform.localScale = Vector3.one * 0.8f; // Larger size

            moonRenderer = moonObj.GetComponent<Renderer>();
            moonRenderer.material = new Material(Shader.Find("Standard"));
            moonRenderer.material.color = new Color(0.8f, 0.8f, 0.8f);
            moonRenderer.material.EnableKeyword("_EMISSION");
            moonRenderer.material.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.3f));

            moon = moonObj.transform;

            // Add MoonPhaseController
            MoonPhaseController moonPhase = moonObj.AddComponent<MoonPhaseController>();

            Debug.Log("Moon created with enhanced visibility");
        }

        // Create Milky Way if missing
       /* if (milkyWay == null)
        {
            // Use a cylinder instead of quad for better 3D appearance
            GameObject milkyWayObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            milkyWayObj.name = "MilkyWay";
            milkyWayObj.transform.SetParent(celestialSphere);
            milkyWayObj.transform.localPosition = Vector3.zero;
            milkyWayObj.transform.localScale = new Vector3(15f, 0.1f, 15f); // Wide and thin
            milkyWayObj.transform.localRotation = Quaternion.Euler(60, 0, 0);

            Renderer mwRenderer = milkyWayObj.GetComponent<Renderer>();
            mwRenderer.material = new Material(Shader.Find("Standard"));
            mwRenderer.material.color = new Color(0.3f, 0.3f, 0.5f, 0.6f);
            mwRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mwRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mwRenderer.material.SetInt("_ZWrite", 0);
            mwRenderer.material.DisableKeyword("_ALPHATEST_ON");
            mwRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            mwRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mwRenderer.material.renderQueue = 3000;

            milkyWay = milkyWayObj.transform;

            Debug.Log("Milky Way created with enhanced visibility");
        }*/
    }

    public void UpdateAllCelestialPositions()
    {
        Debug.Log("Updating all celestial positions...");

        // Force update of all positions
        UpdateSunPosition();
        UpdateMoonPosition();
        UpdatePositionDisplays();
        UpdateCelestialObjectLabels(); // Update labels when positions change

        // Update star positions
        UpdateStarPositions();

        Debug.Log($"Sun position: {sun?.localPosition}");
        Debug.Log($"Moon position: {moon?.localPosition}");
    }

    private void OnUpdateButtonClicked()
    {
        Debug.Log("Update button clicked!");

        bool success = true;
        string message = "Positions updated successfully!";

        // Parse latitude
        if (float.TryParse(latInput.text, out float newLat))
        {
            latitude = Math.Max(-90, Math.Min(90, newLat));
        }
        else
        {
            success = false;
            message = "Invalid latitude format!";
            Debug.LogError("Invalid latitude: " + latInput.text);
        }

        // Parse longitude
        if (success && float.TryParse(lonInput.text, out float newLon))
        {
            longitude = Math.Max(-180, Math.Min(180, newLon));
        }
        else if (success)
        {
            success = false;
            message = "Invalid longitude format!";
            Debug.LogError("Invalid longitude: " + lonInput.text);
        }

        // Parse date and time
        if (success)
        {
            string dateTimeString = dateInput.text + " " + timeInput.text;
            if (DateTime.TryParse(dateTimeString, out DateTime newDateTime))
            {
                currentDateTime = newDateTime;
            }
            else
            {
                success = false;
                message = "Invalid date/time format!";
                Debug.LogError("Invalid date/time: " + dateTimeString);
            }
        }

        if (success)
        {
            UpdateAllCelestialPositions();
            // Provide visual feedback
            StartCoroutine(ShowUpdateSuccess());
        }
        else
        {
            // Show error feedback
            StartCoroutine(ShowUpdateError(message));
        }
    }

    private void UpdateSunPosition()
    {
        if (sun == null)
        {
            Debug.LogError("Sun is null in UpdateSunPosition!");
            return;
        }

        try
        {
            DateTime utc = currentDateTime.ToUniversalTime();
            double jd = CalculateJulianDate(utc);

            double[] sunEcliptic = CalculateSunPosition(jd);
            double[] sunEquatorial = EclipticToEquatorial(sunEcliptic[0], sunEcliptic[1], jd);
            Vector3 sunPos = EquatorialToHorizontal(sunEquatorial[0], sunEquatorial[1], jd);

            sun.localPosition = sunPos.normalized * sphereRadius;

            // Update light intensity based on sun altitude
            float altitude = sunPos.y;
            sunLight.intensity = Mathf.Clamp01((altitude + 6f) / 12f);

            Debug.Log($"Sun position calculated: {sun.localPosition}, Altitude: {altitude:F1}�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating sun position: {e.Message}");
        }
    }

    private void UpdateMoonPosition()
    {
        if (moon == null)
        {
            Debug.LogError("Moon is null in UpdateMoonPosition!");
            return;
        }

        try
        {
            DateTime utc = currentDateTime.ToUniversalTime();
            double jd = CalculateJulianDate(utc);

            double[] moonPos = CalculateMoonPosition(jd);
            double[] moonEquatorial = EclipticToEquatorial(moonPos[0], moonPos[1], jd);
            Vector3 moonHorizontal = EquatorialToHorizontal(moonEquatorial[0], moonEquatorial[1], jd);

            moon.localPosition = moonHorizontal.normalized * sphereRadius;

            // Update moon phase
            UpdateMoonPhase(jd);

            Debug.Log($"Moon position calculated: {moon.localPosition}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating moon position: {e.Message}");
        }
    }

 

    private void UpdateMoonPhase(double jd)
    {
        double phase = CalculateMoonPhase(jd);

        MoonPhaseController moonPhase = moon.GetComponent<MoonPhaseController>();
        if (moonPhase != null)
        {
            moonPhase.SetPhase((float)phase);
        }
    }

    #region Core Astronomical Algorithms

    private double[] CalculateSunPosition(double jd)
    {
        double T = (jd - J2000) / 36525.0;

        // Mean anomaly
        double M = 357.52911 + 35999.05029 * T - 0.0001537 * T * T;
        M %= 360;
        if (M < 0) M += 360;

        // Mean longitude
        double L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T * T;
        L0 %= 360;
        if (L0 < 0) L0 += 360;

        // Equation of center
        double C = (1.914602 - 0.004817 * T - 0.000014 * T * T) * Math.Sin(M * DEG_TO_RAD)
                 + (0.019993 - 0.000101 * T) * Math.Sin(2 * M * DEG_TO_RAD)
                 + 0.000289 * Math.Sin(3 * M * DEG_TO_RAD);

        // True longitude
        double L = L0 + C;
        double B = 0; // Ecliptic latitude

        return new double[] { L, B };
    }

    private double[] CalculateMoonPosition(double jd)
    {
        double T = (jd - J2000) / 36525.0;

        double D = 297.8501921 + 445267.1114034 * T;
        double M = 357.5291092 + 35999.0502909 * T;
        double Mprime = 134.9633964 + 477198.8675055 * T;
        double Lprime = 218.3164477 + 481267.88123421 * T;

        // Ecliptic longitude
        double lambda = Lprime + 6.289 * Math.Sin(Mprime * DEG_TO_RAD)
                      + 1.274 * Math.Sin((2 * D - Mprime) * DEG_TO_RAD)
                      + 0.658 * Math.Sin(2 * D * DEG_TO_RAD);

        // Ecliptic latitude
        double beta = 5.128 * Math.Sin((2 * D - Mprime) * DEG_TO_RAD)
                    + 0.281 * Math.Sin(2 * D * DEG_TO_RAD);

        return new double[] { lambda, beta };
    }

    private double CalculateMoonPhase(double jd)
    {
        double T = (jd - J2000) / 36525.0;
        double D = 297.8501921 + 445267.1114034 * T;
        double M = 357.5291092 + 35999.0502909 * T;
        double Mprime = 134.9633964 + 477198.8675055 * T;

        double phase = (D + Mprime - M) % 360;
        if (phase < 0) phase += 360;

        return phase / 360.0;
    }

    private double[] EclipticToEquatorial(double lambda, double beta, double jd)
    {
        double T = (jd - J2000) / 36525.0;
        double epsilon = 23.43929111 - 0.013004167 * T;

        double sinEpsilon = Math.Sin(epsilon * DEG_TO_RAD);
        double cosEpsilon = Math.Cos(epsilon * DEG_TO_RAD);

        double sinLambda = Math.Sin(lambda * DEG_TO_RAD);
        double cosLambda = Math.Cos(lambda * DEG_TO_RAD);
        double sinBeta = Math.Sin(beta * DEG_TO_RAD);
        double cosBeta = Math.Cos(beta * DEG_TO_RAD);

        // Right Ascension
        double ra = Math.Atan2(
            sinLambda * cosEpsilon - Math.Tan(beta * DEG_TO_RAD) * sinEpsilon,
            cosLambda
        ) * RAD_TO_DEG;
        if (ra < 0) ra += 360;

        // Declination
        double dec = Math.Asin(
            sinBeta * cosEpsilon + cosBeta * sinEpsilon * sinLambda
        ) * RAD_TO_DEG;

        return new double[] { ra, dec };
    }

    private Vector3 EquatorialToHorizontal(double ra, double dec, double jd)
    {
        double lst = CalculateLocalSiderealTime();
        double ha = lst - ra;
        if (ha < 0) ha += 360;

        double haRad = ha * DEG_TO_RAD;
        double decRad = dec * DEG_TO_RAD;
        double latRad = latitude * DEG_TO_RAD;

        // Altitude
        double sinAlt = Math.Sin(decRad) * Math.Sin(latRad) +
                       Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(haRad);
        double alt = Math.Asin(sinAlt) * RAD_TO_DEG;

        // Azimuth
        double cosAz = (Math.Sin(decRad) - Math.Sin(latRad) * Math.Sin(alt * DEG_TO_RAD)) /
                      (Math.Cos(latRad) * Math.Cos(alt * DEG_TO_RAD));
        cosAz = Math.Max(-1, Math.Min(1, cosAz));

        double az = Math.Acos(cosAz) * RAD_TO_DEG;
        if (Math.Sin(haRad) > 0) az = 360 - az;

        // Convert to Cartesian
        double altRad = alt * DEG_TO_RAD;
        double azRad = az * DEG_TO_RAD;

        return new Vector3(
            (float)(Math.Cos(altRad) * Math.Sin(azRad)),
            (float)(Math.Sin(altRad)),
            (float)(Math.Cos(altRad) * Math.Cos(azRad))
        );
    }

    private double CalculateLocalSiderealTime()
    {
        DateTime utc = currentDateTime.ToUniversalTime();
        double jd = CalculateJulianDate(utc);

        double T = (jd - J2000) / 36525.0;
        double GST = 280.46061837 + 360.98564736629 * (jd - 2451545.0) +
                    0.000387933 * T * T - T * T * T / 38710000.0;

        double LST = (GST + longitude) % 360;
        if (LST < 0) LST += 360;

        return LST;
    }

    private double CalculateJulianDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;
        double hour = date.Hour + date.Minute / 60.0 + date.Second / 3600.0;

        if (month <= 2)
        {
            year--;
            month += 12;
        }

        int a = year / 100;
        int b = 2 - a + a / 4;

        return Math.Floor(365.25 * (year + 4716)) +
               Math.Floor(30.6001 * (month + 1)) +
               day + b - 1524.5 + hour / 24.0;
    }

    #endregion


    #region Grid and Visual Elements

    private void CreateGridSystem()
    {
        CreateLatitudeLines();
        CreateLongitudeLines();
        CreateEquatorLine(); // Add equator for better reference
    }

    private void CreateLatitudeLines()
    {
        for (int lat = -80; lat <= 80; lat += 20)
        {
            if (lat == 0) continue; // Skip equator as we'll create it separately

            LineRenderer line = CreateGridLine("Latitude_" + lat, new Color(0.2f, 0.2f, 0.8f, 0.6f));
            line.positionCount = 72;
            line.loop = true;
            line.startWidth = latitudeLineWidth; // Use customizable width
            line.endWidth = latitudeLineWidth;

            float radius = Mathf.Cos(lat * Mathf.Deg2Rad);
            float height = Mathf.Sin(lat * Mathf.Deg2Rad);

            for (int i = 0; i < 72; i++)
            {
                float angle = i * 5 * Mathf.Deg2Rad;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                ) * sphereRadius;
                line.SetPosition(i, point);
            }
        }
    }

    private void CreateLongitudeLines()
    {
        for (int lon = 0; lon < 360; lon += 30)
        {
            LineRenderer line = CreateGridLine("Longitude_" + lon, new Color(0.2f, 0.8f, 0.2f, 0.6f));
            line.positionCount = 37;
            line.startWidth = longitudeLineWidth;
            line.endWidth = longitudeLineWidth;

            for (int i = 0; i < 37; i++)
            {
                float lat = -90 + i * 5;
                float radLat = lat * Mathf.Deg2Rad;
                float radLon = lon * Mathf.Deg2Rad;

                Vector3 point = new Vector3(
                    Mathf.Cos(radLat) * Mathf.Cos(radLon),
                    Mathf.Sin(radLat),
                    Mathf.Cos(radLat) * Mathf.Sin(radLon)
                ) * sphereRadius;
                line.SetPosition(i, point);
            }
        }
    }

    private void CreateEquatorLine()
    {
        LineRenderer equator = CreateGridLine("Equator", new Color(1f, 1f, 0f, 0.8f));
        equator.positionCount = 72;
        equator.loop = true;
        equator.startWidth = equatorLineWidth; // Thicker equator line
        equator.endWidth = equatorLineWidth;

        for (int i = 0; i < 72; i++)
        {
            float angle = i * 5 * Mathf.Deg2Rad;
            Vector3 point = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * sphereRadius;
            equator.SetPosition(i, point);
        }
    }

    private LineRenderer CreateGridLine(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(celestialSphere);
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        // Use a simple shader that works well on mobile
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.material.color = color;

        line.startColor = color;
        line.endColor = color;
        line.useWorldSpace = false;
        line.loop = false;

        gridLines.Add(line);
        return line;
    }

    private void CreateCardinalDirections()
    {
        string[] directions = { "N", "E", "S", "W", "NE", "SE", "SW", "NW" };
        Vector3[] positions = {
        Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
        (Vector3.forward + Vector3.right).normalized,
        (Vector3.back + Vector3.right).normalized,
        (Vector3.back + Vector3.left).normalized,
        (Vector3.forward + Vector3.left).normalized
    };

        for (int i = 0; i < directions.Length; i++)
        {
            CreateDirectionLabel3D(directions[i], positions[i] * sphereRadius * 1.1f);
        }
    }

    private void CreateDirectionLabel3D(string direction, Vector3 position)
    {
        GameObject textObj = new GameObject("Direction_" + direction);
        textObj.transform.SetParent(celestialSphere);
        textObj.transform.localPosition = position;

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = direction;
        text.fontSize = 30;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Add billboard behavior
        textObj.AddComponent<BillboardText>();
        directionLabels.Add(textObj);
    }

    private void CreatePoleLabels()
    {
        // North Pole Label
        CreatePoleLabel("North Pole", Vector3.forward * sphereRadius * 1.2f, Color.blue);

        // South Pole Label
        CreatePoleLabel("South Pole", Vector3.back * sphereRadius * 1.2f, Color.red);
    }

    private void CreatePoleLabel(string labelText, Vector3 position, Color color)
    {
        GameObject textObj = new GameObject("PoleLabel_" + labelText.Replace(" ", ""));
        textObj.transform.SetParent(celestialSphere);
        textObj.transform.localPosition = position;

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = labelText;
        text.fontSize = 28; // Slightly larger font for pole labels
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
        text.fontStyle = FontStyle.Bold;

        // Make text face outward
        textObj.transform.LookAt(textObj.transform.position * 2);
        directionLabels.Add(textObj);
    }

    private float GetAltitude(Transform obj)
    {
        if (obj == null) return 0f;

        Vector3 pos = obj.localPosition.normalized;
        float altitude = Mathf.Asin(pos.y) * Mathf.Rad2Deg;
        return altitude;
    }

    private float GetAzimuth(Transform obj)
    {
        if (obj == null) return 0f;

        Vector3 pos = obj.localPosition.normalized;
        float azimuth = Mathf.Atan2(pos.x, pos.z) * Mathf.Rad2Deg;

        // Convert from [-180, 180] to [0, 360]
        if (azimuth < 0)
            azimuth += 360f;

        return azimuth;
    }

    private void CreateHorizonLine()
    {
        LineRenderer horizon = CreateGridLine("Horizon", Color.green);
        horizon.positionCount = 72;
        horizon.loop = true;
        horizon.startWidth = horizonLineWidth; // Use customizable width
        horizon.endWidth = horizonLineWidth;

        for (int i = 0; i < 72; i++)
        {
            float angle = i * 5 * Mathf.Deg2Rad;
            Vector3 point = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * sphereRadius;
            horizon.SetPosition(i, point);
        }
    }

    private void CreatePoles()
    {
        // North Pole - positioned horizontally at the front
        GameObject northPole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        northPole.name = "NorthPole";
        northPole.transform.SetParent(celestialSphere);
        northPole.transform.localPosition = Vector3.forward * sphereRadius;
        northPole.transform.localScale = Vector3.one * poleMarkerSize;
        northPole.GetComponent<Renderer>().material.color = Color.blue;

        // South Pole - positioned horizontally at the back
        GameObject southPole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        southPole.name = "SouthPole";
        southPole.transform.SetParent(celestialSphere);
        southPole.transform.localPosition = Vector3.back * sphereRadius;
        southPole.transform.localScale = Vector3.one * poleMarkerSize;
        southPole.GetComponent<Renderer>().material.color = Color.red;

        // Create pole labels
        CreatePoleLabels();
    }

    /// <summary>
    /// Updates all grid line widths with current inspector values
    /// Call this method if you change line widths at runtime
    /// </summary>
    public void UpdateGridLineWidths()
    {
        foreach (LineRenderer line in gridLines)
        {
            if (line != null)
            {
                string lineName = line.gameObject.name;

                if (lineName.StartsWith("Latitude"))
                {
                    line.startWidth = latitudeLineWidth;
                    line.endWidth = latitudeLineWidth;
                }
                else if (lineName.StartsWith("Longitude"))
                {
                    line.startWidth = longitudeLineWidth;
                    line.endWidth = longitudeLineWidth;
                }
                else if (lineName.StartsWith("Equator"))
                {
                    line.startWidth = equatorLineWidth;
                    line.endWidth = equatorLineWidth;
                }
                else if (lineName.StartsWith("Horizon"))
                {
                    line.startWidth = horizonLineWidth;
                    line.endWidth = horizonLineWidth;
                }
            }
        }

        // Update pole sizes
        UpdatePoleSizes();

        Debug.Log("Grid line widths updated");
    }

    private void UpdatePoleSizes()
    {
        Transform northPole = celestialSphere.Find("NorthPole");
        Transform southPole = celestialSphere.Find("SouthPole");

        if (northPole != null)
            northPole.localScale = Vector3.one * poleMarkerSize;

        if (southPole != null)
            southPole.localScale = Vector3.one * poleMarkerSize;
    }

    private void UpdateSkyColor()
    {
        // Simple day/night cycle based on sun position
        float sunAltitude = sun.localPosition.y;
        float t = Mathf.Clamp01((sunAltitude + 0.1f) / 0.2f);
        Camera.main.backgroundColor = Color.Lerp(nightColor, dayColor, t);
    }

    #endregion

    #region UI System - Enhanced with Larger Elements
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.referencePixelsPerUnit = 100;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add safe area handler
        SafeAreaHandler safeArea = canvasObj.AddComponent<SafeAreaHandler>();
    }

    private void InitializeDashboard()
    {
        CreateDashboardPanel();
        CreateToggleButton();
    }

    private void CreateDashboardPanel()
    {
        // Main panel with responsive anchors
        dashboardPanel = CreateUIObject("DashboardPanel", canvas.transform);
        RectTransform panelRect = dashboardPanel.AddComponent<RectTransform>();

        // Responsive anchors based on screen orientation
        if (Screen.width > Screen.height) // Landscape
        {
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.45f, 0.98f); // Side panel in landscape
        }
        else // Portrait
        {
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.98f, 0.50f); // Bottom panel in portrait
        }
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = dashboardPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        panelImage.raycastTarget = true;

        // Vertical layout with scaled spacing
        VerticalLayoutGroup verticalLayout = dashboardPanel.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(
            Mathf.RoundToInt(20 * uiScaleFactor),
            Mathf.RoundToInt(20 * uiScaleFactor),
            Mathf.RoundToInt(20 * uiScaleFactor),
            Mathf.RoundToInt(20 * uiScaleFactor)
        );
        verticalLayout.spacing = 12f * uiScaleFactor;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        // Create all UI sections
        CreateHeaderSection();
        CreateDateTimeSection();
        CreateLocationSection();
        CreateDateTimeInputSection();
        CreatePositionDisplaySection();
        CreateUpdateButtonSection();
    }

    private void UpdateDashboardUI()
    {
        // Update panel layout based on orientation
        RectTransform panelRect = dashboardPanel.GetComponent<RectTransform>();
        if (Screen.width > Screen.height) // Landscape
        {
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.45f, 0.98f);
        }
        else // Portrait
        {
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.98f, 0.50f);
        }

        // Update all font sizes
        UpdateAllFontSizes();
    }

    private void UpdateAllFontSizes()
    {
        // Update header
        TextMeshProUGUI headerText = dashboardPanel.transform.Find("Header")?.GetComponent<TextMeshProUGUI>();
        if (headerText != null) headerText.fontSize = Mathf.RoundToInt(32 * uiScaleFactor);

        // Update date time
        if (dateTimeText != null) dateTimeText.fontSize = Mathf.RoundToInt(22 * uiScaleFactor);

        // Update position texts
        if (sunPositionText != null) sunPositionText.fontSize = Mathf.RoundToInt(18 * uiScaleFactor);
        if (moonPositionText != null) moonPositionText.fontSize = Mathf.RoundToInt(18 * uiScaleFactor);
        if (milkyWayPositionText != null) milkyWayPositionText.fontSize = Mathf.RoundToInt(18 * uiScaleFactor);
        if (deviceOrientationText != null) deviceOrientationText.fontSize = Mathf.RoundToInt(16 * uiScaleFactor);

        // Update button text
        TextMeshProUGUI buttonText = updateButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) buttonText.fontSize = Mathf.RoundToInt(24 * uiScaleFactor);

        // Update toggle button text
        TextMeshProUGUI toggleText = toggleDashboardButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (toggleText != null) toggleText.fontSize = Mathf.RoundToInt(20 * uiScaleFactor);
    }

    private void CreateHeaderSection()
    {
        GameObject headerObj = CreateUIObject("Header", dashboardPanel.transform);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(50 * uiScaleFactor));

        TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
        headerText.text = "CELESTIAL SPHERE";
        headerText.fontSize = Mathf.RoundToInt(32 * uiScaleFactor);
        headerText.color = Color.white;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.fontStyle = FontStyles.Bold;
    }

    private void CreateDateTimeSection()
    {
        GameObject datetimeObj = CreateUIObject("DateTimeDisplay", dashboardPanel.transform);
        RectTransform datetimeRect = datetimeObj.AddComponent<RectTransform>();
        datetimeRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(35 * uiScaleFactor));

        dateTimeText = datetimeObj.AddComponent<TextMeshProUGUI>();
        dateTimeText.text = "Date/Time: Loading...";
        dateTimeText.fontSize = Mathf.RoundToInt(22 * uiScaleFactor);
        dateTimeText.color = Color.yellow;
        dateTimeText.alignment = TextAlignmentOptions.Center;
        dateTimeText.fontStyle = FontStyles.Bold;
    }

    private void AdjustForMobile()
    {
        // Additional mobile-specific adjustments
#if UNITY_ANDROID || UNITY_IOS
        // Increase touch areas and font sizes for mobile
        if (updateButton != null)
        {
            RectTransform buttonRect = updateButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, Mathf.RoundToInt(100 * uiScaleFactor));
        }

        if (toggleDashboardButton != null)
        {
            RectTransform toggleRect = toggleDashboardButton.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(toggleRect.sizeDelta.x, Mathf.RoundToInt(80 * uiScaleFactor));
        }
#endif
    }

    private void CreateLocationSection()
    {
        GameObject locationSection = CreateUIObject("LocationSection", dashboardPanel.transform);
        RectTransform locationRect = locationSection.AddComponent<RectTransform>();
        locationRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(120 * uiScaleFactor));

        // Horizontal layout for location inputs
        HorizontalLayoutGroup horizontalLayout = locationSection.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.padding = new RectOffset(
            Mathf.RoundToInt(15 * uiScaleFactor),
            Mathf.RoundToInt(15 * uiScaleFactor), 0, 0
        );
        horizontalLayout.spacing = 20f * uiScaleFactor;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandHeight = true;
        horizontalLayout.childForceExpandWidth = true;

        // Latitude input
        latInput = CreateResponsiveInputField("Latitude", "Latitude:", latitude.ToString("F4"), locationSection.transform);

        // Longitude input
        lonInput = CreateResponsiveInputField("Longitude", "Longitude:", longitude.ToString("F4"), locationSection.transform);
    }

    private void CreateDateTimeInputSection()
    {
        GameObject datetimeSection = CreateUIObject("DateTimeInputSection", dashboardPanel.transform);
        RectTransform datetimeRect = datetimeSection.AddComponent<RectTransform>();
        datetimeRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(120 * uiScaleFactor));

        // Horizontal layout for date/time inputs
        HorizontalLayoutGroup horizontalLayout = datetimeSection.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.padding = new RectOffset(
            Mathf.RoundToInt(15 * uiScaleFactor),
            Mathf.RoundToInt(15 * uiScaleFactor), 0, 0
        );
        horizontalLayout.spacing = 20f * uiScaleFactor;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandHeight = true;
        horizontalLayout.childForceExpandWidth = true;

        // Date input (use system now so UI shows current date even if currentDateTime set later)
        dateInput = CreateResponsiveInputField("Date", "Date:", DateTime.Now.ToString("yyyy-MM-dd"), datetimeSection.transform);

        // Time input (use system now so UI shows current time even if currentDateTime set later)
        timeInput = CreateResponsiveInputField("Time", "Time:", DateTime.Now.ToString("HH:mm"), datetimeSection.transform);
    }

    private void CreatePositionDisplaySection()
    {
        GameObject positionSection = CreateUIObject("PositionSection", dashboardPanel.transform);
        RectTransform positionRect = positionSection.AddComponent<RectTransform>();
        positionRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(120 * uiScaleFactor));

        // Vertical layout for position displays
        VerticalLayoutGroup verticalLayout = positionSection.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(0, 0, 0, 0);
        verticalLayout.spacing = 8f * uiScaleFactor;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        // Sun position
        sunPositionText = CreateResponsiveTextElement("SunPosition", "Sun: Calculating...",
            Mathf.RoundToInt(18 * uiScaleFactor), FontStyles.Normal, positionSection.transform);

        // Moon position
        moonPositionText = CreateResponsiveTextElement("MoonPosition", "Moon: Calculating...",
            Mathf.RoundToInt(18 * uiScaleFactor), FontStyles.Normal, positionSection.transform);

        // Milky Way position
        milkyWayPositionText = CreateResponsiveTextElement("MilkyWayPosition", "Milky Way: Calculating...",
            Mathf.RoundToInt(18 * uiScaleFactor), FontStyles.Normal, positionSection.transform);

        // Device orientation info
        deviceOrientationText = CreateResponsiveTextElement("DeviceOrientation", "Orientation: Initializing...",
            Mathf.RoundToInt(16 * uiScaleFactor), FontStyles.Normal, positionSection.transform);
        deviceOrientationText.color = Color.cyan;
    }

    private void CreateUpdateButtonSection()
    {
        GameObject buttonSection = CreateUIObject("UpdateButtonSection", dashboardPanel.transform);
        RectTransform buttonRect = buttonSection.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(90 * uiScaleFactor));

        // Create update button with proper button setup
        updateButton = CreateResponsiveButton("Update Celestial Positions",
            new Color(0.2f, 0.6f, 1f, 1f),
            Mathf.RoundToInt(24 * uiScaleFactor),
            FontStyles.Bold, buttonSection.transform);

        //SetupButtonInteractivity(updateButton);
        updateButton.onClick.AddListener(OnUpdateButtonClicked);
    }

    private TMP_InputField CreateResponsiveInputField(string name, string label, string defaultValue, Transform parent)
    {
        GameObject container = CreateUIObject(name + "Container", parent);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(
            Mathf.RoundToInt(400 * uiScaleFactor),
            Mathf.RoundToInt(100 * uiScaleFactor)
        );

        // Container layout
        VerticalLayoutGroup verticalLayout = container.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(
            Mathf.RoundToInt(8 * uiScaleFactor),
            Mathf.RoundToInt(8 * uiScaleFactor), 0, 0
        );
        verticalLayout.spacing = 8f * uiScaleFactor;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        // Label
        GameObject labelObj = CreateUIObject("Label", container.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(28 * uiScaleFactor));

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = Mathf.RoundToInt(18 * uiScaleFactor);
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.fontStyle = FontStyles.Bold;

        // Input field container
        GameObject inputContainer = CreateUIObject("InputContainer", container.transform);
        RectTransform inputContainerRect = inputContainer.AddComponent<RectTransform>();
        inputContainerRect.sizeDelta = new Vector2(0, Mathf.RoundToInt(50 * uiScaleFactor));

        // Input field background
        GameObject inputBg = CreateUIObject("InputBackground", inputContainer.transform);
        RectTransform inputBgRect = inputBg.AddComponent<RectTransform>();
        inputBgRect.anchorMin = Vector2.zero;
        inputBgRect.anchorMax = Vector2.one;
        inputBgRect.sizeDelta = Vector2.zero;

        Image bgImage = inputBg.AddComponent<Image>();
        bgImage.color = Color.white;
        bgImage.raycastTarget = true;

        // Input field
        GameObject inputObj = CreateUIObject("InputField", inputContainer.transform);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(1, 1);
        inputRect.sizeDelta = new Vector2(
            -Mathf.RoundToInt(15 * uiScaleFactor),
            -Mathf.RoundToInt(12 * uiScaleFactor)
        );
        inputRect.anchoredPosition = Vector2.zero;

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.targetGraphic = bgImage;

        // Text component
        GameObject textObj = CreateUIObject("Text", inputObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = defaultValue;
        textComponent.fontSize = Mathf.RoundToInt(20 * uiScaleFactor);
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Left;
        textComponent.raycastTarget = true;
        textComponent.overflowMode = TextOverflowModes.Overflow;

        inputField.textComponent = textComponent;
        inputField.text = defaultValue;

        return inputField;
    }

    private TextMeshProUGUI CreateResponsiveTextElement(string name, string text, int fontSize, FontStyles style, Transform parent)
    {
        GameObject textObj = CreateUIObject(name, parent);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, Mathf.RoundToInt(25 * uiScaleFactor));

        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.fontStyle = style;
        textComp.overflowMode = TextOverflowModes.Ellipsis;

        return textComp;
    }

    private Button CreateResponsiveButton(string buttonText, Color color, int fontSize, FontStyles style, Transform parent)
    {
        GameObject buttonObj = CreateUIObject("Button", parent);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(
            Mathf.RoundToInt(500 * uiScaleFactor),
            Mathf.RoundToInt(80 * uiScaleFactor)
        );

        Button button = buttonObj.AddComponent<Button>();

        // Add image component for button appearance
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        buttonImage.raycastTarget = true;

        // Button text with proper anchoring
        GameObject textObj = CreateUIObject("ButtonText", buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = buttonText;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = style;
        textComponent.raycastTarget = false;

        // Add a simple scale animation for button press
        AddButtonAnimation(buttonObj);

        return button;
    }

    private void CreateToggleButton()
    {
        GameObject toggleObj = CreateUIObject("ToggleButton", canvas.transform);
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();

        // Position in bottom center with responsive margins
        float widthPercentage = 0.3f * uiScaleFactor; // Adjust width based on scale
        float heightPercentage = 0.08f * uiScaleFactor; // Adjust height based on scale

        toggleRect.anchorMin = new Vector2(0.5f - widthPercentage / 2, 0.02f);
        toggleRect.anchorMax = new Vector2(0.5f + widthPercentage / 2, 0.02f + heightPercentage);
        toggleRect.offsetMin = Vector2.zero;
        toggleRect.offsetMax = Vector2.zero;

        toggleDashboardButton = toggleObj.AddComponent<Button>();
        Image toggleImage = toggleObj.AddComponent<Image>();
        toggleImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        toggleImage.raycastTarget = true;

        // Toggle text
        GameObject textObj = CreateUIObject("ToggleText", toggleObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI toggleText = textObj.AddComponent<TextMeshProUGUI>();
        toggleText.text = "Hide UI";
        toggleText.fontSize = Mathf.RoundToInt(20 * uiScaleFactor);
        toggleText.color = Color.white;
        toggleText.alignment = TextAlignmentOptions.Center;
        toggleText.fontStyle = FontStyles.Bold;

        toggleDashboardButton.onClick.AddListener(ToggleDashboard);
    }
    private void FindStarDatabase()
    {
        if (starDatabase == null)
        {
            starDatabase = FindObjectOfType<StarDatabase>();
            if (starDatabase != null)
            {
                Debug.Log("StarDatabase found automatically");
            }
            else
            {
                Debug.LogWarning("No StarDatabase found in scene");
            }
        }
    }

    /// <summary>
    /// Gets the current latitude for external scripts
    /// </summary>
    public double GetLatitude()
    {
        return latitude;
    }

    /// <summary>
    /// Gets the current longitude for external scripts
    /// </summary>
    public double GetLongitude()
    {
        return longitude;
    }

    /// <summary>
    /// Gets the current date time
    /// </summary>
    public DateTime GetCurrentDateTime()
    {
        return currentDateTime;
    }

    /// <summary>
    /// Gets the celestial sphere transform
    /// </summary>
    public Transform GetCelestialSphere()
    {
        return celestialSphere;
    }

    /// <summary>
    /// Gets the sphere radius
    /// </summary>
    public float GetSphereRadius()
    {
        return sphereRadius;
    }
    private void AddButtonAnimation(GameObject buttonObj)
    {
        // Simple scale animation on press
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => {
            StartCoroutine(ButtonPressAnimation(buttonObj.transform));
        });
    }

    private IEnumerator ButtonPressAnimation(Transform buttonTransform)
    {
        Vector3 originalScale = buttonTransform.localScale;
        buttonTransform.localScale = originalScale * 0.95f;
        yield return new WaitForSeconds(0.1f);
        buttonTransform.localScale = originalScale;
    }

   
    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    private void ToggleDashboard()
    {
        bool isVisible = !dashboardPanel.activeSelf;
        dashboardPanel.SetActive(isVisible);

        TextMeshProUGUI toggleText = toggleDashboardButton.GetComponentInChildren<TextMeshProUGUI>();
        toggleText.text = isVisible ? "Hide UI" : "Show UI";
    }

    private IEnumerator ShowUpdateSuccess()
    {
        if (updateButton != null)
        {
            Image buttonImage = updateButton.GetComponent<Image>();
            Color originalColor = buttonImage.color;
            buttonImage.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            buttonImage.color = originalColor;
        }
    }

    private IEnumerator ShowUpdateError(string message)
    {
        if (updateButton != null)
        {
            Image buttonImage = updateButton.GetComponent<Image>();
            Color originalColor = buttonImage.color;
            buttonImage.color = Color.red;

            // Temporarily change button text to show error
            TextMeshProUGUI buttonText = updateButton.GetComponentInChildren<TextMeshProUGUI>();
            string originalText = buttonText.text;
            buttonText.text = message;

            yield return new WaitForSeconds(1.5f);

            buttonImage.color = originalColor;
            buttonText.text = originalText;
        }
    }

    private void UpdateDateTimeDisplay()
    {
        if (dateTimeText != null)
        {
            dateTimeText.text = $"Date/Time: {currentDateTime:yyyy-MM-dd HH:mm:ss}";
        }
    }

    private void UpdatePositionDisplays()
    {
        if (sunPositionText != null)
        {
            float sunAlt = GetAltitude(sun);
            float sunAz = GetAzimuth(sun);
            sunPositionText.text = $"Sun: Alt {sunAlt:F1}�, Az {sunAz:F1}�";
        }

        if (moonPositionText != null)
        {
            float moonAlt = GetAltitude(moon);
            float moonAz = GetAzimuth(moon);
            moonPositionText.text = $"Moon: Alt {moonAlt:F1}�, Az {moonAz:F1}�";
        }
    }

    #endregion

    #region Public Methods - Enhanced

    public void SetUseARDirection(bool useAR)
    {
        useARDirection = useAR;
        if (deviceOrientationText != null)
        {
            deviceOrientationText.color = useAR ? Color.cyan : Color.yellow;
        }

        if (useAR)
        {
            // Reset alignment when switching to AR mode
            isOrientationInitialized = false;
            initialAlignmentDone = false;
            Debug.Log("AR mode activated - realigning with real world...");
        }
        else
        {
            // Switch back to traditional sidereal tracking
            double lst = CalculateLocalSiderealTime();
            celestialSphere.localRotation = Quaternion.Euler(0, (float)lst, 0);
            Debug.Log("AR mode deactivated - using traditional tracking");
        }
    }

    public void SetSmoothFactor(float factor)
    {
        smoothFactor = Mathf.Clamp01(factor);
        Debug.Log($"Smooth factor set to: {smoothFactor}");
    }

    public void ResetOrientation()
    {
        isOrientationInitialized = false;
        initialAlignmentDone = false;
        lastCompassHeading = 0f;
        Debug.Log("Orientation reset - recalibrating...");
    }

    public void RecalibrateSensors()
    {
        // Reinitialize sensors
        gyroEnabled = InitializeGyroscope();
        compassEnabled = InitializeCompass();

        // Reset orientation state
        isOrientationInitialized = false;
        initialAlignmentDone = false;

        Debug.Log("Sensors recalibrated - wave device in figure-8 pattern for better accuracy");
    }

    /// <summary>
    /// Quick realignment - useful when the alignment seems off
    /// </summary>
    public void QuickRealign()
    {
        if (compassEnabled)
        {
            // Force fresh compass reading
            float newHeading = GetCompassHeading();
            lastCompassHeading = newHeading;

            Debug.Log($"Quick realignment - Current heading: {newHeading:F1}�");

            // Force immediate update
            UpdateCelestialSphereOrientation();
        }
    }

    /// <summary>
    /// Manual realignment - point device North and call this method for precise alignment
    /// </summary>
    public void RealignWithNorth()
    {
        if (compassEnabled)
        {
            // Get multiple samples for better accuracy
            float heading1 = GetCompassHeading();
            System.Threading.Thread.Sleep(100);
            float heading2 = GetCompassHeading();
            System.Threading.Thread.Sleep(100);
            float heading3 = GetCompassHeading();

            float averageHeading = (heading1 + heading2 + heading3) / 3f;
            lastCompassHeading = averageHeading;
            initialAlignmentDone = true;

            Debug.Log($"Precise realignment - Average heading: {averageHeading:F1}� " +
                     $"(Readings: {heading1:F1}, {heading2:F1}, {heading3:F1})");

            // Force immediate update
            UpdateCelestialSphereOrientation();
        }
    }

    #endregion
}