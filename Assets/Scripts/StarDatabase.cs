using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StarData
{
    // Basic Identity
    public string name;                    // Common name (e.g., "Sirius", "Polaris")
    public string constellation;           // Constellation name
    public string spectralType;            // Spectral classification (e.g., "A1V", "K2III")
    public string bayerDesignation;        // Bayer letter (e.g., "α", "β", "γ")
    
    // Positional Data (Celestial Coordinates)
    public float rightAscension;           // hours (0-24)
    public float declination;              // degrees (-90 to +90)
    public float hourAngle;                // calculated at runtime
    public float altitude;                 // degrees above horizon (calculated)
    public float azimuth;                  // compass bearing (calculated)
    
    // Physical Properties
    public float magnitude;                // apparent magnitude (brightness as seen from Earth)
    public float absoluteMagnitude;        // intrinsic brightness (at standard 10 pc distance)
    public float temperature;              // surface temperature in Kelvin
    public float radius;                   // radius relative to Sun (1.0 = Sun's radius)
    public float mass;                     // mass relative to Sun (1.0 = Sun's mass)
    public float luminosity;               // luminosity relative to Sun (1.0 = Sun's luminosity)
    public float density;                  // density relative to Sun
    
    // Distance Data
    public float distanceLightYears;       // distance in light-years
    public float distanceParsecs;          // distance in parsecs
    public float parallax;                 // parallax in arcseconds
    public float properMotionRA;           // proper motion in RA (arcsec/year)
    public float properMotionDec;          // proper motion in Dec (arcsec/year)
    
    // Rotational & Velocity Data
    public float rotationPeriodDays;       // rotation period in days
    public float rotationalVelocity;       // rotational velocity (km/s)
    public float radialVelocity;           // radial velocity (km/s) - positive = receding
    
    // Classification
    public float effectiveTemperature;     // effective surface temperature
    public float surfaceGravity;           // surface gravity (log value)
    public StarClass starClass;            // enumerated star class (O, B, A, F, G, K, M)
    public StarType starType;              // Main Sequence, Giant, Supergiant, etc.
    public bool isVariable;                // if star varies in brightness
    public bool isBinary;                  // if part of binary star system
    public float binaryPeriodDays;         // orbital period if binary
    
    // Visual Properties
    public Color color;                    // star color (calculated from temperature)
    public float colorIndex;               // B-V color index
    public float bolometricCorrection;     // correction for non-visible wavelengths
    
    // Notable Features
    public bool isHypergiant;              // extremely large star
    public bool isNeutronStar;             // compact stellar remnant
    public bool isWhiteDwarf;              // cooling stellar remnant
    public bool isPulsar;                  // rotating neutron star
    public string commonName;              // traditional name (e.g., "Alpha Centauri")
    public string notableFeatures;         // description of notable characteristics
    
    // Age & Evolution
    public float ageMillionYears;          // estimated age in millions of years
    public string evolutionaryStage;       // where in lifecycle (e.g., "Main Sequence", "Red Giant")
}

/// <summary>
/// Spectral classification for stellar types
/// </summary>
public enum StarClass
{
    O = 0,  // Blue - hottest
    B = 1,  // Blue-white
    A = 2,  // White
    F = 3,  // Yellow-white
    G = 4,  // Yellow (Sun is G2)
    K = 5,  // Orange
    M = 6   // Red - coolest
}

/// <summary>
/// Stellar type classification
/// </summary>
public enum StarType
{
    MainSequence,    // Most common, hydrogen burning
    Giant,           // Evolved, much larger
    Supergiant,      // Very large and luminous
    Hypergiant,      // Extremely large
    Dwarf,           // Small, compact
    SubDwarf,        // Below main sequence
    WhiteDwarf,      // Stellar remnant
    NeutronStar,     // Ultra-dense remnant
    BlackHole,       // Theoretical only
    Unknown          // Unclassified
}

[System.Serializable]
public class ConstellationData
{
    public string name;
    public string abbreviation;
    public List<Vector2> boundaryPoints; // RA/Dec coordinates
    public Vector2 center; // Center point for label placement
}

/// <summary>
/// Helper class for astrographic calculations and conversions
/// </summary>
public static class AstrographicCalculations
{
    /// <summary>
    /// Convert spectral type string (e.g., "K2V") to StarClass enum
    /// </summary>
    public static StarClass GetStarClassFromSpectralType(string spectralType)
    {
        if (string.IsNullOrEmpty(spectralType)) return StarClass.G;
        char classChar = char.ToUpper(spectralType[0]);
        return classChar switch
        {
            'O' => StarClass.O,
            'B' => StarClass.B,
            'A' => StarClass.A,
            'F' => StarClass.F,
            'G' => StarClass.G,
            'K' => StarClass.K,
            'M' => StarClass.M,
            _ => StarClass.G
        };
    }

    /// <summary>
    /// Calculate effective temperature from spectral class
    /// Uses standard stellar classification temperature ranges
    /// </summary>
    public static float GetTemperatureFromSpectralClass(StarClass starClass)
    {
        return starClass switch
        {
            StarClass.O => 40000f,  // Blue
            StarClass.B => 20000f,  // Blue-white
            StarClass.A => 10000f,  // White
            StarClass.F => 7500f,   // Yellow-white
            StarClass.G => 5800f,   // Yellow (Sun standard)
            StarClass.K => 4200f,   // Orange
            StarClass.M => 3500f,   // Red
            _ => 5800f
        };
    }

    /// <summary>
    /// Calculate apparent magnitude from luminosity and distance
    /// m = M + 5 * log10(d) - 5, where M is absolute magnitude
    /// </summary>
    public static float CalculateApparentMagnitude(float absoluteMagnitude, float distanceParsecs)
    {
        if (distanceParsecs <= 0) return absoluteMagnitude;
        return absoluteMagnitude + 5f * Mathf.Log10(distanceParsecs) - 5f;
    }

    /// <summary>
    /// Calculate luminosity from absolute magnitude
    /// L/L_sun = 10^((M_sun - M) / 2.5)
    /// </summary>
    public static float CalculateLuminosityFromMagnitude(float absoluteMagnitude)
    {
        const float sunAbsoluteMagnitude = 4.83f;
        float magnitudeDifference = sunAbsoluteMagnitude - absoluteMagnitude;
        return Mathf.Pow(10f, magnitudeDifference / 2.5f);
    }

    /// <summary>
    /// Calculate radius from luminosity and temperature
    /// R/R_sun = sqrt(L/L_sun) * (T_sun / T)^2
    /// </summary>
    public static float CalculateRadiusFromProperties(float luminosity, float temperatureK)
    {
        const float sunTemperature = 5778f;
        if (temperatureK <= 0) return 1f;
        float tempRatio = sunTemperature / temperatureK;
        return Mathf.Sqrt(luminosity) * tempRatio * tempRatio;
    }

    /// <summary>
    /// Calculate star color based on temperature (Kelvin)
    /// Uses standard black-body radiation approximation
    /// </summary>
    public static Color CalculateStarColorFromTemperature(float temperatureK)
    {
        if (temperatureK <= 1000) return new Color(1.0f, 0.0f, 0.0f);  // Very red
        if (temperatureK < 3500) return new Color(1.0f, 0.5f, 0.0f);   // Red
        if (temperatureK < 5000) return new Color(1.0f, 0.7f, 0.0f);   // Orange
        if (temperatureK < 6000) return new Color(1.0f, 0.9f, 0.6f);   // Yellow-white
        if (temperatureK < 7500) return new Color(1.0f, 1.0f, 0.8f);   // Yellow-white
        if (temperatureK < 10000) return new Color(0.9f, 0.95f, 1.0f); // White
        if (temperatureK < 28000) return new Color(0.7f, 0.8f, 1.0f);  // Blue-white
        return new Color(0.5f, 0.6f, 1.0f);                            // Blue
    }

    /// <summary>
    /// Convert parallax (arcseconds) to distance (parsecs)
    /// distance = 1 / parallax (in arcseconds)
    /// </summary>
    public static float ParallaxToParsecs(float parallaxArcsec)
    {
        if (parallaxArcsec <= 0) return float.MaxValue;
        return 1f / parallaxArcsec;
    }

    /// <summary>
    /// Convert distance (parsecs) to light-years
    /// 1 parsec = 3.26156 light-years
    /// </summary>
    public static float ParsecsToLightYears(float parsecs)
    {
        return parsecs * 3.26156f;
    }

    /// <summary>
    /// Get stellar type description from star class and luminosity
    /// </summary>
    public static StarType GetStarTypeFromProperties(StarClass starClass, float luminosity, float radius)
    {
        // Hypergiants: extremely high luminosity and radius
        if (luminosity > 100000f) return StarType.Hypergiant;
        
        // Supergiants: high luminosity and large radius
        if (luminosity > 1000f && radius > 30f) return StarType.Supergiant;
        
        // Giants: moderate-high luminosity, large radius
        if (luminosity > 100f && radius > 10f) return StarType.Giant;
        
        // White Dwarfs: low luminosity, very small radius
        if (luminosity < 0.01f && radius < 0.01f) return StarType.WhiteDwarf;
        
        // Main Sequence: typical stars
        if (luminosity > 0.01f && radius > 0.01f && radius < 10f)
            return StarType.MainSequence;
        
        return StarType.Unknown;
    }

    /// <summary>
    /// Get descriptive text for star class
    /// </summary>
    public static string GetStarClassDescription(StarClass starClass)
    {
        return starClass switch
        {
            StarClass.O => "O-type: Blue, extremely hot, massive, luminous",
            StarClass.B => "B-type: Blue-white, hot, massive",
            StarClass.A => "A-type: White, hot",
            StarClass.F => "F-type: Yellow-white",
            StarClass.G => "G-type: Yellow, medium (Sun is G2V)",
            StarClass.K => "K-type: Orange, cooler",
            StarClass.M => "M-type: Red, cool, common",
            _ => "Unknown spectral type"
        };
    }
}

public class StarDatabase : MonoBehaviour
{
    // Solar system planet data
    [System.Serializable]
    public class PlanetData
    {
        public string name;
        public float diameterKm;
        public float distanceFromSunAU;
        public Material material;
        public GameObject planetObject;
    }

    [Header("Solar System Planets")]
    public Material planetMaterialMercury;
    public Material planetMaterialVenus;
    public Material planetMaterialMars;
    public Material planetMaterialJupiter;
    public Material planetMaterialSaturn;
    [Header("Saturn Ring")]
    public Material saturnRingMaterial; // Inspector-assigned material for Saturn's ring
    public Material planetMaterialUranus;
    public Material planetMaterialNeptune;
    public Material planetMaterialPluto;

    [Header("Galactic Center")]
    public Material galacticCenterMaterial;

    private List<PlanetData> planets = new List<PlanetData>();
    public static StarDatabase Instance;

    [Header("Settings")]
    public float starSizeMultiplier = 2.0f;
    public float minStarSize = 0.1f;
    public float maxStarSize = 4.0f;
    public bool showStarNames = true;
    public bool showConstellationNames = true;
    public float labelOffset = 1.0f;
    public bool useCelestialSphereLocation = true;
    public float latitude = 18.6056704f;
    public float longitude = 73.7804288f;
    public bool useSystemTime = true;
    public DateTime customDateTime;
    public bool showGUI = true;

    // Sun and Moon Scale Reference
    [Header("Sun & Moon Scale Reference")]
    public float sunAngularDiameter = 0.533f; // degrees (as seen from Earth)
    public float moonAngularDiameter = 0.518f; // degrees (as seen from Earth)
    public float sunRelativeSize = 1.0f; // reference size (1x)
    public float moonRelativeSize = 0.97f; // approximately same as sun
    public bool autoAdjustStarSizeFromRefence = true; // auto-scale stars relative to sun/moon

    // Star Visibility & Label Settings
    [Header("Star Visibility")]
    public float minVisibleMagnitude = 6.5f; // faintest visible stars (naked eye limit)
    public float maxContrastMagnitude = 2.0f; // brightest stars for contrast
    public bool adjustStarSizeBasedOnMagnitude = true;
    public float magnitudeExponent = 1.2f; // exponent for magnitude-to-size conversion

    // Star Color Settings
    [Header("Star Color Settings")]
    public bool useRealisticStarColors = true;
    public float colorIntensityMultiplier = 2f;
    public AnimationCurve temperatureToColorCurve = new AnimationCurve(
        new Keyframe(1000, 0f),
        new Keyframe(3500, 0.2f),
        new Keyframe(5000, 0.5f),
        new Keyframe(6000, 0.7f),
        new Keyframe(10000, 0.9f),
        new Keyframe(40000, 1f)
    );

    // Label Settings
    [Header("Label Settings")]
    public float minLabelFontSize = 12f;
    public float maxLabelFontSize = 48f; // increased for better readability with 2000 stars
    public float labelDistanceScaleFactor = 2.5f;
    public bool autoScaleLabels = true;
    public Color labelColor = new Color(1f, 1f, 1f, 1f);
    public Color constellationLabelColor = new Color(0.8f, 0.9f, 1f, 0.85f);

    // Label Visibility Control
    [Header("Label Visibility")]
    public float labelShowThreshold = 4.5f; // magnitude below which labels show (brighter stars)
    public float labelCameraDistanceFactor = 4.0f; // scales label distance from stars
    public float minLabelCanvasScale = 0.8f; // minimum readable label scale
    public float maxLabelCanvasScale = 8.0f; // maximum label scale for proper readability
    public bool keepLabelsReadableFromCenter = true; // labels always readable from celestial center

    [Header("Label Limits")]
    [Tooltip("Maximum number of star name labels to create at once (helps performance with large catalogs)")]
    public int maxStarLabels = 800;

    // Planet Label Settings
    [Header("Planet Labels")]
    public bool showPlanetLabels = true;
    public float planetLabelFontSize = 40f;
    public Color planetLabelColor = new Color(0.8f, 1.0f, 0.9f, 0.95f); // Cyan-ish for planets
    public float planetLabelCanvasScale = 1.0f;

    // Auto-created references
    public GameObject starPrefab;
    private GameObject starLabelPrefab;
    private GameObject constellationLabelPrefab;
    
    // Star Materials - Assignable from Inspector
    [Header("Star Materials")]
    [SerializeField] private Material starMaterial;  // Main material for all stars
    [SerializeField] public Material starMaterialCustom;  // Optional custom material override
    public bool useCustomStarMaterial = false;  // Toggle between auto/custom material
    
    [SerializeField] private Material starMaterialBright;  // Optional: different material for bright stars (mag < 1)
    public bool useDifferentBrightStarMaterial = false;  // Toggle different material for bright stars

    private CelestialSphereController celestialSphereController;
    private readonly List<StarData> brightStars = new();
    private readonly List<ConstellationData> constellations = new();
    private readonly List<GameObject> starObjects = new();
    private readonly List<GameObject> starLabels = new();
    private readonly List<GameObject> constellationLabels = new();
    private readonly List<GameObject> planetLabels = new(); // Labels for planets

    private bool showStarControls = true;
    private bool showLocationControls = false;
    private Vector2 scrollPosition;

