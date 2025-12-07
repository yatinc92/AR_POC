# Quick Reference: Star Visibility Settings

## 🎯 Key Parameters to Adjust

### **For Better Star Visibility**
```
starSizeMultiplier = 1.5 - 2.0          // Increase from 1.0
maxStarSize = 1.0 - 1.5                 // Increase from 0.8
labelShowThreshold = 4.0 - 5.0          // Show more labels
```

### **For Better Label Readability**
```
minLabelFontSize = 10 - 12              // Minimum readable size
maxLabelFontSize = 32 - 40              // Increase from 24
labelCameraDistanceFactor = 3.0 - 4.0   // Labels further away
keepLabelsReadableFromCenter = true     // Always face camera
```

### **For Accurate Astronomical Display**
```
autoAdjustStarSizeFromRefence = true    // Use Sun/Moon reference
adjustStarSizeBasedOnMagnitude = true   // Magnitude-based sizing
maxContrastMagnitude = 2.0              // Reference magnitude
sunRelativeSize = 1.0                   // Sun as reference (1x)
```

---

## 📊 Magnitude Reference Chart

| Star | Magnitude | Relative Size | Visibility |
|------|-----------|---------------|------------|
| Sirius | -1.46 | ~3.5x | Brightest ✨✨✨ |
| Canopus | -0.74 | ~2.1x | Very Bright ✨✨ |
| Vega | 0.03 | ~1.5x | Bright ✨ |
| Rigel | 0.12 | ~1.4x | Bright ✨ |
| Polaris | 1.98 | ~0.6x | Medium ⭐ |
| Alcor | 4.0 | ~0.15x | Dim • |
| Faint Naked Eye | 6.5 | ~0.05x | Very Dim • |

*Sizes shown are relative to `maxContrastMagnitude` (2.0)*

---

## 🔧 Preset Configurations

### **Preset 1: Mobile AR**
```csharp
// Best for phones/tablets - highly visible
starSizeMultiplier = 2.0f;
minStarSize = 0.2f;
maxStarSize = 1.0f;
minLabelFontSize = 12f;
maxLabelFontSize = 40f;
labelShowThreshold = 4.0f;
labelCameraDistanceFactor = 3.5f;
autoAdjustStarSizeFromRefence = true;
keepLabelsReadableFromCenter = true;
```

### **Preset 2: Desktop Planetarium**
```csharp
// High accuracy, professional appearance
starSizeMultiplier = 1.0f;
minStarSize = 0.05f;
maxStarSize = 0.6f;
minLabelFontSize = 8f;
maxLabelFontSize = 24f;
labelShowThreshold = 2.5f;
labelCameraDistanceFactor = 2.5f;
autoAdjustStarSizeFromRefence = true;
adjustStarSizeBasedOnMagnitude = true;
```

### **Preset 3: Education (Constellations Only)**
```csharp
// Clean view, focus on constellations
showStarNames = false;
showConstellationNames = true;
starSizeMultiplier = 1.2f;
labelShowThreshold = 1.0f;
autoAdjustStarSizeFromRefence = true;
```

---

## 🎨 Label Customization

### **Make Labels Larger**
```csharp
minLabelFontSize = 14f;      // Minimum readable
maxLabelFontSize = 48f;      // Maximum size
maxLabelCanvasScale = 0.08f; // Larger canvas
```

### **Make Labels Smaller (Less Clutter)**
```csharp
minLabelFontSize = 6f;
maxLabelFontSize = 16f;
minLabelCanvasScale = 0.003f;
maxLabelCanvasScale = 0.02f;
```

### **Change Label Colors**
```csharp
labelColor = new Color(1f, 0.8f, 0.3f, 0.9f);          // Golden
labelColor = new Color(0.2f, 0.8f, 1f, 0.9f);          // Cyan
constellationLabelColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Red
```

---

## 🚀 Runtime Adjustment Code

