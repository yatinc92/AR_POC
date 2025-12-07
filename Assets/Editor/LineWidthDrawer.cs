using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CelestialSphereController))]
public class CelestialSphereControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CelestialSphereController controller = (CelestialSphereController)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Update Line Widths (Runtime)"))
        {
            controller.UpdateGridLineWidths();
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("Change line width values above and click the button to update at runtime, or they will update automatically on scene start.", MessageType.Info);
    }
}