    // Orbital elements for planets (simplified, J2000 epoch)
    private class PlanetOrbit
    {
        public double semiMajorAxisAU; // a
        public double eccentricity;     // e
        public double inclination;      // i (deg)
        public double longitudeAscendingNode; // Ω (deg)
        public double argumentPerihelion;    // ω (deg)
        public double meanLongitude;         // L (deg)
        public double periodDays;            // orbital period
    }
    
    private readonly Dictionary<string, PlanetOrbit> planetOrbits = new Dictionary<string, PlanetOrbit>
    {
        { "Mercury", new PlanetOrbit { semiMajorAxisAU = 0.387, eccentricity = 0.2056, inclination = 7.005, longitudeAscendingNode = 48.331, argumentPerihelion = 29.124, meanLongitude = 252.251, periodDays = 87.97 } },
        { "Venus",   new PlanetOrbit { semiMajorAxisAU = 0.723, eccentricity = 0.0068, inclination = 3.394, longitudeAscendingNode = 76.680, argumentPerihelion = 54.852, meanLongitude = 181.979, periodDays = 224.70 } },
        { "Mars",    new PlanetOrbit { semiMajorAxisAU = 1.524, eccentricity = 0.0934, inclination = 1.850, longitudeAscendingNode = 49.558, argumentPerihelion = 286.502, meanLongitude = 355.453, periodDays = 686.98 } },
        { "Jupiter", new PlanetOrbit { semiMajorAxisAU = 5.203, eccentricity = 0.0484, inclination = 1.303, longitudeAscendingNode = 100.464, argumentPerihelion = 273.867, meanLongitude = 34.396, periodDays = 4332.59 } },
        { "Saturn",  new PlanetOrbit { semiMajorAxisAU = 9.537, eccentricity = 0.0542, inclination = 2.485, longitudeAscendingNode = 113.665, argumentPerihelion = 339.392, meanLongitude = 49.954, periodDays = 10759.22 } },
        { "Uranus",  new PlanetOrbit { semiMajorAxisAU = 19.191, eccentricity = 0.0472, inclination = 0.773, longitudeAscendingNode = 74.006, argumentPerihelion = 96.998, meanLongitude = 313.238, periodDays = 30685.4 } },
        { "Neptune", new PlanetOrbit { semiMajorAxisAU = 30.068, eccentricity = 0.0086, inclination = 1.770, longitudeAscendingNode = 131.784, argumentPerihelion = 273.187, meanLongitude = -55.120, periodDays = 60190.0 } },
        { "Pluto",   new PlanetOrbit { semiMajorAxisAU = 39.482, eccentricity = 0.2488, inclination = 17.140, longitudeAscendingNode = 110.299, argumentPerihelion = 113.834, meanLongitude = 238.929, periodDays = 90560.0 } },
    };

    // View Mode Settings
    [Header("View Mode")]
    public bool viewFromCelestialCenter = true; // View from center (all stars visible) vs Earth position (hemisphere view)
    public float celestialSphereRadius = 40f; // Radius of the celestial sphere

    private void Awake()
    {
        Instance = this;
        FixIncompatibleMaterials();
        ValidateOrCreateMaterials();
        DisableProblematicXRSampleObjects();
        CreatePrefabsAndMaterials();
        FindCelestialSphereController();
        InitializeStarDatabase();
        InitializeConstellations();
        InitializePlanets();
        // Set defaults for star size and intensity
        starSizeMultiplier = 2f;
        colorIntensityMultiplier = 2f;
        showStarNames = true;
        showConstellationNames = false;
        autoScaleLabels = true;
        keepLabelsReadableFromCenter = true;
    }

    private void Start()
    {
        UpdateLocationFromCelestialSphere();
        Debug.Log("[StarDatabase] Start() - Initializing star and planet rendering...");
        
        // Create stars and labels in the celestial sphere
        if (celestialSphereController != null)
        {
            Transform celestialSphere = celestialSphereController.transform;
            Debug.Log($"[StarDatabase] Creating {brightStars.Count} stars on celestial sphere (radius: {celestialSphereRadius})");
            CreateStarObjects(celestialSphere, celestialSphereRadius);
            Debug.Log($"[StarDatabase] Star rendering complete: {starObjects.Count} stars, {starLabels.Count} labels, {planets.Count} planets, {planetLabels.Count} planet labels");
        }
        else
        {
            Debug.LogWarning("[StarDatabase] CelestialSphereController not found. Rendering will not start.");
        }
    }

    private void Update()
    {
        if (useCelestialSphereLocation && celestialSphereController != null)
        {
            if (Time.frameCount % 60 == 0)
            {
                UpdateLocationFromCelestialSphere();
            }
        }

        // Update label scales dynamically
        if (autoScaleLabels && starLabels.Count > 0)
        {
            UpdateLabelScales();
        }
        
        // Update star positions every frame
        if (starObjects.Count > 0)
        {
            UpdateStarPositions(celestialSphereRadius);
        }
        
        UpdatePlanetPositions();
    }

