# Star Materials Inspector Guide

## Overview
Star materials can now be fully controlled from the Unity Inspector, allowing you to:
- Use auto-generated materials (default)
- Assign custom materials from the project
- Use different materials for bright vs. normal stars
- Adjust all material properties without code changes

---

## Inspector Fields

### **Star Materials Section**

#### **starMaterial** (Private, Default Material)
- **Type**: Material
- **Default**: Auto-generated (URP Unlit or Standard shader)
- **What it does**: Main material used for all stars if no custom material is assigned
- **When to modify**: Leave empty to auto-generate; don't drag here directly

#### **starMaterialCustom** (Custom Material Override)
- **Type**: Material
- **Default**: None (empty)
- **How to use**:
  1. Create or select a material in your project
  2. Drag it into this field
  3. Enable `useCustomStarMaterial` toggle
- **Effect**: Overrides the auto-generated material for all stars
- **Use cases**: 
  - Custom shaders for special effects
  - Specific star appearance matching your art style
  - Glow/bloom effects from a custom material

#### **useCustomStarMaterial** (Toggle)
- **Type**: Boolean (Checkbox)
- **Default**: false (unchecked)
- **What it does**: 
  - **Checked**: Uses `starMaterialCustom` for all stars
  - **Unchecked**: Uses auto-generated `starMaterial`
- **How to enable**:
  1. Check the checkbox
  2. Assign a material to `starMaterialCustom`
  3. Stars will update to use the custom material

#### **starMaterialBright** (Optional Bright Star Material)
- **Type**: Material
- **Default**: None (empty)
- **How to use**:
  1. Create a material with brighter/more emissive properties
  2. Drag it into this field
  3. Enable `useDifferentBrightStarMaterial` toggle
- **Effect**: Applies only to stars with magnitude < 1 (brightest stars)
- **Stars affected**: Sirius, Canopus, Rigil Kentaurus, Arcturus, Vega, Capella, etc.
- **Use cases**:
  - Make brightest stars visually distinct
  - Extra glow/bloom on prominent stars
  - Different color scheme for very bright objects

#### **useDifferentBrightStarMaterial** (Toggle)
- **Type**: Boolean (Checkbox)
- **Default**: false (unchecked)
- **What it does**:
  - **Checked**: Stars with mag < 1 use `starMaterialBright`; others use main material
  - **Unchecked**: All stars use same material
- **Requirements**: 
  - Must have a material assigned to `starMaterialBright`
  - Works independently of `useCustomStarMaterial`

---

## Workflow: Using Custom Materials

### **Scenario 1: All Stars Use Custom Material**

```
1. Create or select a material (e.g., "StarMaterial_Custom")
2. In StarDatabase Inspector:
   - Drag material into "starMaterialCustom" field
   - Check "useCustomStarMaterial" checkbox
3. All stars now use this material
```

### **Scenario 2: Bright Stars Look Different**

```
1. Create two materials:
   - "StarMaterial_Normal" (normal stars)
   - "StarMaterial_Bright" (very bright stars, higher emission)
2. In StarDatabase Inspector:
   - Leave "starMaterialCustom" empty (or assign "StarMaterial_Normal")
   - Assign "StarMaterial_Bright" to "starMaterialBright" field
   - Check "useDifferentBrightStarMaterial" checkbox
3. Result:
   - Sirius, Canopus, etc. use bright material (more glow)
   - Other stars use main material
```

### **Scenario 3: Custom + Different Bright Stars**

```
1. Create two materials:
   - "StarMaterial_Custom_Normal"
   - "StarMaterial_Custom_Bright"
2. In StarDatabase Inspector:
   - Assign "StarMaterial_Custom_Normal" to "starMaterialCustom"
   - Assign "StarMaterial_Custom_Bright" to "starMaterialBright"
   - Check "useCustomStarMaterial" checkbox
   - Check "useDifferentBrightStarMaterial" checkbox
3. Result:
   - Custom materials used
   - Bright stars have distinct appearance
```

---

## Material Creation Tips

### **Creating a Star Material**

1. **In Project**: Right-click → Create → Material
2. **Name it**: "StarMaterial_Custom"
3. **Assign shader**:
   - Recommended: "Universal Render Pipeline/Unlit" (for emissive effect)
   - Or: "Standard" (for physics-based rendering)