### **Adjust at Runtime**
```csharp
// Make stars bigger
StarDatabase.Instance.starSizeMultiplier = 2.0f;
StarDatabase.Instance.RefreshAllStars(transform, StarDatabase.Instance.GetCelestialSphereRadius());

// Show more labels
StarDatabase.Instance.labelShowThreshold = 4.0f;
StarDatabase.Instance.RefreshLabels();

// Adjust font size
StarDatabase.Instance.minLabelFontSize = 12f;
StarDatabase.Instance.maxLabelFontSize = 40f;
```

### **Platform-Specific Setup**
```csharp
#if UNITY_ANDROID || UNITY_IOS
    // Mobile: Larger, more visible
    StarDatabase.Instance.starSizeMultiplier = 1.8f;
    StarDatabase.Instance.minLabelFontSize = 14f;
    StarDatabase.Instance.maxLabelCanvasScale = 0.08f;
#elif UNITY_STANDALONE
    // Desktop: Accurate, professional
    StarDatabase.Instance.starSizeMultiplier = 1.0f;
    StarDatabase.Instance.minLabelFontSize = 8f;
    StarDatabase.Instance.adjustStarSizeBasedOnMagnitude = true;
#endif
```

---

## 📱 Mobile-Specific Tips

1. **For Small Screens (<5 inches)**:
   - Increase `starSizeMultiplier` to 2.0-2.5
   - Increase `minLabelFontSize` to 12-14
   - Set `labelShowThreshold` to 4.0

2. **For Large Screens (>6 inches)**:
   - Use normal settings (1.0-1.5 multiplier)
   - Moderate font sizes (8-24)
   - Standard thresholds

3. **For Poor Visibility (Bright Sunlight)**:
   - Increase `colorIntensityMultiplier` to 3.0-4.0
   - Increase `starSizeMultiplier` to 2.0+
   - Add slight glow effect via material

---

## 🐛 Troubleshooting

### **Stars too small**
→ Increase `starSizeMultiplier` and `maxStarSize`

### **Stars too large**
→ Decrease `starSizeMultiplier` or `maxStarSize`

### **Labels unreadable**
→ Increase `maxLabelFontSize` and `maxLabelCanvasScale`

### **Too many labels cluttering screen**
→ Increase `labelShowThreshold` (shows only brightest stars)

### **Labels not facing camera**
→ Ensure `keepLabelsReadableFromCenter = true`

### **Incorrect star sizes**
→ Enable `autoAdjustStarSizeFromRefence` and set `sunRelativeSize`

---

## 📈 Performance Optimization

### **If FPS is Low**
```csharp
// Reduce update frequency
autoScaleLabels = false;                    // Disable dynamic scaling
keepLabelsReadableFromCenter = false;       // Disable camera facing
showConstellationNames = false;             // Reduce label count
labelShowThreshold = 1.0f;                  // Show only brightest

// Alternatively, adjust label update in Update():
if (Time.deltaTime > 0.016f) return;        // Skip if frame rate drops
```

### **If Memory is Tight**
```csharp
// Reduce visible objects
showStarNames = false;                      // No star labels
showConstellationNames = true;              // Only constellations
// Total labels: ~20 vs ~100
```

---

## ✨ Visual Hierarchy

After implementing these improvements, the visual hierarchy is:

```
Brightest           Dimmest
  Sirius              Polaris
    ↑                   ↓
    |                   |
  LARGEST            SMALLEST
    |                   |
    ↑                   ↓
CLEAREST LABELS   NO LABELS
    (Full Font)     (Filtered)
```

---

## 📚 Related Classes/Methods

- `CalculateStarSize()` - Size calculation engine
- `CreateStarLabel()` - Label creation with filtering
- `UpdateLabelScales()` - Real-time label adjustment
- `CreateSingleStar()` - Individual star instantiation
- `GetCelestialSphereCenter()` - Center position helper
- `RefreshLabels()` - Rebuild all labels
- `RefreshAllStars()` - Rebuild all stars

---

## 🎓 Educational Value

### **For Astronomy Learning**:
- Accurate magnitude-to-size relationship (2.512x per magnitude)
- Proper brightness hierarchy (visual magnitude system)
- Sun/Moon reference scale for context
- Real star data with 50 brightest stars

### **For AR/VR Development**:
- Camera-facing label techniques
- Dynamic scaling systems
- Distance-aware UI
- Real-time adjustment patterns