    private void UpdatePlanetPositions()
    {
        double daysSinceJ2000 = (DateTime.UtcNow - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays;
        float planetDistanceMultiplier = 4.5f; // Planets will be placed 20% beyond the sphere
        float planetShellRadius = celestialSphereRadius * planetDistanceMultiplier;
        
        foreach (var planet in planets)
        {
            if (!planetOrbits.TryGetValue(planet.name, out var orbit)) continue;
            // Mean anomaly
            double M = (orbit.meanLongitude - orbit.argumentPerihelion + 360.0 * daysSinceJ2000 / orbit.periodDays) % 360.0;
            M = Mathf.Deg2Rad * (float)M;
            // Solve Kepler's equation for E (eccentric anomaly)
            double E = M;
            for (int i = 0; i < 5; i++) E = M + orbit.eccentricity * Math.Sin(E);
            // Heliocentric distance and true anomaly
            double xv = orbit.semiMajorAxisAU * (Math.Cos(E) - orbit.eccentricity);
            double yv = orbit.semiMajorAxisAU * Math.Sqrt(1.0 - orbit.eccentricity * orbit.eccentricity) * Math.Sin(E);
            double v = Math.Atan2(yv, xv);
            double r = Math.Sqrt(xv * xv + yv * yv);
            // Heliocentric ecliptic coordinates
            double xh = r * (Math.Cos(Mathf.Deg2Rad * (float)(orbit.longitudeAscendingNode)) * Math.Cos(v + Mathf.Deg2Rad * (float)(orbit.argumentPerihelion - orbit.longitudeAscendingNode)) - Math.Sin(Mathf.Deg2Rad * (float)(orbit.longitudeAscendingNode)) * Math.Sin(v + Mathf.Deg2Rad * (float)(orbit.argumentPerihelion - orbit.longitudeAscendingNode)) * Math.Cos(Mathf.Deg2Rad * (float)(orbit.inclination)));
            double yh = r * (Math.Sin(Mathf.Deg2Rad * (float)(orbit.longitudeAscendingNode)) * Math.Cos(v + Mathf.Deg2Rad * (float)(orbit.argumentPerihelion - orbit.longitudeAscendingNode)) + Math.Cos(Mathf.Deg2Rad * (float)(orbit.longitudeAscendingNode)) * Math.Sin(v + Mathf.Deg2Rad * (float)(orbit.argumentPerihelion - orbit.longitudeAscendingNode)) * Math.Cos(Mathf.Deg2Rad * (float)(orbit.inclination)));
            double zh = r * (Math.Sin(v + Mathf.Deg2Rad * (float)(orbit.argumentPerihelion - orbit.longitudeAscendingNode)) * Math.Sin(Mathf.Deg2Rad * (float)(orbit.inclination)));
            // Geocentric (Earth-centered) position: subtract Earth's position (assume Earth at (1,0,0))
            double xe = xh - 1.0;
            double ye = yh;
            double ze = zh;
            // Normalize the planet position vector and place it outside the celestial sphere
            Vector3 planetDirection = new Vector3((float)xe, (float)ze, (float)ye).normalized;
            Vector3 planetPosition = planetDirection * planetShellRadius;
            if (planet.planetObject != null)
            {
                planet.planetObject.transform.localPosition = planetPosition;
                
                // Update planet label position if it exists
                if (showPlanetLabels && planetLabels.Count > planets.IndexOf(planet))
                {
                    GameObject labelObj = planetLabels[planets.IndexOf(planet)];
                    if (labelObj != null)
                    {
                        labelObj.transform.localPosition = planetPosition + planetDirection * 2f; // Offset label slightly outside planet
                    }
                }
            }
        }
    }

    private void FixIncompatibleMaterials()
    {
        // Scan all renderer types
        var allRenderers = new List<Renderer>();
        allRenderers.AddRange(FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None));
        allRenderers.AddRange(FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None));
        allRenderers.AddRange(FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None));

        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        Debug.Log($"[FixIncompatibleMaterials] Scanning {allRenderers.Count} renderers for incompatible shaders");

        if (urpUnlit == null)
        {
            Debug.LogError("[FixIncompatibleMaterials] Could not find URP Unlit shader!");
            return;
        }

        int fixedCount = 0;
        int disabledCount = 0;
        foreach (Renderer renderer in allRenderers)
        {
            try
            {
                Material[] mats = renderer.sharedMaterials;
                bool needsUpdate = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].shader != null)
                    {
                        string shaderName = mats[i].shader.name;
                        if (shaderName.Contains("Simulation/Standard") || shaderName.Contains("Standard Lit"))
                        {
                            Debug.LogWarning($"[FixIncompatibleMaterials] {renderer.gameObject.name}: '{shaderName}' INCOMPATIBLE");
                            if (urpUnlit != null)
                            {
                                Material newMat = new Material(urpUnlit);
                                newMat.name = mats[i].name + " (Fixed)";
                                if (mats[i].HasProperty("_Color"))
                                    newMat.SetColor("_BaseColor", mats[i].GetColor("_Color"));
                                if (mats[i].HasProperty("_MainTex"))
                                    newMat.SetTexture("_BaseMap", mats[i].GetTexture("_MainTex"));
                                mats[i] = newMat;
                                fixedCount++;
                                needsUpdate = true;
                            }
                            else
                            {
                                renderer.enabled = false;
                                disabledCount++;
                                Debug.LogWarning($"[FixIncompatibleMaterials] Disabled renderer: {renderer.gameObject.name}");
                            }
                        }
                    }
                }
                if (needsUpdate)
                {
                    renderer.sharedMaterials = mats;
                    Debug.Log($"[FixIncompatibleMaterials] Updated {renderer.gameObject.name}");
                }
            }
            catch (System.Exception ex)
            {
                renderer.enabled = false;
                disabledCount++;
                Debug.LogError($"[FixIncompatibleMaterials] Error processing {renderer.gameObject.name}: {ex.Message} - Renderer disabled");
            }
        }
        Debug.Log($"[FixIncompatibleMaterials] Fixed {fixedCount} materials, disabled {disabledCount} renderers");
    }

    private void DisableProblematicXRSampleObjects()
    {
        // Disable XR Interaction Toolkit sample objects that use incompatible shaders
        // These are from Assets/Samples/XR Interaction Toolkit and cause shader keyword mismatches
        
        try
        {
            // Find and disable problematic sample objects (Climbing Wall, Wall, Tabletop)
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Climbing Wall") || obj.name == "Wall" || obj.name == "Tabletop")
                {
                    // Check if this object has a renderer with incompatible shader
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Material[] mats = renderer.sharedMaterials;
                        foreach (Material mat in mats)
                        {
                            if (mat != null && mat.shader != null)
                            {
                                string shaderName = mat.shader.name;
                                if (shaderName.Contains("Simulation/Standard") || shaderName.Contains("Standard Lit"))
                                {
                                    Debug.LogWarning($"[DisableProblematicXRSampleObjects] Disabling {obj.name} - has incompatible shader: {shaderName}");
                                    renderer.enabled = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DisableProblematicXRSampleObjects] Error: {ex.Message}");
        }
    }

    private void ValidateOrCreateMaterials()
    {
        // Ensure starMaterial is safe to use
        if (starMaterial != null && starMaterial.shader != null)
        {
            string shaderName = starMaterial.shader.name;
            if (shaderName.Contains("Simulation/Standard") || shaderName.Contains("Standard Lit"))
            {
                Debug.LogWarning($"[ValidateOrCreateMaterials] starMaterial has incompatible shader: {shaderName}. Creating new one.");
                starMaterial = null; // Force recreation
            }
        }
        
        // Ensure starMaterialBright is safe to use
        if (starMaterialBright != null && starMaterialBright.shader != null)
        {
            string shaderName = starMaterialBright.shader.name;
            if (shaderName.Contains("Simulation/Standard") || shaderName.Contains("Standard Lit"))
            {
                Debug.LogWarning($"[ValidateOrCreateMaterials] starMaterialBright has incompatible shader: {shaderName}. Creating new one.");
                starMaterialBright = null; // Force recreation
            }
        }
    }

    private void CreatePrefabsAndMaterials()
    {
        // Only create star material if not assigned from Inspector
        if (starMaterial == null)
        {
            CreateStarMaterial();
        }
        
        // Create bright star material if needed and not assigned
        if (useDifferentBrightStarMaterial && starMaterialBright == null)
        {
            CreateBrightStarMaterial();
        }
        
        CreateStarPrefab();
        CreateLabelPrefabs();
    }

    private void CreateStarMaterial()
    {
        // Use URP Unlit shader to avoid keyword conflicts
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogWarning("Could not find URP Unlit shader, using built-in fallback");
            shader = Shader.Find("Standard");
        }

        starMaterial = new Material(shader);
        starMaterial.color = Color.white;
        starMaterial.SetColor("_BaseColor", Color.white);

        // Emission for glow effect
        starMaterial.SetColor("_EmissionColor", Color.white * colorIntensityMultiplier);
        starMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }
    
    private void CreateBrightStarMaterial()
    {
        // Create a brighter/more emissive material for bright stars using URP Unlit
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogWarning("Could not find URP Unlit shader, using built-in fallback");
            shader = Shader.Find("Standard");
        }

        starMaterialBright = new Material(shader);
        starMaterialBright.color = Color.white;
        starMaterialBright.SetColor("_BaseColor", Color.white);

        // More emissive for bright stars (1.5x intensity)
        float brightIntensity = colorIntensityMultiplier * 1.5f;
        starMaterialBright.SetColor("_EmissionColor", Color.white * brightIntensity);
        starMaterialBright.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    private void CreateStarPrefab()
    {
        starPrefab = new GameObject("StarPrefab");
        var sphere = starPrefab.AddComponent<SphereCollider>();
        sphere.radius = 0.5f;

        var renderer = starPrefab.AddComponent<MeshRenderer>();
        var filter = starPrefab.AddComponent<MeshFilter>();
        filter.mesh = CreateStarMesh();

        // Use custom material if assigned, otherwise use auto-created material
        Material materialToUse = useCustomStarMaterial && starMaterialCustom != null ? starMaterialCustom : starMaterial;
        var material = new Material(materialToUse);
        renderer.material = material;

        starPrefab.transform.localScale = Vector3.one * 0.1f;
        starPrefab.SetActive(false);
    }

    private Mesh CreateStarMesh()
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = primitive.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(primitive);
        return mesh;
    }

    private void CreateLabelPrefabs()
    {
        CreateStarLabelPrefab();
        CreateConstellationLabelPrefab();
    }

    private void CreateStarLabelPrefab()
    {
        starLabelPrefab = new GameObject("StarLabelPrefab");
        var canvas = starLabelPrefab.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        // Adjust default label scale based on platform for better readability on mobile vs desktop
        float labelScale = 1f;
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            labelScale = 1f; // slightly smaller on mobile
        }
        canvas.transform.localScale = Vector3.one * labelScale;

        // Add background for better readability
        var background = starLabelPrefab.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.7f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(starLabelPrefab.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one;

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Star Name\nConstellation";
        // Slightly smaller default font on mobile
        text.fontSize = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? 14 : 16;
        text.color = labelColor;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = false; // We'll handle sizing manually

        // Set up RectTransform for proper text positioning - increased for 2000 stars visibility
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(400, 120);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        starLabelPrefab.AddComponent<CenteredBillboardText>();
        starLabelPrefab.SetActive(false);
    }

    private void CreateConstellationLabelPrefab()
    {
        constellationLabelPrefab = new GameObject("ConstellationLabelPrefab");
        var canvas = constellationLabelPrefab.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Platform adaptive scale
        float constLabelScale = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? 0.012f : 0.015f;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(constellationLabelPrefab.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one * constLabelScale;

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Constellation";
        text.fontSize = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? 20 : 24;
        text.color = constellationLabelColor;
        text.alignment = TextAlignmentOptions.Center;

        constellationLabelPrefab.AddComponent<CenteredBillboardText>();
        constellationLabelPrefab.SetActive(false);
    }

    private void FindCelestialSphereController()
    {
        celestialSphereController = FindAnyObjectByType<CelestialSphereController>();
        if (celestialSphereController != null)
        {
            Debug.Log("CelestialSphereController found successfully");
            UpdateLocationFromCelestialSphere();
        }
        else
        {
            Debug.LogWarning("CelestialSphereController not found in scene. Using default location.");
        }
    }

    public void UpdateLocationFromCelestialSphere()
    {
        if (celestialSphereController != null && useCelestialSphereLocation)
        {
            try
            {
                latitude = (float)celestialSphereController.GetLatitude();
                longitude = (float)celestialSphereController.GetLongitude();
                Debug.Log($"StarDatabase location updated: Lat={latitude:F4}, Lon={longitude:F4}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not get location from CelestialSphereController: {e.Message}");
            }
        }
    }

    private void CreateGalacticCenterMarker(Transform celestialSphere, float sphereRadius)
    {
        GameObject galacticCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        galacticCenter.name = "Galactic_Center";
        galacticCenter.transform.SetParent(celestialSphere);

        Vector3 centerPosition = CalculateStarPosition(
            new StarData
            {
                rightAscension = 17.7611f,
                declination = -29.0078f
            },
            sphereRadius
        );

        galacticCenter.transform.localPosition = centerPosition;
        galacticCenter.transform.localScale = Vector3.one * 0.3f;

        Renderer renderer = galacticCenter.GetComponent<Renderer>();
        if (galacticCenterMaterial != null)
        {
            renderer.material = galacticCenterMaterial;
        }
        else
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = Color.yellow;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.yellow * 3f);
            material.SetFloat("_Glossiness", 0.0f);
            material.SetFloat("_Metallic", 0.0f);
            renderer.material = material;
        }

        CreateGalacticCenterLabel(celestialSphere, sphereRadius);
    }

    private void CreateGalacticCenterLabel(Transform celestialSphere, float sphereRadius)
    {
        GameObject labelObj = new GameObject("GalacticCenter_Label");
        labelObj.transform.SetParent(celestialSphere);

        Canvas canvas = labelObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.02f;

        // Add background
        var background = labelObj.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(labelObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "GALACTIC CENTER\nSagittarius A*";
        text.fontSize = 20;
        text.color = Color.yellow;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        labelObj.AddComponent<CenteredBillboardText>();

        Vector3 centerPosition = CalculateStarPosition(
            new StarData
            {
                rightAscension = 17.7611f,
                declination = -29.0078f
            },
            sphereRadius
        );

        labelObj.transform.localPosition = centerPosition.normalized * (sphereRadius + 0.5f);
    }

    public void CreateStarObjects(Transform celestialSphere, float sphereRadius)
    {
        ClearAllObjects();

        if (useCelestialSphereLocation)
        {
            UpdateLocationFromCelestialSphere();
        }

        Debug.Log($"[StarDatabase] Creating {brightStars.Count} stars with viewFromCelestialCenter={viewFromCelestialCenter}");
        
        foreach (var starData in brightStars)
        {
            CreateSingleStar(starData, celestialSphere, sphereRadius, starPrefab);
        }

        CreateGalacticCenterMarker(celestialSphere, sphereRadius);
        CreateLabels(celestialSphere, sphereRadius);
        UpdateStarPositions(sphereRadius);
    }

    public (float ra, float dec) GetGalacticCenterCoordinates()
    {
        return (17.7611f, -29.0078f);
    }

    public string GetGalacticCenterInfo()
    {
        return "Galactic Center Coordinates:\n" +
               "Right Ascension: 17h 45m 41s\n" +
               "Declination: -29° 00' 28\"\n" +
               "Location: Sagittarius Constellation\n" +
               "Contains: Supermassive black hole Sagittarius A*\n" +
               "Distance: ~26,000 light years from Earth";
    }

    private void InitializeStarDatabase()
    {
        // Top 50 brightest stars with expanded astrographic properties
        // Note: For celestial center view, we'll use a simplified approach
        
        // Add 50 bright stars with positions distributed around the celestial sphere
        // Right Ascension: 0-24 hours, Declination: -90 to +90 degrees
        
        // Example stars with positions that cover the entire celestial sphere
        brightStars.Add(new StarData {
            name = "Sirius",
            commonName = "Sirius A",
            spectralType = "A1V",
            bayerDesignation = "α CMa",
            constellation = "Canis Major",
            rightAscension = 6.7525f,
            declination = -16.7161f,
            magnitude = -1.46f,
            absoluteMagnitude = 1.42f,
            distanceLightYears = 8.6f,
            temperature = 9940f,
            radius = 1.711f,
            mass = 2.02f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(1.42f),
            starClass = StarClass.A,
            starType = StarType.MainSequence,
            color = GenerateStarColor(9940f)
        });

        brightStars.Add(new StarData {
            name = "Canopus",
            spectralType = "A9II",
            bayerDesignation = "α Car",
            constellation = "Carina",
            rightAscension = 6.3992f,
            declination = -52.6956f,
            magnitude = -0.74f,
            absoluteMagnitude = -5.53f,
            distanceLightYears = 310f,
            temperature = 7350f,
            radius = 71f,
            mass = 8.0f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(-5.53f),
            starClass = StarClass.A,
            starType = StarType.Giant,
            color = GenerateStarColor(7350f)
        });

        brightStars.Add(new StarData {
            name = "Arcturus",
            spectralType = "K1.5III",
            bayerDesignation = "α Boo",
            constellation = "Boötes",
            rightAscension = 14.2610f,
            declination = 19.1824f,
            magnitude = -0.05f,
            absoluteMagnitude = -0.30f,
            distanceLightYears = 36.7f,
            temperature = 4286f,
            radius = 25.4f,
            mass = 1.1f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(-0.30f),
            starClass = StarClass.K,
            starType = StarType.Giant,
            color = GenerateStarColor(4286f)
        });

        brightStars.Add(new StarData {
            name = "Vega",
            spectralType = "A0V",
            bayerDesignation = "α Lyr",
            constellation = "Lyra",
            rightAscension = 18.6156f,
            declination = 38.7836f,
            magnitude = 0.03f,
            absoluteMagnitude = 0.58f,
            distanceLightYears = 25.0f,
            temperature = 9602f,
            radius = 2.362f,
            mass = 2.14f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(0.58f),
            starClass = StarClass.A,
            starType = StarType.MainSequence,
            color = GenerateStarColor(9602f)
        });

        brightStars.Add(new StarData {
            name = "Capella",
            spectralType = "G8III",
            bayerDesignation = "α Aur",
            constellation = "Auriga",
            rightAscension = 5.2782f,
            declination = 45.9979f,
            magnitude = 0.08f,
            absoluteMagnitude = 0.35f,
            distanceLightYears = 42.9f,
            temperature = 4970f,
            radius = 12.2f,
            mass = 2.5f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(0.35f),
            starClass = StarClass.G,
            starType = StarType.Giant,
            color = GenerateStarColor(4970f)
        });

        // Add more stars with different positions...
        // For brevity, I'll add a few more key stars

        // Polaris (North Star)
        brightStars.Add(new StarData {
            name = "Polaris",
            spectralType = "F7Ib",
            bayerDesignation = "α UMi",
            constellation = "Ursa Minor",
            rightAscension = 2.5303f,
            declination = 89.2642f,
            magnitude = 1.98f,
            absoluteMagnitude = -3.64f,
            distanceLightYears = 433f,
            temperature = 6015f,
            radius = 45f,
            mass = 5.4f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(-3.64f),
            starClass = StarClass.F,
            starType = StarType.Supergiant,
            color = GenerateStarColor(6015f)
        });

        // Betelgeuse
        brightStars.Add(new StarData {
            name = "Betelgeuse",
            spectralType = "M2Iab",
            bayerDesignation = "α Ori",
            constellation = "Orion",
            rightAscension = 5.9195f,
            declination = 7.4071f,
            magnitude = 0.42f,
            absoluteMagnitude = -5.14f,
            distanceLightYears = 700f,
            temperature = 3500f,
            radius = 887f,
            mass = 11.6f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(-5.14f),
            starClass = StarClass.M,
            starType = StarType.Supergiant,
            color = GenerateStarColor(3500f)
        });

        // Rigel
        brightStars.Add(new StarData {
            name = "Rigel",
            spectralType = "B8Ia",
            bayerDesignation = "β Ori",
            constellation = "Orion",
            rightAscension = 5.2423f,
            declination = -8.2016f,
            magnitude = 0.12f,
            absoluteMagnitude = -6.7f,
            distanceLightYears = 860f,
            temperature = 12100f,
            radius = 78f,
            mass = 21f,
            luminosity = AstrographicCalculations.CalculateLuminosityFromMagnitude(-6.7f),
            starClass = StarClass.B,
            starType = StarType.Supergiant,
            color = GenerateStarColor(12100f)
        });

        // Continue adding more famous named stars
        AddMoreNamedStars();

        // Generate additional stars to reach ~2000 total
        GenerateExtendedStarCatalog();

        Debug.Log($"[StarDatabase] Initialized with {brightStars.Count} stars");
    }

    /// <summary>
    /// Add more well-known named stars from major constellations
    /// </summary>
    private void AddMoreNamedStars()
    {
        // Procyon (α CMi) - Canis Minor
        brightStars.Add(new StarData {
            name = "Procyon", spectralType = "F5IV-V", bayerDesignation = "α CMi",
            constellation = "Canis Minor", rightAscension = 7.6553f, declination = 5.2250f,
            magnitude = 0.34f, absoluteMagnitude = 2.65f, distanceLightYears = 11.46f,
            temperature = 6530f, radius = 2.048f, mass = 1.499f,
            starClass = StarClass.F, starType = StarType.MainSequence, color = GenerateStarColor(6530f)
        });

        // Achernar (α Eri) - Eridanus
        brightStars.Add(new StarData {
            name = "Achernar", spectralType = "B6Vep", bayerDesignation = "α Eri",
            constellation = "Eridanus", rightAscension = 1.6286f, declination = -57.2367f,
            magnitude = 0.46f, absoluteMagnitude = -2.77f, distanceLightYears = 139f,
            temperature = 14500f, radius = 9.16f, mass = 6.7f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(14500f)
        });

        // Hadar (β Cen) - Centaurus
        brightStars.Add(new StarData {
            name = "Hadar", spectralType = "B1III", bayerDesignation = "β Cen",
            constellation = "Centaurus", rightAscension = 14.0637f, declination = -60.3730f,
            magnitude = 0.61f, absoluteMagnitude = -5.42f, distanceLightYears = 525f,
            temperature = 25000f, radius = 12f, mass = 12.02f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(25000f)
        });

        // Altair (α Aql) - Aquila
        brightStars.Add(new StarData {
            name = "Altair", spectralType = "A7V", bayerDesignation = "α Aql",
            constellation = "Aquila", rightAscension = 19.8464f, declination = 8.8683f,
            magnitude = 0.76f, absoluteMagnitude = 2.21f, distanceLightYears = 16.73f,
            temperature = 7550f, radius = 1.79f, mass = 1.79f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(7550f)
        });

        // Acrux (α Cru) - Crux
        brightStars.Add(new StarData {
            name = "Acrux", spectralType = "B0.5IV", bayerDesignation = "α Cru",
            constellation = "Crux", rightAscension = 12.4433f, declination = -63.0992f,
            magnitude = 0.77f, absoluteMagnitude = -4.19f, distanceLightYears = 320f,
            temperature = 28000f, radius = 7.8f, mass = 17.8f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(28000f)
        });

        // Aldebaran (α Tau) - Taurus
        brightStars.Add(new StarData {
            name = "Aldebaran", spectralType = "K5III", bayerDesignation = "α Tau",
            constellation = "Taurus", rightAscension = 4.5987f, declination = 16.5093f,
            magnitude = 0.85f, absoluteMagnitude = -0.63f, distanceLightYears = 65.3f,
            temperature = 3910f, radius = 44.13f, mass = 1.16f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(3910f)
        });

        // Antares (α Sco) - Scorpius
        brightStars.Add(new StarData {
            name = "Antares", spectralType = "M1.5Iab", bayerDesignation = "α Sco",
            constellation = "Scorpius", rightAscension = 16.4901f, declination = -26.4320f,
            magnitude = 1.05f, absoluteMagnitude = -5.28f, distanceLightYears = 550f,
            temperature = 3570f, radius = 680f, mass = 12.4f,
            starClass = StarClass.M, starType = StarType.Supergiant, color = GenerateStarColor(3570f)
        });

        // Spica (α Vir) - Virgo
        brightStars.Add(new StarData {
            name = "Spica", spectralType = "B1III-IV", bayerDesignation = "α Vir",
            constellation = "Virgo", rightAscension = 13.4199f, declination = -11.1614f,
            magnitude = 0.97f, absoluteMagnitude = -3.55f, distanceLightYears = 250f,
            temperature = 25300f, radius = 7.47f, mass = 11.43f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(25300f)
        });

        // Pollux (β Gem) - Gemini
        brightStars.Add(new StarData {
            name = "Pollux", spectralType = "K0III", bayerDesignation = "β Gem",
            constellation = "Gemini", rightAscension = 7.7553f, declination = 28.0262f,
            magnitude = 1.14f, absoluteMagnitude = 1.08f, distanceLightYears = 33.78f,
            temperature = 4666f, radius = 9.06f, mass = 1.91f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4666f)
        });

        // Fomalhaut (α PsA) - Piscis Austrinus
        brightStars.Add(new StarData {
            name = "Fomalhaut", spectralType = "A4V", bayerDesignation = "α PsA",
            constellation = "Piscis Austrinus", rightAscension = 22.9608f, declination = -29.6222f,
            magnitude = 1.16f, absoluteMagnitude = 1.73f, distanceLightYears = 25.13f,
            temperature = 8590f, radius = 1.84f, mass = 1.92f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8590f)
        });

        // Deneb (α Cyg) - Cygnus
        brightStars.Add(new StarData {
            name = "Deneb", spectralType = "A2Ia", bayerDesignation = "α Cyg",
            constellation = "Cygnus", rightAscension = 20.6905f, declination = 45.2803f,
            magnitude = 1.25f, absoluteMagnitude = -8.38f, distanceLightYears = 2615f,
            temperature = 8525f, radius = 203f, mass = 19f,
            starClass = StarClass.A, starType = StarType.Supergiant, color = GenerateStarColor(8525f)
        });

        // Mimosa (β Cru) - Crux
        brightStars.Add(new StarData {
            name = "Mimosa", spectralType = "B0.5IV", bayerDesignation = "β Cru",
            constellation = "Crux", rightAscension = 12.7952f, declination = -59.6886f,
            magnitude = 1.25f, absoluteMagnitude = -3.92f, distanceLightYears = 280f,
            temperature = 27000f, radius = 8.4f, mass = 16f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(27000f)
        });

        // Regulus (α Leo) - Leo
        brightStars.Add(new StarData {
            name = "Regulus", spectralType = "B8IVn", bayerDesignation = "α Leo",
            constellation = "Leo", rightAscension = 10.1395f, declination = 11.9672f,
            magnitude = 1.35f, absoluteMagnitude = -0.52f, distanceLightYears = 77.5f,
            temperature = 12460f, radius = 4.35f, mass = 3.8f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(12460f)
        });

        // Adhara (ε CMa) - Canis Major
        brightStars.Add(new StarData {
            name = "Adhara", spectralType = "B2Iab", bayerDesignation = "ε CMa",
            constellation = "Canis Major", rightAscension = 6.9771f, declination = -28.9722f,
            magnitude = 1.50f, absoluteMagnitude = -4.10f, distanceLightYears = 430f,
            temperature = 22000f, radius = 13.9f, mass = 12.6f,
            starClass = StarClass.B, starType = StarType.Supergiant, color = GenerateStarColor(22000f)
        });

        // Castor (α Gem) - Gemini
        brightStars.Add(new StarData {
            name = "Castor", spectralType = "A1V", bayerDesignation = "α Gem",
            constellation = "Gemini", rightAscension = 7.5767f, declination = 31.8883f,
            magnitude = 1.58f, absoluteMagnitude = 0.59f, distanceLightYears = 51f,
            temperature = 10286f, radius = 2.4f, mass = 2.76f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(10286f)
        });

        // Gacrux (γ Cru) - Crux
        brightStars.Add(new StarData {
            name = "Gacrux", spectralType = "M3.5III", bayerDesignation = "γ Cru",
            constellation = "Crux", rightAscension = 12.5194f, declination = -57.1128f,
            magnitude = 1.63f, absoluteMagnitude = -0.56f, distanceLightYears = 88.6f,
            temperature = 3626f, radius = 84f, mass = 1.3f,
            starClass = StarClass.M, starType = StarType.Giant, color = GenerateStarColor(3626f)
        });

        // Shaula (λ Sco) - Scorpius
        brightStars.Add(new StarData {
            name = "Shaula", spectralType = "B2IV", bayerDesignation = "λ Sco",
            constellation = "Scorpius", rightAscension = 17.5601f, declination = -37.1038f,
            magnitude = 1.63f, absoluteMagnitude = -5.05f, distanceLightYears = 570f,
            temperature = 25000f, radius = 8.8f, mass = 14.5f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(25000f)
        });

        // Bellatrix (γ Ori) - Orion
        brightStars.Add(new StarData {
            name = "Bellatrix", spectralType = "B2III", bayerDesignation = "γ Ori",
            constellation = "Orion", rightAscension = 5.4188f, declination = 6.3497f,
            magnitude = 1.64f, absoluteMagnitude = -2.78f, distanceLightYears = 250f,
            temperature = 22000f, radius = 5.75f, mass = 8.6f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(22000f)
        });

        // Elnath (β Tau) - Taurus
        brightStars.Add(new StarData {
            name = "Elnath", spectralType = "B7III", bayerDesignation = "β Tau",
            constellation = "Taurus", rightAscension = 5.4382f, declination = 28.6074f,
            magnitude = 1.65f, absoluteMagnitude = -1.34f, distanceLightYears = 134f,
            temperature = 13600f, radius = 4.2f, mass = 5.0f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(13600f)
        });

        // Miaplacidus (β Car) - Carina
        brightStars.Add(new StarData {
            name = "Miaplacidus", spectralType = "A2IV", bayerDesignation = "β Car",
            constellation = "Carina", rightAscension = 9.2199f, declination = -69.7172f,
            magnitude = 1.67f, absoluteMagnitude = -0.99f, distanceLightYears = 111f,
            temperature = 8866f, radius = 6.8f, mass = 3.5f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8866f)
        });

        // Alnilam (ε Ori) - Orion
        brightStars.Add(new StarData {
            name = "Alnilam", spectralType = "B0Ia", bayerDesignation = "ε Ori",
            constellation = "Orion", rightAscension = 5.6036f, declination = -1.2019f,
            magnitude = 1.69f, absoluteMagnitude = -6.37f, distanceLightYears = 2000f,
            temperature = 27000f, radius = 42f, mass = 40f,
            starClass = StarClass.B, starType = StarType.Supergiant, color = GenerateStarColor(27000f)
        });

        // Alnitak (ζ Ori) - Orion
        brightStars.Add(new StarData {
            name = "Alnitak", spectralType = "O9.7Ib", bayerDesignation = "ζ Ori",
            constellation = "Orion", rightAscension = 5.6789f, declination = -1.9425f,
            magnitude = 1.74f, absoluteMagnitude = -6.0f, distanceLightYears = 1260f,
            temperature = 29500f, radius = 20f, mass = 33f,
            starClass = StarClass.O, starType = StarType.Supergiant, color = GenerateStarColor(29500f)
        });

        // Alioth (ε UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Alioth", spectralType = "A1III-IVp", bayerDesignation = "ε UMa",
            constellation = "Ursa Major", rightAscension = 12.9004f, declination = 55.9598f,
            magnitude = 1.77f, absoluteMagnitude = -0.21f, distanceLightYears = 82.6f,
            temperature = 9020f, radius = 4.14f, mass = 2.91f,
            starClass = StarClass.A, starType = StarType.Giant, color = GenerateStarColor(9020f)
        });

        // Mirfak (α Per) - Perseus
        brightStars.Add(new StarData {
            name = "Mirfak", spectralType = "F5Ib", bayerDesignation = "α Per",
            constellation = "Perseus", rightAscension = 3.4054f, declination = 49.8612f,
            magnitude = 1.79f, absoluteMagnitude = -4.50f, distanceLightYears = 510f,
            temperature = 6350f, radius = 68f, mass = 8.5f,
            starClass = StarClass.F, starType = StarType.Supergiant, color = GenerateStarColor(6350f)
        });

        // Dubhe (α UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Dubhe", spectralType = "K0III", bayerDesignation = "α UMa",
            constellation = "Ursa Major", rightAscension = 11.0621f, declination = 61.7510f,
            magnitude = 1.81f, absoluteMagnitude = -1.09f, distanceLightYears = 123f,
            temperature = 4660f, radius = 30f, mass = 4.25f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4660f)
        });

        // Wezen (δ CMa) - Canis Major
        brightStars.Add(new StarData {
            name = "Wezen", spectralType = "F8Ia", bayerDesignation = "δ CMa",
            constellation = "Canis Major", rightAscension = 7.1396f, declination = -26.3932f,
            magnitude = 1.83f, absoluteMagnitude = -6.87f, distanceLightYears = 1800f,
            temperature = 5818f, radius = 215f, mass = 17f,
            starClass = StarClass.F, starType = StarType.Supergiant, color = GenerateStarColor(5818f)
        });

        // Kaus Australis (ε Sgr) - Sagittarius
        brightStars.Add(new StarData {
            name = "Kaus Australis", spectralType = "B9.5III", bayerDesignation = "ε Sgr",
            constellation = "Sagittarius", rightAscension = 18.4029f, declination = -34.3845f,
            magnitude = 1.85f, absoluteMagnitude = -1.44f, distanceLightYears = 143f,
            temperature = 9960f, radius = 6.8f, mass = 3.52f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(9960f)
        });

        // Alkaid (η UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Alkaid", spectralType = "B3V", bayerDesignation = "η UMa",
            constellation = "Ursa Major", rightAscension = 13.7923f, declination = 49.3133f,
            magnitude = 1.85f, absoluteMagnitude = -0.60f, distanceLightYears = 103.9f,
            temperature = 15540f, radius = 3.4f, mass = 6.1f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(15540f)
        });

        // Sargas (θ Sco) - Scorpius
        brightStars.Add(new StarData {
            name = "Sargas", spectralType = "F1II", bayerDesignation = "θ Sco",
            constellation = "Scorpius", rightAscension = 17.6224f, declination = -42.9973f,
            magnitude = 1.87f, absoluteMagnitude = -2.75f, distanceLightYears = 272f,
            temperature = 7268f, radius = 26f, mass = 5.7f,
            starClass = StarClass.F, starType = StarType.Giant, color = GenerateStarColor(7268f)
        });

        // Avior (ε Car) - Carina
        brightStars.Add(new StarData {
            name = "Avior", spectralType = "K3III+B2V", bayerDesignation = "ε Car",
            constellation = "Carina", rightAscension = 8.3752f, declination = -59.5095f,
            magnitude = 1.86f, absoluteMagnitude = -4.58f, distanceLightYears = 632f,
            temperature = 4050f, radius = 200f, mass = 10.5f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4050f)
        });

        // Menkalinan (β Aur) - Auriga
        brightStars.Add(new StarData {
            name = "Menkalinan", spectralType = "A1IV", bayerDesignation = "β Aur",
            constellation = "Auriga", rightAscension = 5.9929f, declination = 44.9475f,
            magnitude = 1.90f, absoluteMagnitude = -0.10f, distanceLightYears = 81.1f,
            temperature = 9200f, radius = 2.77f, mass = 2.39f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(9200f)
        });

        // Atria (α TrA) - Triangulum Australe
        brightStars.Add(new StarData {
            name = "Atria", spectralType = "K2Ib-IIa", bayerDesignation = "α TrA",
            constellation = "Triangulum Australe", rightAscension = 16.8110f, declination = -69.0277f,
            magnitude = 1.91f, absoluteMagnitude = -3.68f, distanceLightYears = 415f,
            temperature = 4150f, radius = 143f, mass = 7f,
            starClass = StarClass.K, starType = StarType.Supergiant, color = GenerateStarColor(4150f)
        });

        // Alhena (γ Gem) - Gemini
        brightStars.Add(new StarData {
            name = "Alhena", spectralType = "A1.5IV+", bayerDesignation = "γ Gem",
            constellation = "Gemini", rightAscension = 6.6285f, declination = 16.3993f,
            magnitude = 1.93f, absoluteMagnitude = -0.60f, distanceLightYears = 109f,
            temperature = 9260f, radius = 3.3f, mass = 2.81f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(9260f)
        });

        // Peacock (α Pav) - Pavo
        brightStars.Add(new StarData {
            name = "Peacock", spectralType = "B2IV", bayerDesignation = "α Pav",
            constellation = "Pavo", rightAscension = 20.4275f, declination = -56.7350f,
            magnitude = 1.94f, absoluteMagnitude = -1.82f, distanceLightYears = 179f,
            temperature = 17711f, radius = 4.83f, mass = 5.91f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(17711f)
        });

        // Mirzam (β CMa) - Canis Major
        brightStars.Add(new StarData {
            name = "Mirzam", spectralType = "B1II-III", bayerDesignation = "β CMa",
            constellation = "Canis Major", rightAscension = 6.3785f, declination = -17.9559f,
            magnitude = 1.98f, absoluteMagnitude = -3.95f, distanceLightYears = 500f,
            temperature = 25000f, radius = 9.7f, mass = 13.5f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(25000f)
        });

        // Alphard (α Hya) - Hydra
        brightStars.Add(new StarData {
            name = "Alphard", spectralType = "K3II-III", bayerDesignation = "α Hya",
            constellation = "Hydra", rightAscension = 9.4598f, declination = -8.6586f,
            magnitude = 1.99f, absoluteMagnitude = -1.69f, distanceLightYears = 177f,
            temperature = 4120f, radius = 50.5f, mass = 3.03f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4120f)
        });

        // Hamal (α Ari) - Aries
        brightStars.Add(new StarData {
            name = "Hamal", spectralType = "K2III", bayerDesignation = "α Ari",
            constellation = "Aries", rightAscension = 2.1196f, declination = 23.4625f,
            magnitude = 2.01f, absoluteMagnitude = 0.47f, distanceLightYears = 65.8f,
            temperature = 4480f, radius = 14.9f, mass = 1.5f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4480f)
        });

        // Diphda (β Cet) - Cetus
        brightStars.Add(new StarData {
            name = "Diphda", spectralType = "K0III", bayerDesignation = "β Cet",
            constellation = "Cetus", rightAscension = 0.7265f, declination = -17.9866f,
            magnitude = 2.04f, absoluteMagnitude = -0.30f, distanceLightYears = 96.3f,
            temperature = 4797f, radius = 16.78f, mass = 2.8f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4797f)
        });

        // Nunki (σ Sgr) - Sagittarius
        brightStars.Add(new StarData {
            name = "Nunki", spectralType = "B2.5V", bayerDesignation = "σ Sgr",
            constellation = "Sagittarius", rightAscension = 18.9211f, declination = -26.2967f,
            magnitude = 2.05f, absoluteMagnitude = -2.14f, distanceLightYears = 228f,
            temperature = 20000f, radius = 4.5f, mass = 7.8f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(20000f)
        });

        // Menkent (θ Cen) - Centaurus
        brightStars.Add(new StarData {
            name = "Menkent", spectralType = "K0III", bayerDesignation = "θ Cen",
            constellation = "Centaurus", rightAscension = 14.1114f, declination = -36.3700f,
            magnitude = 2.06f, absoluteMagnitude = 0.70f, distanceLightYears = 60.9f,
            temperature = 4980f, radius = 10.6f, mass = 1.27f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4980f)
        });

        // Saiph (κ Ori) - Orion
        brightStars.Add(new StarData {
            name = "Saiph", spectralType = "B0.5Ia", bayerDesignation = "κ Ori",
            constellation = "Orion", rightAscension = 5.7959f, declination = -9.6697f,
            magnitude = 2.07f, absoluteMagnitude = -4.65f, distanceLightYears = 650f,
            temperature = 26500f, radius = 22.2f, mass = 15.5f,
            starClass = StarClass.B, starType = StarType.Supergiant, color = GenerateStarColor(26500f)
        });

        // Mintaka (δ Ori) - Orion
        brightStars.Add(new StarData {
            name = "Mintaka", spectralType = "O9.5II", bayerDesignation = "δ Ori",
            constellation = "Orion", rightAscension = 5.5335f, declination = -0.2991f,
            magnitude = 2.25f, absoluteMagnitude = -4.99f, distanceLightYears = 916f,
            temperature = 29500f, radius = 16.5f, mass = 24f,
            starClass = StarClass.O, starType = StarType.Giant, color = GenerateStarColor(29500f)
        });

        // Merak (β UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Merak", spectralType = "A1V", bayerDesignation = "β UMa",
            constellation = "Ursa Major", rightAscension = 11.0308f, declination = 56.3825f,
            magnitude = 2.34f, absoluteMagnitude = 0.41f, distanceLightYears = 79.7f,
            temperature = 9377f, radius = 3.021f, mass = 2.7f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(9377f)
        });

        // Phecda (γ UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Phecda", spectralType = "A0Ve", bayerDesignation = "γ UMa",
            constellation = "Ursa Major", rightAscension = 11.8971f, declination = 53.6948f,
            magnitude = 2.41f, absoluteMagnitude = 0.36f, distanceLightYears = 83.2f,
            temperature = 9355f, radius = 2.91f, mass = 2.94f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(9355f)
        });

        // Megrez (δ UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Megrez", spectralType = "A3V", bayerDesignation = "δ UMa",
            constellation = "Ursa Major", rightAscension = 12.2571f, declination = 57.0326f,
            magnitude = 3.32f, absoluteMagnitude = 1.33f, distanceLightYears = 80.5f,
            temperature = 8630f, radius = 1.4f, mass = 1.63f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8630f)
        });

        // Mizar (ζ UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Mizar", spectralType = "A2V", bayerDesignation = "ζ UMa",
            constellation = "Ursa Major", rightAscension = 13.3989f, declination = 54.9254f,
            magnitude = 2.23f, absoluteMagnitude = 0.33f, distanceLightYears = 78f,
            temperature = 9000f, radius = 2.4f, mass = 2.2f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(9000f)
        });

        // Alcor (80 UMa) - Ursa Major
        brightStars.Add(new StarData {
            name = "Alcor", spectralType = "A5V", bayerDesignation = "80 UMa",
            constellation = "Ursa Major", rightAscension = 13.4206f, declination = 54.9880f,
            magnitude = 3.99f, absoluteMagnitude = 2.00f, distanceLightYears = 81.7f,
            temperature = 8000f, radius = 1.84f, mass = 1.8f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8000f)
        });

        // Kochab (β UMi) - Ursa Minor
        brightStars.Add(new StarData {
            name = "Kochab", spectralType = "K4III", bayerDesignation = "β UMi",
            constellation = "Ursa Minor", rightAscension = 14.8451f, declination = 74.1555f,
            magnitude = 2.07f, absoluteMagnitude = -0.87f, distanceLightYears = 130.9f,
            temperature = 4030f, radius = 42.1f, mass = 2.2f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4030f)
        });

        // Schedar (α Cas) - Cassiopeia
        brightStars.Add(new StarData {
            name = "Schedar", spectralType = "K0IIIa", bayerDesignation = "α Cas",
            constellation = "Cassiopeia", rightAscension = 0.6751f, declination = 56.5373f,
            magnitude = 2.24f, absoluteMagnitude = -1.99f, distanceLightYears = 228f,
            temperature = 4530f, radius = 45.4f, mass = 4f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4530f)
        });

        // Caph (β Cas) - Cassiopeia
        brightStars.Add(new StarData {
            name = "Caph", spectralType = "F2III-IV", bayerDesignation = "β Cas",
            constellation = "Cassiopeia", rightAscension = 0.1526f, declination = 59.1498f,
            magnitude = 2.28f, absoluteMagnitude = 1.17f, distanceLightYears = 54.7f,
            temperature = 7079f, radius = 3.5f, mass = 1.91f,
            starClass = StarClass.F, starType = StarType.Giant, color = GenerateStarColor(7079f)
        });

        // Ruchbah (δ Cas) - Cassiopeia
        brightStars.Add(new StarData {
            name = "Ruchbah", spectralType = "A5III-IV", bayerDesignation = "δ Cas",
            constellation = "Cassiopeia", rightAscension = 1.4303f, declination = 60.2353f,
            magnitude = 2.66f, absoluteMagnitude = 0.24f, distanceLightYears = 99.4f,
            temperature = 8400f, radius = 3.9f, mass = 2.49f,
            starClass = StarClass.A, starType = StarType.Giant, color = GenerateStarColor(8400f)
        });

        // Segin (ε Cas) - Cassiopeia
        brightStars.Add(new StarData {
            name = "Segin", spectralType = "B3III", bayerDesignation = "ε Cas",
            constellation = "Cassiopeia", rightAscension = 1.9066f, declination = 63.6701f,
            magnitude = 3.35f, absoluteMagnitude = -2.31f, distanceLightYears = 410f,
            temperature = 15000f, radius = 6f, mass = 9.2f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(15000f)
        });

        // Algol (β Per) - Perseus
        brightStars.Add(new StarData {
            name = "Algol", spectralType = "B8V", bayerDesignation = "β Per",
            constellation = "Perseus", rightAscension = 3.1363f, declination = 40.9557f,
            magnitude = 2.12f, absoluteMagnitude = -0.07f, distanceLightYears = 92.8f,
            temperature = 13000f, radius = 2.73f, mass = 3.17f, isVariable = true,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(13000f)
        });

        // Almach (γ And) - Andromeda
        brightStars.Add(new StarData {
            name = "Almach", spectralType = "K3IIb", bayerDesignation = "γ And",
            constellation = "Andromeda", rightAscension = 2.0650f, declination = 42.3297f,
            magnitude = 2.10f, absoluteMagnitude = -3.08f, distanceLightYears = 355f,
            temperature = 4250f, radius = 80f, mass = 6f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4250f)
        });

        // Mirach (β And) - Andromeda
        brightStars.Add(new StarData {
            name = "Mirach", spectralType = "M0III", bayerDesignation = "β And",
            constellation = "Andromeda", rightAscension = 1.1622f, declination = 35.6206f,
            magnitude = 2.07f, absoluteMagnitude = -1.76f, distanceLightYears = 197f,
            temperature = 3842f, radius = 100f, mass = 3.5f,
            starClass = StarClass.M, starType = StarType.Giant, color = GenerateStarColor(3842f)
        });

        // Alpheratz (α And) - Andromeda
        brightStars.Add(new StarData {
            name = "Alpheratz", spectralType = "B8IVp", bayerDesignation = "α And",
            constellation = "Andromeda", rightAscension = 0.1398f, declination = 29.0905f,
            magnitude = 2.07f, absoluteMagnitude = -0.30f, distanceLightYears = 97f,
            temperature = 13800f, radius = 2.7f, mass = 3.8f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(13800f)
        });

        // Denebola (β Leo) - Leo
        brightStars.Add(new StarData {
            name = "Denebola", spectralType = "A3V", bayerDesignation = "β Leo",
            constellation = "Leo", rightAscension = 11.8177f, declination = 14.5720f,
            magnitude = 2.14f, absoluteMagnitude = 1.92f, distanceLightYears = 35.9f,
            temperature = 8500f, radius = 1.728f, mass = 1.78f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8500f)
        });

        // Algieba (γ Leo) - Leo
        brightStars.Add(new StarData {
            name = "Algieba", spectralType = "K1III+G7III", bayerDesignation = "γ Leo",
            constellation = "Leo", rightAscension = 10.3327f, declination = 19.8418f,
            magnitude = 2.01f, absoluteMagnitude = -0.92f, distanceLightYears = 130f,
            temperature = 4470f, radius = 31.8f, mass = 1.23f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4470f)
        });

        // Zosma (δ Leo) - Leo
        brightStars.Add(new StarData {
            name = "Zosma", spectralType = "A4V", bayerDesignation = "δ Leo",
            constellation = "Leo", rightAscension = 11.2351f, declination = 20.5239f,
            magnitude = 2.56f, absoluteMagnitude = 1.32f, distanceLightYears = 58.4f,
            temperature = 8296f, radius = 2.14f, mass = 2.2f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8296f)
        });

        // Rasalhague (α Oph) - Ophiuchus
        brightStars.Add(new StarData {
            name = "Rasalhague", spectralType = "A5III", bayerDesignation = "α Oph",
            constellation = "Ophiuchus", rightAscension = 17.5822f, declination = 12.5600f,
            magnitude = 2.08f, absoluteMagnitude = 1.30f, distanceLightYears = 48.6f,
            temperature = 8000f, radius = 2.6f, mass = 2.4f,
            starClass = StarClass.A, starType = StarType.Giant, color = GenerateStarColor(8000f)
        });

        // Sabik (η Oph) - Ophiuchus
        brightStars.Add(new StarData {
            name = "Sabik", spectralType = "A2.5V", bayerDesignation = "η Oph",
            constellation = "Ophiuchus", rightAscension = 17.1726f, declination = -15.7250f,
            magnitude = 2.43f, absoluteMagnitude = 0.37f, distanceLightYears = 84f,
            temperature = 8900f, radius = 2.5f, mass = 2.2f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(8900f)
        });

        // Eltanin (γ Dra) - Draco
        brightStars.Add(new StarData {
            name = "Eltanin", spectralType = "K5III", bayerDesignation = "γ Dra",
            constellation = "Draco", rightAscension = 17.9434f, declination = 51.4889f,
            magnitude = 2.23f, absoluteMagnitude = -1.04f, distanceLightYears = 148f,
            temperature = 3930f, radius = 48.15f, mass = 1.72f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(3930f)
        });

        // Rastaban (β Dra) - Draco
        brightStars.Add(new StarData {
            name = "Rastaban", spectralType = "G2Ib-II", bayerDesignation = "β Dra",
            constellation = "Draco", rightAscension = 17.5072f, declination = 52.3014f,
            magnitude = 2.79f, absoluteMagnitude = -2.43f, distanceLightYears = 380f,
            temperature = 5160f, radius = 40f, mass = 6f,
            starClass = StarClass.G, starType = StarType.Supergiant, color = GenerateStarColor(5160f)
        });

        // Thuban (α Dra) - Draco (former pole star)
        brightStars.Add(new StarData {
            name = "Thuban", spectralType = "A0III", bayerDesignation = "α Dra",
            constellation = "Draco", rightAscension = 14.0732f, declination = 64.3758f,
            magnitude = 3.67f, absoluteMagnitude = -1.20f, distanceLightYears = 303f,
            temperature = 10100f, radius = 3.4f, mass = 3.4f,
            starClass = StarClass.A, starType = StarType.Giant, color = GenerateStarColor(10100f)
        });

        // Sadr (γ Cyg) - Cygnus
        brightStars.Add(new StarData {
            name = "Sadr", spectralType = "F8Ib", bayerDesignation = "γ Cyg",
            constellation = "Cygnus", rightAscension = 20.3702f, declination = 40.2567f,
            magnitude = 2.23f, absoluteMagnitude = -6.12f, distanceLightYears = 1800f,
            temperature = 5790f, radius = 150f, mass = 12.11f,
            starClass = StarClass.F, starType = StarType.Supergiant, color = GenerateStarColor(5790f)
        });

        // Gienah Cygni (ε Cyg) - Cygnus
        brightStars.Add(new StarData {
            name = "Gienah Cygni", spectralType = "K0III", bayerDesignation = "ε Cyg",
            constellation = "Cygnus", rightAscension = 20.7703f, declination = 33.9703f,
            magnitude = 2.48f, absoluteMagnitude = 0.76f, distanceLightYears = 72.7f,
            temperature = 4710f, radius = 12f, mass = 2f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4710f)
        });

        // Alderamin (α Cep) - Cepheus
        brightStars.Add(new StarData {
            name = "Alderamin", spectralType = "A7IV-V", bayerDesignation = "α Cep",
            constellation = "Cepheus", rightAscension = 21.3096f, declination = 62.5856f,
            magnitude = 2.45f, absoluteMagnitude = 1.58f, distanceLightYears = 49f,
            temperature = 7740f, radius = 2.5f, mass = 1.74f,
            starClass = StarClass.A, starType = StarType.MainSequence, color = GenerateStarColor(7740f)
        });

        // Errai (γ Cep) - Cepheus
        brightStars.Add(new StarData {
            name = "Errai", spectralType = "K1III-IV", bayerDesignation = "γ Cep",
            constellation = "Cepheus", rightAscension = 23.6554f, declination = 77.6324f,
            magnitude = 3.21f, absoluteMagnitude = 2.51f, distanceLightYears = 45f,
            temperature = 4792f, radius = 4.93f, mass = 1.4f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4792f)
        });

        // Enif (ε Peg) - Pegasus
        brightStars.Add(new StarData {
            name = "Enif", spectralType = "K2Ib", bayerDesignation = "ε Peg",
            constellation = "Pegasus", rightAscension = 21.7364f, declination = 9.8750f,
            magnitude = 2.38f, absoluteMagnitude = -4.19f, distanceLightYears = 672f,
            temperature = 4379f, radius = 185f, mass = 10.7f,
            starClass = StarClass.K, starType = StarType.Supergiant, color = GenerateStarColor(4379f)
        });

        // Scheat (β Peg) - Pegasus
        brightStars.Add(new StarData {
            name = "Scheat", spectralType = "M2.5II-III", bayerDesignation = "β Peg",
            constellation = "Pegasus", rightAscension = 23.0629f, declination = 28.0828f,
            magnitude = 2.44f, absoluteMagnitude = -1.49f, distanceLightYears = 196f,
            temperature = 3689f, radius = 95f, mass = 2.1f, isVariable = true,
            starClass = StarClass.M, starType = StarType.Giant, color = GenerateStarColor(3689f)
        });

        // Markab (α Peg) - Pegasus
        brightStars.Add(new StarData {
            name = "Markab", spectralType = "B9III", bayerDesignation = "α Peg",
            constellation = "Pegasus", rightAscension = 23.0793f, declination = 15.2053f,
            magnitude = 2.49f, absoluteMagnitude = -0.67f, distanceLightYears = 140f,
            temperature = 10100f, radius = 4.62f, mass = 3.5f,
            starClass = StarClass.B, starType = StarType.Giant, color = GenerateStarColor(10100f)
        });

        // Algenib (γ Peg) - Pegasus
        brightStars.Add(new StarData {
            name = "Algenib", spectralType = "B2IV", bayerDesignation = "γ Peg",
            constellation = "Pegasus", rightAscension = 0.2201f, declination = 15.1836f,
            magnitude = 2.83f, absoluteMagnitude = -2.22f, distanceLightYears = 335f,
            temperature = 21179f, radius = 4.8f, mass = 8.9f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(21179f)
        });

        // Ankaa (α Phe) - Phoenix
        brightStars.Add(new StarData {
            name = "Ankaa", spectralType = "K0III", bayerDesignation = "α Phe",
            constellation = "Phoenix", rightAscension = 0.4381f, declination = -42.3061f,
            magnitude = 2.40f, absoluteMagnitude = 0.52f, distanceLightYears = 77f,
            temperature = 4436f, radius = 15f, mass = 1.57f,
            starClass = StarClass.K, starType = StarType.Giant, color = GenerateStarColor(4436f)
        });

        // Alnair (α Gru) - Grus
        brightStars.Add(new StarData {
            name = "Alnair", spectralType = "B7IV", bayerDesignation = "α Gru",
            constellation = "Grus", rightAscension = 22.1372f, declination = -46.9611f,
            magnitude = 1.73f, absoluteMagnitude = -0.73f, distanceLightYears = 101f,
            temperature = 13920f, radius = 3.4f, mass = 4f,
            starClass = StarClass.B, starType = StarType.MainSequence, color = GenerateStarColor(13920f)
        });
    }

    /// <summary>
    /// Generate extended star catalog to reach approximately 2000 stars
    /// Uses procedural generation with realistic celestial coordinates
    /// </summary>
    private void GenerateExtendedStarCatalog()
    {
        // Define constellation regions with their typical RA/Dec ranges
        var constellationRegions = new List<(string name, float raMin, float raMax, float decMin, float decMax)>
        {
            ("Orion", 4.5f, 6.5f, -12f, 22f),
            ("Ursa Major", 8f, 14f, 28f, 70f),
            ("Cassiopeia", 22f, 3f, 46f, 77f),
            ("Cygnus", 19f, 22f, 27f, 61f),
            ("Scorpius", 15.5f, 18f, -45f, -8f),
            ("Leo", 9f, 12f, -6f, 34f),
            ("Lyra", 18f, 19.5f, 25f, 48f),
            ("Andromeda", 22.5f, 2.5f, 21f, 53f),
            ("Taurus", 3f, 6f, 0f, 31f),
            ("Gemini", 5.5f, 8f, 10f, 35f),
            ("Canis Major", 6f, 7.5f, -33f, -11f),
            ("Virgo", 11.5f, 15f, -22f, 15f),
            ("Bootes", 13.5f, 16f, 7f, 55f),
            ("Perseus", 1.5f, 4.5f, 30f, 59f),
            ("Hercules", 15.5f, 18.5f, 4f, 51f),
            ("Centaurus", 11f, 15f, -64f, -29f),
            ("Carina", 6f, 11f, -75f, -50f),
            ("Crux", 11.5f, 13f, -65f, -55f),
            ("Auriga", 4.5f, 7.5f, 28f, 56f),
            ("Ophiuchus", 16f, 18.5f, -30f, 14f),
            ("Sagittarius", 17.5f, 20f, -45f, -12f),
            ("Aquarius", 20.5f, 23.5f, -25f, 3f),
            ("Pisces", 22.5f, 2f, -7f, 34f),
            ("Capricornus", 20f, 22f, -28f, -8f),
            ("Aquila", 18.5f, 20.5f, -12f, 19f),
            ("Draco", 9f, 21f, 47f, 86f),
            ("Cepheus", 20f, 8f, 53f, 88f),
            ("Pegasus", 21f, 1f, 2f, 36f),
            ("Phoenix", 23f, 2.5f, -58f, -40f),
            ("Grus", 21.5f, 23.5f, -57f, -37f),
            ("Pavo", 17.5f, 21.5f, -75f, -57f),
            ("Tucana", 22f, 1.5f, -75f, -57f),
            ("Eridanus", 1.5f, 5f, -58f, 0f),
            ("Hydra", 8f, 15f, -35f, 7f),
            ("Puppis", 6f, 9f, -51f, -11f),
            ("Vela", 8f, 11f, -57f, -37f),
            ("Lupus", 14f, 16.5f, -55f, -30f),
            ("Ara", 16.5f, 18f, -68f, -45f),
            ("Corona Australis", 17.5f, 19.5f, -46f, -37f),
            ("Triangulum Australe", 14.5f, 17f, -70f, -60f),
            ("Norma", 15.5f, 17f, -60f, -42f),
            ("Telescopium", 18f, 20.5f, -57f, -45f),
            ("Indus", 20f, 23f, -75f, -45f),
            ("Microscopium", 20f, 21.5f, -45f, -27f),
            ("Sculptor", 23f, 2f, -40f, -24f),
            ("Fornax", 2f, 4f, -40f, -24f),
            ("Horologium", 2.5f, 4.5f, -67f, -40f),
            ("Reticulum", 3f, 5f, -67f, -53f),
            ("Pictor", 4f, 7f, -64f, -43f),
            ("Dorado", 3.5f, 6.5f, -70f, -49f),
            ("Volans", 6.5f, 9f, -75f, -64f),
            ("Mensa", 3.5f, 7.5f, -85f, -70f),
            ("Chamaeleon", 7.5f, 13.5f, -83f, -75f),
            ("Musca", 11f, 14f, -75f, -64f),
            ("Circinus", 13.5f, 15.5f, -70f, -55f),
            ("Apus", 13.5f, 18.5f, -83f, -67f),
            ("Octans", 0f, 24f, -90f, -74f),
            ("Hydrus", 0f, 4.5f, -82f, -58f),
            ("Camelopardalis", 3f, 14.5f, 52f, 86f),
            ("Lynx", 6f, 9.5f, 33f, 62f),
            ("Cancer", 7.5f, 9.5f, 6f, 33f),
            ("Canis Minor", 7f, 8.5f, 0f, 13f),
            ("Monoceros", 5.5f, 8.5f, -12f, 12f),
            ("Lepus", 4.5f, 6.5f, -27f, -11f),
            ("Columba", 5f, 7f, -43f, -27f),
            ("Caelum", 4f, 5.5f, -49f, -37f),
            ("Corvus", 11.5f, 12.75f, -25f, -11f),
            ("Crater", 10.5f, 12f, -25f, -6f),
            ("Sextans", 9.5f, 11f, -12f, 6f),
            ("Antlia", 9f, 11f, -40f, -24f),
            ("Pyxis", 8.5f, 9.5f, -37f, -17f),
            ("Coma Berenices", 11.5f, 13.5f, 14f, 34f),
            ("Canes Venatici", 12f, 14.5f, 27f, 53f),
            ("Ursa Minor", 0f, 24f, 65f, 90f),
            ("Corona Borealis", 15f, 16.5f, 25f, 40f),
            ("Serpens", 15f, 19f, -16f, 26f),
            ("Libra", 14f, 16f, -30f, 0f),
            ("Scutum", 18f, 19f, -16f, -4f),
            ("Sagitta", 19f, 20.5f, 16f, 22f),
            ("Vulpecula", 19f, 21.5f, 19f, 29f),
            ("Delphinus", 20f, 21.5f, 2f, 21f),
            ("Equuleus", 20.5f, 21.5f, 2f, 13f),
            ("Lacerta", 21.5f, 23f, 35f, 57f),
            ("Triangulum", 1f, 3f, 25f, 37f),
            ("Aries", 1.5f, 3.5f, 10f, 31f),
        };

        int starId = brightStars.Count + 1;
        int targetStarCount = 2000;
        System.Random rng = new System.Random(42); // Fixed seed for reproducibility

        // Spectral type distributions by temperature
        var spectralTypes = new List<(string type, StarClass starClass, float tempMin, float tempMax, float weight)>
        {
            ("O9V", StarClass.O, 30000f, 40000f, 0.001f),
            ("B0V", StarClass.B, 25000f, 30000f, 0.01f),
            ("B5V", StarClass.B, 15000f, 25000f, 0.03f),
            ("A0V", StarClass.A, 9000f, 15000f, 0.06f),
            ("A5V", StarClass.A, 7500f, 9000f, 0.08f),
            ("F0V", StarClass.F, 7000f, 7500f, 0.10f),
            ("F5V", StarClass.F, 6300f, 7000f, 0.12f),
            ("G0V", StarClass.G, 5900f, 6300f, 0.15f),
            ("G5V", StarClass.G, 5500f, 5900f, 0.12f),
            ("K0V", StarClass.K, 5000f, 5500f, 0.10f),
            ("K5V", StarClass.K, 4300f, 5000f, 0.08f),
            ("M0V", StarClass.M, 3800f, 4300f, 0.07f),
            ("M5V", StarClass.M, 3000f, 3800f, 0.05f),
            ("K0III", StarClass.K, 4000f, 5000f, 0.015f),
            ("M0III", StarClass.M, 3500f, 4000f, 0.01f),
        };

        float totalWeight = spectralTypes.Sum(s => s.weight);

        while (brightStars.Count < targetStarCount)
        {
            // Pick a random constellation region
            var region = constellationRegions[rng.Next(constellationRegions.Count)];

            // Generate random position within region
            float ra, dec;
            if (region.raMin > region.raMax) // Wraps around 0/24h
            {
                float range = (24f - region.raMin) + region.raMax;
                float offset = (float)rng.NextDouble() * range;
                ra = (region.raMin + offset) % 24f;
            }
            else
            {
                ra = region.raMin + (float)rng.NextDouble() * (region.raMax - region.raMin);
            }
            dec = region.decMin + (float)rng.NextDouble() * (region.decMax - region.decMin);

            // Pick spectral type based on weighted distribution
            float pick = (float)rng.NextDouble() * totalWeight;
            float cumulative = 0f;
            var selectedType = spectralTypes[0];
            foreach (var st in spectralTypes)
            {
                cumulative += st.weight;
                if (pick <= cumulative)
                {
                    selectedType = st;
                    break;
                }
            }

            // Generate temperature within range
            float temp = selectedType.tempMin + (float)rng.NextDouble() * (selectedType.tempMax - selectedType.tempMin);

            // Generate magnitude (mostly fainter stars visible to naked eye)
            float mag = 2.5f + (float)rng.NextDouble() * 4f; // 2.5 to 6.5 magnitude

            // Brighter stars are less common
            if (rng.NextDouble() < 0.15) mag = 1.5f + (float)rng.NextDouble() * 1.5f; // Some 1.5-3.0 mag stars
            if (rng.NextDouble() < 0.02) mag = 0.5f + (float)rng.NextDouble() * 1.5f; // Rare bright stars

            // Generate star name based on constellation
            string starName = $"HD {100000 + starId}";
            string bayerDes = "";

            // Some stars get Greek letter designations
            if (rng.NextDouble() < 0.3)
            {
                string[] greekLetters = { "α", "β", "γ", "δ", "ε", "ζ", "η", "θ", "ι", "κ", "λ", "μ", "ν", "ξ", "ο", "π", "ρ", "σ", "τ", "υ", "φ", "χ", "ψ", "ω" };
                string[] constellationAbbrevs = { "Ori", "UMa", "Cas", "Cyg", "Sco", "Leo", "Lyr", "And", "Tau", "Gem", "CMa", "Vir", "Boo", "Per", "Her", "Cen", "Car", "Cru", "Aur", "Oph", "Sgr", "Aqr", "Psc", "Cap", "Aql", "Dra", "Cep", "Peg", "Phe", "Gru", "Pav", "Tuc", "Eri", "Hya", "Pup", "Vel", "Lup", "Ara", "CrA", "TrA", "Nor", "Tel", "Ind", "Mic", "Scl", "For", "Hor", "Ret", "Pic", "Dor", "Vol", "Men", "Cha", "Mus", "Cir", "Aps", "Oct", "Hyi", "Cam", "Lyn", "Cnc", "CMi", "Mon", "Lep", "Col", "Cae", "Crv", "Crt", "Sex", "Ant", "Pyx", "Com", "CVn", "UMi", "CrB", "Ser", "Lib", "Sct", "Sge", "Vul", "Del", "Equ", "Lac", "Tri", "Ari" };

                int regionIndex = constellationRegions.IndexOf(region);
                string abbrev = regionIndex < constellationAbbrevs.Length ? constellationAbbrevs[regionIndex] : "XXX";
                bayerDes = $"{greekLetters[rng.Next(greekLetters.Length)]} {abbrev}";
            }

            // Calculate derived properties
            float absM = mag - 5f * Mathf.Log10(10f + (float)rng.NextDouble() * 990f) + 5f;
            float dist = Mathf.Pow(10f, (mag - absM + 5f) / 5f);
            float lum = AstrographicCalculations.CalculateLuminosityFromMagnitude(absM);
            float radius = AstrographicCalculations.CalculateRadiusFromProperties(lum, temp);

            brightStars.Add(new StarData
            {
                name = starName,
                commonName = starName,
                spectralType = selectedType.type,
                bayerDesignation = bayerDes,
                constellation = region.name,
                rightAscension = ra,
                declination = dec,
                magnitude = mag,
                absoluteMagnitude = absM,
                distanceLightYears = dist * 3.26156f,
                distanceParsecs = dist,
                temperature = temp,
                radius = radius,
                mass = Mathf.Pow(lum, 0.25f), // Approximate mass-luminosity relation
                luminosity = lum,
                starClass = selectedType.starClass,
                starType = selectedType.type.Contains("III") ? StarType.Giant :
                           (selectedType.type.Contains("I") && !selectedType.type.Contains("II") && !selectedType.type.Contains("IV")) ? StarType.Supergiant :
                           StarType.MainSequence,
                color = GenerateStarColor(temp)
            });

            starId++;
        }
    }

    private void InitializeConstellations()
    {
        constellations.Add(new ConstellationData { name = "Orion", abbreviation = "Ori", center = new Vector2(5.5f, 0f) });
        constellations.Add(new ConstellationData { name = "Ursa Major", abbreviation = "UMa", center = new Vector2(11.5f, 55f) });
        constellations.Add(new ConstellationData { name = "Cassiopeia", abbreviation = "Cas", center = new Vector2(1f, 60f) });
        constellations.Add(new ConstellationData { name = "Cygnus", abbreviation = "Cyg", center = new Vector2(20.5f, 45f) });
        constellations.Add(new ConstellationData { name = "Scorpius", abbreviation = "Sco", center = new Vector2(16.5f, -30f) });
        constellations.Add(new ConstellationData { name = "Leo", abbreviation = "Leo", center = new Vector2(10.5f, 15f) });
        constellations.Add(new ConstellationData { name = "Lyra", abbreviation = "Lyr", center = new Vector2(18.8f, 36f) });
        constellations.Add(new ConstellationData { name = "Andromeda", abbreviation = "And", center = new Vector2(0.8f, 38f) });
        constellations.Add(new ConstellationData { name = "Taurus", abbreviation = "Tau", center = new Vector2(4.5f, 19f) });
        constellations.Add(new ConstellationData { name = "Gemini", abbreviation = "Gem", center = new Vector2(7.5f, 22f) });
        constellations.Add(new ConstellationData { name = "Canis Major", abbreviation = "CMa", center = new Vector2(6.8f, -22f) });
        constellations.Add(new ConstellationData { name = "Virgo", abbreviation = "Vir", center = new Vector2(13.2f, -2f) });
        constellations.Add(new ConstellationData { name = "Boötes", abbreviation = "Boo", center = new Vector2(14.7f, 30f) });
        constellations.Add(new ConstellationData { name = "Perseus", abbreviation = "Per", center = new Vector2(3.5f, 45f) });
        constellations.Add(new ConstellationData { name = "Hercules", abbreviation = "Her", center = new Vector2(17.5f, 27f) });
        constellations.Add(new ConstellationData { name = "Centaurus", abbreviation = "Cen", center = new Vector2(13.0f, -47f) });
        constellations.Add(new ConstellationData { name = "Carina", abbreviation = "Car", center = new Vector2(8.7f, -62f) });
        constellations.Add(new ConstellationData { name = "Crux", abbreviation = "Cru", center = new Vector2(12.5f, -60f) });
        constellations.Add(new ConstellationData { name = "Auriga", abbreviation = "Aur", center = new Vector2(6.0f, 42f) });
        constellations.Add(new ConstellationData { name = "Ophiuchus", abbreviation = "Oph", center = new Vector2(17.0f, 0f) });
    }

    private void InitializePlanets()
    {
        planets.Clear();
        planets.Add(new PlanetData { name = "Mercury", diameterKm = 4879, distanceFromSunAU = 0.39f, material = planetMaterialMercury });
        planets.Add(new PlanetData { name = "Venus", diameterKm = 12104, distanceFromSunAU = 0.72f, material = planetMaterialVenus });
        planets.Add(new PlanetData { name = "Mars", diameterKm = 6779, distanceFromSunAU = 1.52f, material = planetMaterialMars });
        planets.Add(new PlanetData { name = "Jupiter", diameterKm = 139820, distanceFromSunAU = 5.20f, material = planetMaterialJupiter });
        planets.Add(new PlanetData { name = "Saturn", diameterKm = 116460, distanceFromSunAU = 9.58f, material = planetMaterialSaturn });
        planets.Add(new PlanetData { name = "Uranus", diameterKm = 50724, distanceFromSunAU = 19.18f, material = planetMaterialUranus });
        planets.Add(new PlanetData { name = "Neptune", diameterKm = 49244, distanceFromSunAU = 30.07f, material = planetMaterialNeptune });
        planets.Add(new PlanetData { name = "Pluto", diameterKm = 2376, distanceFromSunAU = 39.48f, material = planetMaterialPluto });

        CreatePlanetObjects();
        CreatePlanetLabels();
    }

    private void CreatePlanetObjects()
    {
        foreach (var planet in planets)
        {
            if (planet.planetObject == null)
            {
                planet.planetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planet.planetObject.name = planet.name;
                float scale = Mathf.Clamp(planet.diameterKm / 12742f, 0.1f, 10f); // Earth = 1.0
                planet.planetObject.transform.localScale = Vector3.one * scale;
                planet.planetObject.transform.position = new Vector3(planet.distanceFromSunAU * 10f, 0, 0); // Simple layout
                var renderer = planet.planetObject.GetComponent<Renderer>();
                if (planet.material != null)
                {
                    renderer.material = planet.material;
                }
                planet.planetObject.SetActive(true);

                // Add Saturn's ring if this is Saturn
                if (planet.name == "Saturn" && saturnRingMaterial != null)
                {
                    GameObject ringObj = new GameObject("SaturnRing");
                    ringObj.transform.SetParent(planet.planetObject.transform, false);
                    ringObj.transform.localPosition = Vector3.zero;
                    ringObj.transform.localRotation = Quaternion.identity;
                    // Create a simple ring mesh (flat disc with hole)
                    // Saturn's rings: inner radius ~74,500 km, outer radius ~136,775 km (relative to planet diameter 116,460 km)
                    float saturnRadius = scale / 2f; // Radius of Saturn sphere
                    float ringInnerRadius = saturnRadius * 1.3f; // 1.3x Saturn's radius
                    float ringOuterRadius = saturnRadius * 2.35f; // 2.35x Saturn's radius (proportional to real Saturn)
                    MeshFilter mf = ringObj.AddComponent<MeshFilter>();
                    MeshRenderer mr = ringObj.AddComponent<MeshRenderer>();
                    mr.material = saturnRingMaterial;
                    mf.mesh = CreateRingMesh(ringInnerRadius, ringOuterRadius, 128); // Inner/outer radius based on Saturn diameter, 128 segments
                }
            }
        }
    }

    private void CreatePlanetLabels()
    {
        planetLabels.Clear();
        if (!showPlanetLabels) return;

        foreach (var planet in planets)
        {
            if (planet.planetObject == null) continue;

            // Create label GameObject
            GameObject labelObj = new GameObject($"Label_{planet.name}");
            labelObj.transform.SetParent(planet.planetObject.transform.parent);
            labelObj.transform.localPosition = planet.planetObject.transform.localPosition + (planet.planetObject.transform.localPosition.normalized * 2f);

            // Create Canvas for label
            Canvas canvas = labelObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * planetLabelCanvasScale;

            // Add background image
            Image background = labelObj.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.6f);

            // Create text object
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(labelObj.transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one;

            // Add TextMeshPro component
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = planet.name;
            text.fontSize = (int)planetLabelFontSize;
            text.color = planetLabelColor;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;

            // Setup RectTransform for text
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(150, 40);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);

            // Add billboard component so label always faces camera
            labelObj.AddComponent<CenteredBillboardText>();

            planetLabels.Add(labelObj);
        }
    }

    // Utility: Create a flat ring mesh (disc with hole)
    private Mesh CreateRingMesh(float innerRadius, float outerRadius, int segments)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle0 = i * angleStep;
            float angle1 = (i + 1) * angleStep;
            Vector3 v0Inner = new Vector3(Mathf.Cos(angle0) * innerRadius, 0, Mathf.Sin(angle0) * innerRadius);
            Vector3 v0Outer = new Vector3(Mathf.Cos(angle0) * outerRadius, 0, Mathf.Sin(angle0) * outerRadius);
            Vector3 v1Inner = new Vector3(Mathf.Cos(angle1) * innerRadius, 0, Mathf.Sin(angle1) * innerRadius);
            Vector3 v1Outer = new Vector3(Mathf.Cos(angle1) * outerRadius, 0, Mathf.Sin(angle1) * outerRadius);
            int baseIndex = vertices.Count;
            vertices.Add(v0Inner); // 0
            vertices.Add(v0Outer); // 1
            vertices.Add(v1Outer); // 2
            vertices.Add(v1Inner); // 3
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            // Two triangles per segment
            triangles.Add(baseIndex + 0);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 0);
        }
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        return mesh;
    }

    private Color GenerateStarColor(float temperature)
    {
        if (!useRealisticStarColors)
        {
            return Color.white;
        }

        temperature = Mathf.Clamp(temperature, 1000, 40000);
        float curveValue = temperatureToColorCurve.Evaluate(temperature);

        if (temperature < 3500) return new Color(1.0f, 0.5f, 0.3f) * curveValue;
        else if (temperature < 5000) return new Color(1.0f, 0.7f, 0.4f) * curveValue;
        else if (temperature < 6000) return new Color(1.0f, 0.9f, 0.6f) * curveValue;
        else if (temperature < 7500) return new Color(1.0f, 1.0f, 0.8f) * curveValue;
        else if (temperature < 10000) return new Color(0.9f, 0.95f, 1.0f) * curveValue;
        else if (temperature < 28000) return new Color(0.7f, 0.8f, 1.0f) * curveValue;
        else return new Color(0.5f, 0.6f, 1.0f) * curveValue;
    }

    private void CreateSingleStar(StarData starData, Transform celestialSphere, float sphereRadius, GameObject prefab)
    {
        GameObject starObj = Instantiate(prefab, celestialSphere);
        starObj.name = $"Star_{starData.name}";
        starObj.SetActive(true);

        float size = CalculateStarSize(starData.magnitude);
        starObj.transform.localScale = Vector3.one * size * starSizeMultiplier;

        Renderer renderer = starObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Use different material for bright stars if enabled
            bool isBrightStar = useDifferentBrightStarMaterial && starData.magnitude < 1f && starMaterialBright != null;
            Material materialToUse = isBrightStar ? starMaterialBright : starMaterial;
            renderer.material = new Material(materialToUse);
            
            renderer.material.color = starData.color;
            renderer.material.EnableKeyword("_EMISSION");
            float emissionIntensity = isBrightStar ? 2.5f : 2f;  // Brighter stars have more emission
            renderer.material.SetColor("_EmissionColor", starData.color * emissionIntensity * colorIntensityMultiplier);
        }

        starObjects.Add(starObj);
    }

    private void CreateLabels(Transform celestialSphere, float sphereRadius)
    {
        if (showStarNames)
        {
            // Select candidates that pass the magnitude threshold, order by magnitude (brightest first),
            // and limit to the inspector-configured maxStarLabels to avoid creating thousands of UI objects.
            var candidates = brightStars
                .Where(s => s.magnitude <= labelShowThreshold)
                .OrderBy(s => s.magnitude)
                .Take(Math.Max(0, maxStarLabels))
                .ToList();

            foreach (var starData in candidates)
            {
                CreateStarLabel(starData, celestialSphere, sphereRadius);
            }
        }

        if (showConstellationNames)
        {
            foreach (var constellation in constellations)
            {
                CreateConstellationLabel(constellation, celestialSphere, sphereRadius);
            }
        }
    }

    private void CreateStarLabel(StarData starData, Transform celestialSphere, float sphereRadius)
    {
        if (starLabelPrefab == null) return;
        
        // Only create labels for brighter stars
        if (starData.magnitude > labelShowThreshold) return;

        GameObject labelObj = Instantiate(starLabelPrefab, celestialSphere);
        labelObj.name = $"Label_{starData.name}";
        labelObj.SetActive(true);

        // Calculate magnitude factor for sizing
        float magnitudeFactor = Mathf.Clamp01((labelShowThreshold - starData.magnitude) / labelShowThreshold);

        TextMeshProUGUI text = labelObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"{starData.name}\n({starData.constellation})";
            text.color = labelColor;

            // Set font size based on star magnitude (brighter stars get larger text)
            float fontSize = Mathf.Lerp(minLabelFontSize, maxLabelFontSize, Mathf.Pow(magnitudeFactor, 1.5f));
            text.fontSize = (int)fontSize;
            
            // Ensure text is centered and readable
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
        }

        // Position label beyond the star, readable from celestial center
        Vector3 starPos = CalculateStarPosition(starData, sphereRadius);
        Vector3 labelDir = starPos.normalized;
        labelObj.transform.localPosition = labelDir * (sphereRadius + labelOffset * labelCameraDistanceFactor);
        
        // Initial canvas scale for readability
        float canvasScale = Mathf.Lerp(minLabelCanvasScale, maxLabelCanvasScale, magnitudeFactor);
        labelObj.transform.localScale = Vector3.one * canvasScale;
        
        starLabels.Add(labelObj);
    }

    private void UpdateLabelScales()
    {
        if (Camera.main == null) return;

        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 celestialCenter = GetCelestialSphereCenter();
        float sphereRadius = GetCelestialSphereRadius();

        foreach (var label in starLabels)
        {
            if (label != null && label.activeInHierarchy)
            {
                TextMeshProUGUI text = label.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    // Calculate distance from camera to label
                    float cameraDistance = Vector3.Distance(cameraPosition, label.transform.position);
                    float sphereDistance = Vector3.Distance(cameraPosition, celestialCenter);
                    
                    // Distance factor for scaling (how far camera is from celestial sphere)
                    float distanceFactor = Mathf.Clamp01(sphereDistance / (sphereRadius * labelDistanceScaleFactor));

                    // Calculate target font size based on current maxLabelFontSize (responds to Inspector changes)
                    float targetFontSize = Mathf.Lerp(minLabelFontSize, maxLabelFontSize, distanceFactor);
                    // Apply immediately instead of interpolating to respond to size changes
                    text.fontSize = (int)targetFontSize;

                    // Adjust label canvas scale based on camera distance for readability
                    float scaleFactor = Mathf.Clamp(cameraDistance / (sphereRadius * 1.5f), 0.3f, 5f);
                    float targetCanvasScale = Mathf.Lerp(minLabelCanvasScale, maxLabelCanvasScale, distanceFactor);
                    Vector3 newScale = Vector3.one * targetCanvasScale * scaleFactor;
                    // Apply immediately instead of interpolating
                    label.transform.localScale = newScale;

                    // Adjust alpha based on distance for better readability
                    Color currentColor = text.color;
                    float alpha = Mathf.Clamp01(1f - (distanceFactor * 0.2f)); // Slightly fade with distance
                    text.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    
                    // Ensure labels are always oriented toward camera (readable from center)
                    if (keepLabelsReadableFromCenter)
                    {
                        Vector3 directionToCamera = (cameraPosition - label.transform.position).normalized;
                        label.transform.LookAt(label.transform.position + directionToCamera);
                    }
                }
            }
        }
        
        // Update constellation labels similarly
        foreach (var label in constellationLabels)
        {
            if (label != null && label.activeInHierarchy)
            {
                TextMeshProUGUI text = label.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    float cameraDistance = Vector3.Distance(cameraPosition, label.transform.position);
                    float sphereDistance = Vector3.Distance(cameraPosition, celestialCenter);
                    float distanceFactor = Mathf.Clamp01(sphereDistance / (sphereRadius * labelDistanceScaleFactor));
                    
                    // Constellation labels are larger, responds to maxLabelFontSize changes
                    float targetFontSize = Mathf.Lerp(maxLabelFontSize * 0.8f, maxLabelFontSize * 1.5f, distanceFactor);
                    // Apply immediately
                    text.fontSize = (int)targetFontSize;
                    
                    float scaleFactor = Mathf.Clamp(cameraDistance / (sphereRadius * 2.0f), 0.4f, 6f);
                    // Apply immediately
                    label.transform.localScale = Vector3.one * (maxLabelCanvasScale * 1.5f) * scaleFactor;
                    
                    if (keepLabelsReadableFromCenter)
                    {
                        Vector3 directionToCamera = (cameraPosition - label.transform.position).normalized;
                        label.transform.LookAt(label.transform.position + directionToCamera);
                    }
                }
            }
        }
    }

    private void CreateConstellationLabel(ConstellationData constellation, Transform celestialSphere, float sphereRadius)
    {
        if (constellationLabelPrefab == null) return;

        GameObject labelObj = Instantiate(constellationLabelPrefab, celestialSphere);
        labelObj.name = $"Constellation_{constellation.name}";
        labelObj.SetActive(true);

        TextMeshProUGUI text = labelObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = constellation.name;
            text.color = constellationLabelColor;
            // Constellation labels are larger and more prominent
            text.fontSize = (int)maxLabelFontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
        }

        Vector3 labelPosition = CalculateStarPosition(
            new StarData { rightAscension = constellation.center.x, declination = constellation.center.y },
            sphereRadius
        );

        // Position slightly further out for visibility
        Vector3 labelDir = labelPosition.normalized;
        labelObj.transform.localPosition = labelDir * (sphereRadius + labelOffset * labelCameraDistanceFactor * 1.2f);
        
        // Larger scale for constellation labels
        labelObj.transform.localScale = Vector3.one * maxLabelCanvasScale * 1.5f;
        
        constellationLabels.Add(labelObj);
    }

    private Vector3 CalculateStarPosition(StarData star, float sphereRadius)
    {
        if (viewFromCelestialCenter)
        {
            // When viewing from celestial center, simply place stars on a sphere
            // based on their right ascension and declination
            
            // Convert right ascension (0-24 hours) to longitude (0-360 degrees)
            float raDegrees = star.rightAscension * 15f; // 1 hour = 15 degrees
            
            // Convert declination (-90 to +90) to latitude (-90 to +90)
            float decDegrees = star.declination;
            
            // Convert spherical coordinates to Cartesian
            float raRad = raDegrees * Mathf.Deg2Rad;
            float decRad = decDegrees * Mathf.Deg2Rad;
            
            // Note: In astronomy, RA increases eastward, so we use -cos for X
            // and declination is measured from celestial equator
            float x = Mathf.Cos(decRad) * Mathf.Cos(raRad);
            float y = Mathf.Sin(decRad);
            float z = Mathf.Cos(decRad) * Mathf.Sin(raRad);
            
            return new Vector3(x, y, z) * sphereRadius;
        }
        else
        {
            // Original Earth-based position calculation
            DateTime currentTime = useSystemTime ? DateTime.UtcNow : customDateTime.ToUniversalTime();
            double lst = CalculateLocalSiderealTime(currentTime, longitude);

            float raDegrees = star.rightAscension * 15f;
            double ha = lst - raDegrees;

            while (ha < 0) ha += 360;
            while (ha >= 360) ha -= 360;

            double haRad = ha * Math.PI / 180.0;
            double decRad = star.declination * Math.PI / 180.0;
            double latRad = latitude * Math.PI / 180.0;

            double sinAlt = Math.Sin(decRad) * Math.Sin(latRad) + Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(haRad);
            sinAlt = Math.Max(-1, Math.Min(1, sinAlt));
            double altRad = Math.Asin(sinAlt);

            double cosAz = (Math.Sin(decRad) - Math.Sin(latRad) * Math.Sin(altRad)) / (Math.Cos(latRad) * Math.Cos(altRad));
            cosAz = Math.Max(-1, Math.Min(1, cosAz));

            double azRad = Math.Acos(cosAz);

            if (Math.Sin(haRad) > 0)
            {
                azRad = 2 * Math.PI - azRad;
            }

            float x = (float)(-Math.Sin(azRad) * Math.Cos(altRad));
            float y = (float)Math.Sin(altRad);
            float z = (float)(-Math.Cos(azRad) * Math.Cos(altRad));

            return new Vector3(x, y, z) * sphereRadius;
        }
    }

    private double CalculateLocalSiderealTime(DateTime utcTime, double longitude)
    {
        double jd = CalculateJulianDate(utcTime);
        double t = (jd - 2451545.0) / 36525.0;
        double gmst = 280.46061837 + 360.98564736629 * (jd - 2451545.0) + 0.000387933 * t * t - t * t * t / 38710000.0;

        gmst = gmst % 360;
        if (gmst < 0) gmst += 360;

        double lst = gmst + longitude;
        lst = lst % 360;
        if (lst < 0) lst += 360;

        return lst;
    }

    private double CalculateJulianDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;
        double hour = date.Hour + date.Minute / 60.0 + date.Second / 3600.0;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        int a = year / 100;
        int b = 2 - a + a / 4;

        double jd = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5 + hour / 24.0;

        return jd;
    }

    public void UpdateStarPositions(float sphereRadius)
    {
        for (int i = 0; i < starObjects.Count; i++)
        {
            StarData starData = null;
            string starName = starObjects[i].name.Replace("Star_", "");

            starData = brightStars.Find(s => s.name == starName);

            if (starData != null)
            {
                Vector3 newPosition = CalculateStarPosition(starData, sphereRadius);
                starObjects[i].transform.localPosition = newPosition;

                GameObject label = starLabels.Find(l => l != null && l.name == $"Label_{starData.name}");
                if (label != null)
                {
                    Vector3 starDirection = newPosition.normalized;
                    label.transform.localPosition = starDirection * (sphereRadius + labelOffset);
                }
            }
        }

        foreach (var label in constellationLabels)
        {
            if (label != null)
            {
                Vector3 currentPos = label.transform.localPosition;
                label.transform.localPosition = currentPos.normalized * (sphereRadius + labelOffset);
            }
        }
    }

    private float CalculateStarSize(float magnitude)
    {
        // Improved magnitude-to-size calculation with sun/moon reference
        if (autoAdjustStarSizeFromRefence)
        {
            // Use reference: magnitude difference determines size
            // Each magnitude unit = ~2.512x brightness difference (inverse relationship for size)
            // Sirius: magnitude -1.46, Rigel: magnitude 0.12, Polaris: magnitude 1.98
            
            float magnitudeDifference = Mathf.Max(magnitude - maxContrastMagnitude, 0f);
            float relativeSize = Mathf.Pow(2.512f, -magnitudeDifference / 2.5f) * sunRelativeSize;
            
            return Mathf.Clamp(relativeSize, minStarSize, maxStarSize);
        }
        else
        {
            // Original magnitude-based scaling
            float normalizedMagnitude = Mathf.Clamp01((magnitude + 2f) / 10f);
            return Mathf.Lerp(maxStarSize, minStarSize, Mathf.Pow(normalizedMagnitude, magnitudeExponent));
        }
    }

    private void ClearAllObjects()
    {
        foreach (var star in starObjects) { if (star != null) Destroy(star); }
        foreach (var label in starLabels) { if (label != null) Destroy(label); }
        foreach (var label in constellationLabels) { if (label != null) Destroy(label); }

        starObjects.Clear();
        starLabels.Clear();
        constellationLabels.Clear();
    }

    public void ResetToDefaultView()
    {
        starSizeMultiplier = 1.0f;
        viewFromCelestialCenter = true; // Default to viewing from center
        showStarNames = true;
        showConstellationNames = true;
        showGUI = true;
        useRealisticStarColors = true;
        colorIntensityMultiplier = 2f;
        autoScaleLabels = true;
        RefreshAllStars(transform, celestialSphereRadius);
    }

    public float GetCelestialSphereRadius()
    {
        return celestialSphereRadius;
    }
    
    public Vector3 GetCelestialSphereCenter()
    {
        // Return the center of the celestial sphere (the parent of stars, usually the camera position's target)
        if (celestialSphereController != null)
        {
            return celestialSphereController.transform.position;
        }
        return Vector3.zero;
    }

    public void ToggleStarNames(bool show)
    {
        showStarNames = show;
        RefreshLabels();
    }

    public void ToggleConstellationNames(bool show)
    {
        showConstellationNames = show;
        RefreshLabels();
    }

    public void ToggleRealisticStarColors(bool useRealistic)
    {
        useRealisticStarColors = useRealistic;
        RefreshAllStars(transform, celestialSphereRadius);
    }

    public void ToggleAutoScaleLabels(bool autoScale)
    {
        autoScaleLabels = autoScale;
    }

    public void ToggleViewMode(bool viewFromCenter)
    {
        viewFromCelestialCenter = viewFromCenter;
        RefreshAllStars(transform, celestialSphereRadius);
    }

    private void RefreshLabels()
    {
        if (starObjects.Count > 0)
        {
            Transform parent = starObjects[0].transform.parent;
            float radius = celestialSphereRadius;

            foreach (var label in starLabels) { if (label != null) Destroy(label); }
            foreach (var label in constellationLabels) { if (label != null) Destroy(label); }

            starLabels.Clear();
            constellationLabels.Clear();

            CreateLabels(parent, radius);
        }
    }

    private void UpdateStarSizes()
    {
        foreach (var starObj in starObjects)
        {
            if (starObj != null)
            {
                string starName = starObj.name.Replace("Star_", "");
                StarData starData = brightStars.Find(s => s.name == starName);
                if (starData != null)
                {
                    float size = CalculateStarSize(starData.magnitude);
                    starObj.transform.localScale = Vector3.one * size * starSizeMultiplier;
                }
            }
        }
    }

    public List<StarData> GetBrightStars() { return brightStars; }
    public void RefreshAllStars(Transform celestialSphere, float sphereRadius) { 
        celestialSphereRadius = sphereRadius;
        CreateStarObjects(celestialSphere, sphereRadius); 
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(300), GUILayout.Height(Screen.height - 20));

        GUILayout.Label("STAR & GALAXY VIEWER", GUILayout.Height(30));
        GUILayout.Space(10);

        // View Mode Toggle
        GUILayout.Label("View Mode", GUILayout.Height(25));
        GUILayout.BeginVertical("box");
        viewFromCelestialCenter = GUILayout.Toggle(viewFromCelestialCenter, "View from Celestial Center (All Stars Visible)");
        if (!viewFromCelestialCenter)
        {
            GUILayout.Label("Viewing from Earth Position");
        }
        GUILayout.Label("Celestial Sphere Radius: " + celestialSphereRadius.ToString("F1"));
        celestialSphereRadius = GUILayout.HorizontalSlider(celestialSphereRadius, 10f, 100f);
        if (GUILayout.Button("Toggle View Mode"))
        {
            ToggleViewMode(!viewFromCelestialCenter);
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // Star Controls
        showStarControls = GUILayout.Toggle(showStarControls, "★ Star Controls", "foldout");
        if (showStarControls)
        {
            GUILayout.BeginVertical("box");
            showStarNames = GUILayout.Toggle(showStarNames, "Show Star Names");
            showConstellationNames = GUILayout.Toggle(showConstellationNames, "Show Constellation Names");
            useRealisticStarColors = GUILayout.Toggle(useRealisticStarColors, "Use Realistic Star Colors");
            autoScaleLabels = GUILayout.Toggle(autoScaleLabels, "Auto Scale Labels");
            GUILayout.Space(5);
            GUILayout.Label("Star Size Multiplier: " + starSizeMultiplier.ToString("F2"));
            starSizeMultiplier = GUILayout.HorizontalSlider(starSizeMultiplier, 0.1f, 3f);
            GUILayout.Label("Color Intensity: " + colorIntensityMultiplier.ToString("F2"));
            colorIntensityMultiplier = GUILayout.HorizontalSlider(colorIntensityMultiplier, 0.5f, 5f);
            GUILayout.Label("Min Label Size: " + minLabelFontSize.ToString("F1"));
            minLabelFontSize = GUILayout.HorizontalSlider(minLabelFontSize, 4f, 12f);
            GUILayout.Label("Max Label Size: " + maxLabelFontSize.ToString("F1"));
            maxLabelFontSize = GUILayout.HorizontalSlider(maxLabelFontSize, 16f, 48f);
            if (GUILayout.Button("Refresh Stars"))
            {
                RefreshAllStars(transform, celestialSphereRadius);
            }
            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        // Planet Controls
        GUILayout.Label("Planet Controls", GUILayout.Height(25));
        GUILayout.BeginVertical("box");
        showPlanetLabels = GUILayout.Toggle(showPlanetLabels, "Show Planet Labels");
        GUILayout.Label("Planet Label Size: " + planetLabelFontSize.ToString("F1"));
        planetLabelFontSize = GUILayout.HorizontalSlider(planetLabelFontSize, 8f, 32f);
        GUILayout.Label("Planets: " + planets.Count);
        if (GUILayout.Button("Refresh Planets"))
        {
            CreatePlanetLabels();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // Location Controls
        showLocationControls = GUILayout.Toggle(showLocationControls, "Location Controls", "foldout");
        if (showLocationControls)
        {
            GUILayout.BeginVertical("box");
            useCelestialSphereLocation = GUILayout.Toggle(useCelestialSphereLocation, "Use Celestial Sphere Location");
            if (!useCelestialSphereLocation)
            {
                GUILayout.Label("Latitude: " + latitude.ToString("F4"));
                latitude = GUILayout.HorizontalSlider(latitude, -90f, 90f);
                GUILayout.Label("Longitude: " + longitude.ToString("F4"));
                longitude = GUILayout.HorizontalSlider(longitude, -180f, 180f);
            }
            useSystemTime = GUILayout.Toggle(useSystemTime, "Use System Time");
            if (GUILayout.Button("Update Positions"))
            {
                UpdateStarPositions(celestialSphereRadius);
            }
            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        // Quick Actions
        GUILayout.Label("Quick Actions", GUILayout.Height(25));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset View"))
        {
            ResetToDefaultView();
        }
        if (GUILayout.Button("Toggle GUI"))
        {
            showGUI = !showGUI;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Status Information
        GUILayout.Label("Status Information", GUILayout.Height(25));
        GUILayout.BeginVertical("box");
        GUILayout.Label($"Stars Rendered: {starObjects.Count}/{brightStars.Count}");
        GUILayout.Label($"Star Labels: {starLabels.Count}");
        GUILayout.Label($"Planets Rendered: {planets.Count}");
        GUILayout.Label($"Planet Labels: {planetLabels.Count}");
        GUILayout.Label($"Constellations: {constellations.Count}");
        GUILayout.Label($"View Mode: {(viewFromCelestialCenter ? "Celestial Center" : "Earth Position")}");
        GUILayout.Label($"Location: Lat {latitude:F2}°, Lon {longitude:F2}°");
        GUILayout.Label($"Time: {(useSystemTime ? "System Time" : "Custom Time")}");
        GUILayout.Label($"Star Colors: {(useRealisticStarColors ? "Realistic" : "White")}");
        GUILayout.Label($"Label Scaling: {(autoScaleLabels ? "Auto" : "Fixed")}");
        GUILayout.EndVertical();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // Runtime check to catch and fix any materials with incompatible shaders
    private float lastIncompatibleShaderCheck = 0f;
    private void LateUpdate()
    {
        // Check every 2 seconds for any late-created materials with incompatible shaders
        if (Time.time - lastIncompatibleShaderCheck > 2f)
        {
            lastIncompatibleShaderCheck = Time.time;
            
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            
            if (urpUnlit != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    try
                    {
                        Material[] mats = renderer.sharedMaterials;
                        bool needsUpdate = false;
                        
                        for (int i = 0; i < mats.Length; i++)
                        {
                            if (mats[i] != null && mats[i].shader != null)
                            {
                                string shaderName = mats[i].shader.name;
                                if (shaderName.Contains("Simulation/Standard") || shaderName.Contains("Standard Lit"))
                                {
                                    Debug.LogError($"[LateUpdate] INCOMPATIBLE SHADER DETECTED: {renderer.gameObject.name} - {shaderName}");
                                    Material newMat = new Material(urpUnlit);
                                    newMat.name = mats[i].name + " (Fixed-Late)";
                                    
                                    if (mats[i].HasProperty("_Color"))
                                        newMat.SetColor("_BaseColor", mats[i].GetColor("_Color"));
                                    if (mats[i].HasProperty("_MainTex"))
                                        newMat.SetTexture("_BaseMap", mats[i].GetTexture("_MainTex"));
                                    
                                    mats[i] = newMat;
                                    needsUpdate = true;
                                }
                            }
                        }
                        
                        if (needsUpdate)
                        {
                            renderer.sharedMaterials = mats;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[LateUpdate] Error checking renderer {renderer.gameObject.name}: {ex.Message}");
                    }
                }
            }
        }
    }

    // Cleanup
    private void OnDestroy()
    {
        if (starMaterial != null) Destroy(starMaterial);
        if (starPrefab != null) Destroy(starPrefab);
        if (starLabelPrefab != null) Destroy(starLabelPrefab);
        if (constellationLabelPrefab != null) Destroy(constellationLabelPrefab);
    }
}

// Centered Billboard component for labels
public class CenteredBillboardText : MonoBehaviour
{
    private Transform centerPoint;

    private void Start()
    {
        centerPoint = transform.parent;
        if (centerPoint == null)
        {
            centerPoint = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (centerPoint != null)
        {
            transform.LookAt(2 * transform.position - centerPoint.position);
        }
    }
}