4. **Configure**:
   - **Base Color**: White (stars are colored by StarDatabase code)
   - **Emission**: Enable + white color (for glow)
   - **Metallic**: 0 (non-metallic)
   - **Smoothness**: 0 (rough, matte surface)

### **Creating a Bright Star Material**

```
Same as above, but:
- Increase Emission color intensity
- Or use HDR color for overbright appearance
- Consider adding Bloom post-processing for effect
```

### **Creating a Glowing Star Material**

```
- Shader: "Universal Render Pipeline/Unlit"
- Base Map: White
- Emission: Enabled, bright white (1, 1, 1, 1) or HDR
- Global Illumination: Realtime Emissive (auto-set by code)
- This creates a glowing effect, especially with HDR enabled
```

---

## Code Reference

### **How Material Assignment Works**

The code prioritizes materials in this order:

```csharp
1. For all stars (main creation):
   IF useCustomStarMaterial == true AND starMaterialCustom != null
      → Use starMaterialCustom
   ELSE
      → Use auto-generated starMaterial

2. For bright stars (if enabled):
   IF useDifferentBrightStarMaterial == true 
      AND magnitude < 1.0 
      AND starMaterialBright != null
      → Use starMaterialBright
   ELSE
      → Use main material (custom or auto-generated)
```

### **Emission Intensity**

Bright stars get 1.25x emission boost:
```csharp
float emissionIntensity = isBrightStar ? 2.5f : 2f;
// Applied to color and emission color
```

---

## Material Properties Automatically Applied

Regardless of which material you use, the code applies these properties:

```csharp
// Color (set per-star based on temperature)
renderer.material.color = starData.color;

// Emission (set per-star based on brightness)
renderer.material.EnableKeyword("_EMISSION");
renderer.material.SetColor("_EmissionColor", starData.color * intensity);

// Global Illumination
material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
```

**This means**: Your custom material acts as a base; star color and emission are always applied dynamically.

---

## Troubleshooting

### **"Custom material not showing"**
- ✓ Check `useCustomStarMaterial` is checked
- ✓ Verify material is assigned to `starMaterialCustom`
- ✓ Try clicking Play, then Stop, then check Inspector

### **"Bright star material not showing"**
- ✓ Check `useDifferentBrightStarMaterial` is checked
- ✓ Verify material is assigned to `starMaterialBright`
- ✓ Remember: only applies to stars with magnitude < 1

### **"Stars look wrong after changing material"**
- The script always applies `EnableKeyword("_EMISSION")`
- Make sure your shader supports the `_EMISSION` keyword
- Use URP or Standard shaders (both support this)

### **"Material doesn't save between plays"**
- Auto-generated materials are runtime-only
- Only custom materials assigned from Inspector persist
- This is expected behavior

---

## Performance Notes

- **No performance difference** between auto and custom materials
- **Different materials per star type** adds negligible overhead
- All 50 stars can use different materials with no FPS impact

---

## Examples in Code

### **Access materials in code**
```csharp
// Get the star database
StarDatabase db = StarDatabase.Instance;

// Check current settings
if (db.useCustomStarMaterial)
{
    Debug.Log("Using custom material: " + db.starMaterialCustom.name);
}

// Change at runtime
db.useCustomStarMaterial = true;
db.starMaterialCustom = Resources.Load<Material>("MyMaterials/StarMaterial");
```

### **Apply material to all stars**
```csharp
StarDatabase.Instance.useCustomStarMaterial = true;
StarDatabase.Instance.starMaterialCustom = myCustomMaterial;
StarDatabase.Instance.RefreshAllStars(transform, sphereRadius);
```

---

## Best Practices

1. **Keep auto-generation**: Leave materials empty for automatic, optimal setup
2. **Use custom for special effects**: Only override for specific visual needs
3. **Test on target platform**: Materials may look different on mobile
4. **Keep materials simple**: Star geometry is a sphere; complex shaders may look odd
5. **Use HDR colors**: For emission, HDR colors create better glowing effects

---

## Related Settings

See also:
- `colorIntensityMultiplier` - Controls emission brightness
- `useRealisticStarColors` - Affects star color application
- `maxStarSize` - Affects visual prominence
- `starSizeMultiplier` - Affects visual prominence

